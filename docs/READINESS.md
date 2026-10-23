# CogB Readiness

**Readiness: 20% — initial contract slice, not integrated dispatch.**

## Implemented on development
- Typed outbound request envelope with request, correlation, and idempotency identifiers.
- Deterministic target and operation allowlists.
- Canonical owner/name shape validation, contract version validation, and bounded context.
- Transport interface with receipt explicitly distinguished from target outcome.
- Dependency-free console contract tests.

## Not implemented
- GitHub repository-dispatch or other real transport.
- Authentication/permission verification by a trusted host.
- Persistent idempotency/deduplication.
- Target-side execution, callback handling, or CogC result correlation.
- Cross-repository integration with CiAi or the other Cogs.

## Run the checks
    dotnet run --project tests/CiAi.CogB.Tests/CiAi.CogB.Tests.csproj

These checks are offline and do not demonstrate live dispatch. No credentials, publishing, deployment, or cross-repository writes are used.
