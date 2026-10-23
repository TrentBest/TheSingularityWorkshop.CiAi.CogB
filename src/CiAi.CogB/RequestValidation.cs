namespace CiAi.CogB;

/// <summary>Deterministic validation before any transport can be called.</summary>
public static class RequestValidation
{
    public static IReadOnlyList<string> Validate(OutboundRequest? request, ISet<string> allowedTargets, ISet<string> allowedOperations)
    {
        ArgumentNullException.ThrowIfNull(allowedTargets);
        ArgumentNullException.ThrowIfNull(allowedOperations);
        var errors = new List<string>();
        if (request is null) { errors.Add("request.required"); return errors; }

        CheckId(request.RequestId, "requestId", errors);
        CheckId(request.CorrelationId, "correlationId", errors);
        CheckId(request.IdempotencyKey, "idempotencyKey", errors);
        CheckRepository(request.SourceRepository, "sourceRepository", errors);
        CheckRepository(request.TargetRepository, "targetRepository", errors);

        if (string.IsNullOrWhiteSpace(request.OperationId) || request.OperationId.Length > OutboundRequest.MaxIdentifierLength)
            errors.Add("operation.invalid");
        else if (!allowedOperations.Contains(request.OperationId)) errors.Add("operation.not-allowed");

        if (string.IsNullOrWhiteSpace(request.ContractVersion) || request.ContractVersion.Count(c => c == '.') != 2 || !Version.TryParse(request.ContractVersion, out var version) || version.Major < 1)
            errors.Add("contractVersion.invalid");
        if (string.IsNullOrWhiteSpace(request.AuthorizationRequirement) || request.AuthorizationRequirement.Length > OutboundRequest.MaxIdentifierLength)
            errors.Add("authorizationRequirement.invalid");
        if (request.Context is null || request.Context.Length > OutboundRequest.MaxContextLength) errors.Add("context.invalid");
        if (IsRepositoryId(request.TargetRepository) && !allowedTargets.Contains(request.TargetRepository)) errors.Add("target.not-allowed");
        if (string.Equals(request.SourceRepository, request.TargetRepository, StringComparison.OrdinalIgnoreCase)) errors.Add("source-target.same");
        return errors;
    }

    private static void CheckId(string value, string field, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > OutboundRequest.MaxIdentifierLength || value.Any(char.IsControl))
            errors.Add(field + ".invalid");
    }
    private static void CheckRepository(string value, string field, ICollection<string> errors)
    {
        if (!IsRepositoryId(value)) errors.Add(field + ".invalid");
    }

    // Keep repository identity rules aligned with CogA's offline RepositoryId parser.
    private static bool IsRepositoryId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 200 || value.Any(char.IsWhiteSpace))
            return false;

        var parts = value.Split('/');
        return parts.Length == 2 && IsRepositorySegment(parts[0]) && IsRepositorySegment(parts[1]);
    }

    private static bool IsRepositorySegment(string value) =>
        value.Length is > 0 and <= 100 &&
        value is not "." and not ".." &&
        value.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.');
}
