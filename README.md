# The Singularity Workshop CiAi.CogB

**CogB turns approved intent into a bounded, correlated request.** It records request identity and pinned inputs; it does not dispatch execution.

## 01 — See it in a minute

Requires .NET 8:

```bash
dotnet run --project tests/TheSingularityWorkshop.CiAi.CogB.ContractTests --configuration Release
```

## 02 — Responsibility

CogB validates request, operation and idempotency identity; requester; capability/provider/version; source repository and full commit SHA; input SHA-256; contract version; output byte limit; and expiry. The in-memory request book returns the original request for an identical retry and rejects reuse of an idempotency key with changed inputs.

CogB does not authorize (CogD), discover (CogA), dispatch remote jobs, retrieve artifacts, or accept outputs.

## 03 — Trust boundary

- Requests are data, never executable commands.
- Source and input identities are pinned before work is requested.
- Output size is bounded to 64 MiB in this first slice.
- Idempotency is in-memory only; durable retry safety remains future work.
- No network access or external dependencies.

## Gallery

See [Gallery.md](Gallery.md) for the quick tour and verification recipe.

## 04 — UML and use case

See [CapabilityRequestBook UML and use case](docs/UML/CapabilityRequestBookUML.md).

See [CapabilityRequestBook class companion](src/TheSingularityWorkshop.CiAi.CogB/CapabilityRequestBook.md).

## 05 — Release status

**Implementation readiness: 25%. Package preparation: 25%.** Typed bounded requests and offline idempotency checks are committed; persistence, CogA/CogD integration, authenticated dispatch, and parent-coordinator orchestration remain outstanding. Not a publication approval.