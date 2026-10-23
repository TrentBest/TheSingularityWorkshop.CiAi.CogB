using CiAi.CogB;

var targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "TrentBest/TestTarget" };
var operations = new HashSet<string>(StringComparer.Ordinal) { "observe.status", "test.noop" };
var valid = new OutboundRequest("req-001", "corr-001", "idem-001", "TrentBest/TestSource", "TrentBest/TestTarget", "test.noop", "1.0.0", "test-environment-only", "bounded sample context");
Require(RequestValidation.Validate(valid, targets, operations).Count == 0, "valid request should pass");
Require(RequestValidation.Validate(valid with { TargetRepository = "TrentBest/Unknown" }, targets, operations).Contains("target.not-allowed"), "unknown target must be rejected");
Require(RequestValidation.Validate(valid with { OperationId = "publish.package" }, targets, operations).Contains("operation.not-allowed"), "unknown operation must be rejected");
Require(RequestValidation.Validate(valid with { Context = new string('x', OutboundRequest.MaxContextLength + 1) }, targets, operations).Contains("context.invalid"), "oversized context must be rejected");
Require(RequestValidation.Validate(valid with { SourceRepository = "not-canonical" }, targets, operations).Contains("sourceRepository.invalid"), "malformed source repository must be rejected");
Require(RequestValidation.Validate(valid with { TargetRepository = "TrentBest/.." }, targets, operations).Contains("targetRepository.invalid"), "dot-segment repository identity must be rejected");
Require(RequestValidation.Validate(valid with { SourceRepository = "TréntBest/TestSource" }, targets, operations).Contains("sourceRepository.invalid"), "non-ASCII repository identity must be rejected");
Require(RequestValidation.Validate(valid with { SourceRepository = "TrentBest/TestTarget" }, targets, operations).Contains("source-target.same"), "self-target must be rejected");
Console.WriteLine("PASS: CogB request-contract checks");
return;

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
