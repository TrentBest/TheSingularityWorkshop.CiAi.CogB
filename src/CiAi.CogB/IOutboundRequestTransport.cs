namespace CiAi.CogB;

/// <summary>Transport boundary. Implementations must not be invoked until validation and host authorization pass.</summary>
public interface IOutboundRequestTransport
{
    Task<TransportReceipt> SendAsync(OutboundRequest request, CancellationToken cancellationToken = default);
}

/// <summary>Receipt that proves only what the transport accepted, not that the target succeeded.</summary>
public sealed record TransportReceipt(string RequestId, string CorrelationId, string TransportStatus, string? EvidenceUrl = null);
