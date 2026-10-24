using TheSingularityWorkshop.CiAi.CogB;
var passed = 0;
var book = new CapabilityRequestBook();
var first = NewRequest();
Check("submit records pinned bounded request", () => { Assert(ReferenceEquals(first, book.Submit(first)), "first submission differs"); Assert(book.TryGet("request-1", out var found) && ReferenceEquals(first, found), "lookup failed"); });
Check("identical retry returns original request", () => { Assert(ReferenceEquals(first, book.Submit(NewRequest(requestId: "retry"))), "retry did not return original"); Assert(book.Count == 1, "retry duplicated request"); });
Check("idempotency key reuse with changed inputs is rejected", () => Throws<InvalidOperationException>(() => book.Submit(NewRequest(inputDigest: new string('b', 64)))));
Check("malformed source revision and digest are rejected", () => { Throws<ArgumentException>(() => NewRequest(sourceRevision: "development")); Throws<ArgumentException>(() => NewRequest(inputDigest: "bad")); });
Check("output bound and expiry are enforced", () => { Throws<ArgumentOutOfRangeException>(() => NewRequest(maximumOutputBytes: 67_108_865)); Throws<ArgumentException>(() => NewRequest(expiresAt: DateTimeOffset.Parse("2026-10-24T00:00:00Z"))); });
Check("snapshot is read-only and deterministic", () => { var snapshot = book.Snapshot(); Assert(snapshot.Count == 1 && ReferenceEquals(snapshot[0], first), "snapshot mismatch"); Assert(snapshot is not List<CapabilityRequest>, "mutable list leaked"); });
Console.WriteLine($"PASS: {passed} CogB contract checks.");
void Check(string name, Action action) { action(); passed++; Console.WriteLine($"PASS {passed}: {name}"); }
static CapabilityRequest NewRequest(string requestId = "request-1", string inputDigest = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", string sourceRevision = "abcdef0123456789abcdef0123456789abcdef01", long maximumOutputBytes = 1024, DateTimeOffset? expiresAt = null) => new(requestId, "operation-1", "idempotency-1", "client-ci", "uml.generate", "1.0.0", "TrentBest/Gui", "TrentBest/Gui", sourceRevision, inputDigest, 1, maximumOutputBytes, DateTimeOffset.Parse("2026-10-24T00:00:00Z"), expiresAt ?? DateTimeOffset.Parse("2026-10-24T01:00:00Z"));
static void Assert(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
static void Throws<TException>(Action action) where TException : Exception { try { action(); } catch (TException) { return; } throw new InvalidOperationException($"Expected {typeof(TException).Name}."); }