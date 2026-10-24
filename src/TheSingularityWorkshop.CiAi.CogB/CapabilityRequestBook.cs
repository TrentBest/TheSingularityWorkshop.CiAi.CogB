using System.Security.Cryptography;
using System.Text;

namespace TheSingularityWorkshop.CiAi.CogB;

/// <summary>A request pinned to a provider, capability version, source revision, and input digest.</summary>
public sealed class CapabilityRequest
{
    public CapabilityRequest(string requestId, string operationId, string idempotencyKey,
        string requesterIdentity, string capabilityId, string capabilityVersion, string providerRepository,
        string sourceRepository, string sourceRevision, string inputDigest, int contractVersion,
        long maximumOutputBytes, DateTimeOffset createdAt, DateTimeOffset expiresAt)
    {
        RequestId = Token(requestId, nameof(requestId));
        OperationId = Token(operationId, nameof(operationId));
        IdempotencyKey = Token(idempotencyKey, nameof(idempotencyKey));
        RequesterIdentity = Token(requesterIdentity, nameof(requesterIdentity));
        CapabilityId = Token(capabilityId, nameof(capabilityId), true);
        CapabilityVersion = Token(capabilityVersion, nameof(capabilityVersion));
        ProviderRepository = Repository(providerRepository, nameof(providerRepository));
        SourceRepository = Repository(sourceRepository, nameof(sourceRepository));
        SourceRevision = Hex(sourceRevision, 40, nameof(sourceRevision));
        InputDigest = Hex(inputDigest, 64, nameof(inputDigest));
        if (contractVersion < 1) throw new ArgumentOutOfRangeException(nameof(contractVersion));
        if (maximumOutputBytes is < 1 or > 67_108_864) throw new ArgumentOutOfRangeException(nameof(maximumOutputBytes));
        if (expiresAt <= createdAt) throw new ArgumentException("Expiry must be later than creation.", nameof(expiresAt));
        ContractVersion = contractVersion;
        MaximumOutputBytes = maximumOutputBytes;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    /// <summary>Stable request identity.</summary>
    public string RequestId { get; }
    /// <summary>Stable operation identity.</summary>
    public string OperationId { get; }
    /// <summary>Retry-safe idempotency key scoped to the requester.</summary>
    public string IdempotencyKey { get; }
    /// <summary>Authenticated identity of the requesting coordinator.</summary>
    public string RequesterIdentity { get; }
    /// <summary>Requested capability identifier.</summary>
    public string CapabilityId { get; }
    /// <summary>Exact capability version requested.</summary>
    public string CapabilityVersion { get; }
    /// <summary>Provider repository identity.</summary>
    public string ProviderRepository { get; }
    /// <summary>Source repository whose content is input to the operation.</summary>
    public string SourceRepository { get; }
    /// <summary>Full pinned source commit SHA.</summary>
    public string SourceRevision { get; }
    /// <summary>SHA-256 digest of the pinned input bundle.</summary>
    public string InputDigest { get; }
    /// <summary>Request contract version.</summary>
    public int ContractVersion { get; }
    /// <summary>Maximum accepted output size in bytes; constrained to 64 MiB in this version.</summary>
    public long MaximumOutputBytes { get; }
    /// <summary>Request creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; }
    /// <summary>Request expiry timestamp.</summary>
    public DateTimeOffset ExpiresAt { get; }

    internal string IdempotencyIdentity => $"{RequesterIdentity}\n{IdempotencyKey}";
    internal string Fingerprint => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n",
        OperationId, RequesterIdentity, CapabilityId, CapabilityVersion, ProviderRepository,
        SourceRepository, SourceRevision, InputDigest, ContractVersion, MaximumOutputBytes,
        CreatedAt.ToUniversalTime().ToString("O"), ExpiresAt.ToUniversalTime().ToString("O"))))).ToLowerInvariant();

    private static string Token(string value, string parameter, bool dots = false)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128 ||
            value.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_' || (dots && c == '.'))))
            throw new ArgumentException("Value must be a bounded identifier.", parameter);
        return value;
    }

    private static string Repository(string value, string parameter)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Repository is required.", parameter);
        var parts = value.Split('/');
        if (parts.Length != 2 || parts.Any(p => p.Length is < 1 or > 100 ||
            p.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.'))))
            throw new ArgumentException("Repository must be owner/repository.", parameter);
        return value;
    }

    private static string Hex(string value, int length, string parameter)
    {
        if (value is null || value.Length != length || !value.All(Uri.IsHexDigit))
            throw new ArgumentException($"Value must be {length} hexadecimal characters.", parameter);
        return value.ToLowerInvariant();
    }
}

/// <summary>In-memory request ledger with idempotency conflict detection; not a dispatcher or durable store.</summary>
public sealed class CapabilityRequestBook
{
    private readonly object _gate = new();
    private readonly Dictionary<string, CapabilityRequest> _byRequestId = new(StringComparer.Ordinal);
    private readonly Dictionary<string, CapabilityRequest> _byIdempotency = new(StringComparer.Ordinal);

    /// <summary>Number of distinct requests recorded.</summary>
    public int Count { get { lock (_gate) return _byRequestId.Count; } }

    /// <summary>Records a request or returns the original for an identical retry.</summary>
    public CapabilityRequest Submit(CapabilityRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        lock (_gate)
        {
            if (_byIdempotency.TryGetValue(request.IdempotencyIdentity, out var existing))
            {
                if (existing.Fingerprint != request.Fingerprint)
                    throw new InvalidOperationException("Idempotency key was reused with different pinned inputs.");
                return existing;
            }
            if (_byRequestId.TryGetValue(request.RequestId, out var byId))
            {
                if (byId.Fingerprint != request.Fingerprint)
                    throw new InvalidOperationException("Request ID was reused with different inputs.");
                return byId;
            }
            _byRequestId.Add(request.RequestId, request);
            _byIdempotency.Add(request.IdempotencyIdentity, request);
            return request;
        }
    }

    /// <summary>Finds a request by stable request ID.</summary>
    public bool TryGet(string requestId, out CapabilityRequest? request)
    {
        lock (_gate) return _byRequestId.TryGetValue(requestId, out request);
    }

    /// <summary>Returns a sorted read-only snapshot of recorded requests.</summary>
    public IReadOnlyList<CapabilityRequest> Snapshot()
    {
        lock (_gate) return Array.AsReadOnly(_byRequestId.Values.OrderBy(x => x.CreatedAt).ThenBy(x => x.RequestId, StringComparer.Ordinal).ToArray());
    }
}
