# CiAi.CogB — Outbound Request Boundary

**Mission:** define a typed, bounded outbound request and deterministic validation before any transport is called.

CogB is an experimental library in the CiAi delegation set. It does not itself authorize GitHub operations, perform network calls, or execute an LLM-generated command. A trusted host must provide explicit target and operation allowlists and separately enforce authorization.

## Documentation

See [RequestValidation UML and use case](docs/UML/RequestValidationUML.md) for the request boundary, collaborators, and host-owned authorization responsibilities.

## 01 — See it in a minute

Requires the .NET 8 SDK.

    dotnet build src/CiAi.CogB/CiAi.CogB.csproj --configuration Release
    dotnet run --project tests/CiAi.CogB.Tests/CiAi.CogB.Tests.csproj --configuration Release

The dependency-free offline checks cover a valid request, an unknown target, an unknown operation, oversized context, malformed repository identity, and a self-target. CI runs the same checks on pushes to development/master and pull requests targeting development.

## 02 — Request contract

The request envelope carries request, correlation, and idempotency identifiers; source and target repository identities; operation ID; three-part contract version; a named authorization requirement; and bounded context. The authorization requirement is a policy label, never a credential.

Validation checks field bounds, repository shape, contract version, target allowlist, operation allowlist, and self-target rejection. The transport abstraction returns a transport receipt; that receipt is **not** proof that the target operation succeeded.

## 03 — Safety boundary

- No live transport or GitHub dispatch is implemented.
- No credentials, publishing, deployment, or cross-repository writes.
- The in-memory allowlists are supplied by the caller and are not a complete permission system.
- Idempotency identifiers are carried but are not persisted or deduplicated by this slice.
- CogC result correlation and CiAi integration remain outstanding.

## 04 — Status

**Readiness: 20%.** This is a tested contract slice, not an integrated dispatcher. See [READINESS.md](docs/READINESS.md) for the boundary between implemented and planned behavior.
