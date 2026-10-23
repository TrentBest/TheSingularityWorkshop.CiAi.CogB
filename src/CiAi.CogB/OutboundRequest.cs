namespace CiAi.CogB;

/// <summary>A bounded, typed request proposal. It is data, not authorization to execute.</summary>
public sealed record OutboundRequest(
    string RequestId,
    string CorrelationId,
    string IdempotencyKey,
    string SourceRepository,
    string TargetRepository,
    string OperationId,
    string ContractVersion,
    string AuthorizationRequirement,
    string Context)
{
    public const int MaxContextLength = 4096;
    public const int MaxIdentifierLength = 128;
}
