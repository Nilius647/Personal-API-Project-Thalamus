# Thalamus — Project Context

Personal data API. Collects signals from the machine it runs on, stores them locally in SQLite, and will eventually expose them through aggregation, a GUI, and an AI advisor. Explanations matter as much as working code; don't just hand over finished implementations without the reasoning.

**Current status:** Phase 2 complete (storage, profiles, first collectors, working end to end). Phase 3 (aggregation) in progress. See `docs/ARCHITECTURE.md` and `docs/Checklists/` for the full roadmap and every design decision made so far — read both before making structural changes.

Language: all code, identifiers, comments, and commit messages are in English. Conversation with the user may be in Italian, but nothing written to the repo is.

---

## The three questions

Everything in this project exists to answer these. Any feature that doesn't serve one of them, or a documented future phase, is scope creep — flag it rather than building it silently.

1. How long was the PC on without being used — and roughly what did that cost in power?
2. How much time was spent at the PC this week vs. previous weeks, and on what?
3. What time did the PC turn on/off each day, for the last X days?

---

## The one rule that matters most

**Dependencies point inward.** `Thalamus.Core` must never reference a UI framework, an HTTP client, or a concrete database driver. Everything else — CLI, GUI, AI, collectors, automation — depends on the Core; the Core depends on none of them.

Before adding any `using` in `Thalamus.Core`, check it doesn't violate this.

---

## Solution layout

```
Thalamus.slnx
src/
├── Thalamus.Core/            net10.0, portable — no platform-specific code allowed here
│   ├── Models/                record types: Sample, SystemEvent, EventKind, DataKind, Profile
│   ├── Storage/                IRepository, IProfileManager, DateRange, MemoryRepository,
│   │   │                       SqliteRepository, DbConnectionFactory, Schema, AppPaths
│   │   └── Rows/               internal SQLite-shaped DTOs (SampleRow, SystemEventRow) —
│   │                           never leak outside SqliteRepository
│   └── Collection/             ISampleCollector, ISystemEventCollector (platform-agnostic contracts)
├── Thalamus.Collectors/      net10.0-windows — the ONLY project allowed to be Windows-specific
│   └── Windows/                NativeMethods (P/Invoke), ActivityCollector, EventLogCollector
├── Thalamus.Cli/              net10.0-windows, currently a rough switch-statement smoke test
                                (real command system is Phase 4, not built yet)
tests/
└── Thalamus.Tests/            net10.0-windows (must match Collectors' target to reference it)
docs/
├── ARCHITECTURE.md            every decision made, with reasoning — check before re-deciding something
└── Checklists/                phase-by-phase working checklists (in Italian, personal working notes, not user-facing documentation).
```

---

## Stack

- .NET 10, C# 14, `Nullable enable`, `ImplicitUsings enable`
- Dapper (hand-written SQL, not EF Core — deliberate, keeps query cost visible)
- Microsoft.Data.Sqlite
- xUnit
- `.slnx` solution format (not `.sln`)

---

## Conventions established so far

- **`record` for domain models**, not `class` — value equality (free test assertions) and immutability. A measurement shouldn't be editable after construction.
- **Async everywhere in `IRepository`** — even `MemoryRepository`, which has nothing to await, uses `Task.CompletedTask` / `Task.FromResult` to satisfy the interface consistently with `SqliteRepository`.
- **`IEnumerable<T>` in, `IReadOnlyList<T>` out** on repository methods — permissive on input, restrictive on output so callers can't mutate internal state.
- **GUIDs, never incrementing IDs.** UTC timestamps on every record. Soft delete (`IsDeleted` flag) — filtered inside the repository query itself, never left to calling code to remember.
- **Constructor injection for every path** — `DbConnectionFactory`, `SqliteRepository`, `ProfileManager` all receive their storage location rather than computing it (e.g. from `AppPaths` directly). This is what makes them testable against temp folders instead of real `%APPDATA%`. Don't reintroduce a static path dependency inside these classes.
- **One SQLite file per profile**, not a shared database. `profiles.json` holds only metadata (id, name, current profile pointer) — it lives outside any `.db` because it's what tells you which file to open.
- **`DateRange` is inclusive on both ends** `[From, To]`. This must match exactly between `DateRange.Contains()` and every SQL query (`<=`, never `<`). Getting this inconsistent between the two was a real bug once, caught by running the same contract tests against both `MemoryRepository` and `SqliteRepository` — that dual-run is the regression test for this specific class of bug, keep doing it for every new repository method.
- **`DataKind` enum (`Sample`, `SystemEvent`, `All`)** for operations where the same call can target either table (`EraseDataAsync`, `EraseByIdAsync`). This does *not* extend to read/write methods — those stay one-per-type (`AddSamplesAsync`/`AddSystemEventsAsync`, etc.) because the return type must be known at compile time; a generic method can't return `Sample` or `SystemEvent` depending on a runtime enum value.
- **SQLite row DTOs** (`SampleRow`, `SystemEventRow`, in `Storage/Rows/`, `internal`) exist because Dapper can't materialize a positional record whose constructor types (`Guid`, `DateTime`, `TimeSpan`, `bool`) don't match what SQLite actually returns (`string`, `long`). Each row type has `FromX(X model)` (static) and `ToX()` (instance) conversion methods. Never let `Sample` or `SystemEvent` — or any future domain type — leak the storage-shaped types outside `SqliteRepository`.
- **`DateTime` persisted as `TEXT`** in ISO 8601 round-trip format (`.ToString("o")`), parsed back with `DateTimeStyles.RoundtripKind` to preserve `DateTimeKind.Utc` — omitting that yields `Unspecified` and silently reintroduces timezone bugs in comparisons. **`TimeSpan` persisted as `INTEGER` seconds**, not minutes — a `FromMinutes`/`TotalSeconds` mismatch has already caused a real 60x bug once.
- **Enum values are explicit and start at 1** (`EventKind`, `DataKind`) because they're effectively persisted (as SQL integers or in JSON) — `0` would collide with an uninitialized default, and reordering values would silently reinterpret old data. Comment above each enum should say not to reorder, only append.
- **Nullable `ProcessName`/`WindowTitle` on `Sample`** — `null` means idle, unambiguously. Empty string was rejected because it collides with a window that legitimately has no title.
- Naming: PascalCase for types/members, camelCase for locals/parameters, `_camelCase` for instance fields, PascalCase for `static readonly` fields (treated as constants). SQL column names deliberately match C# property names 1:1 (PascalCase in the schema) so Dapper needs zero configuration.

---

## Known open issues (don't silently "fix" without flagging)

- **`EventLogCollector` re-reads the entire Event Log history on every call** — no "since last timestamp" filtering yet. Fine for manual smoke testing; will produce duplicate `SystemEvent` rows if run repeatedly inside a loop without addressing this first.
- **Orphaned `Boot` events are a confirmed real occurrence** (observed on the dev machine after a restart), not just a theoretical edge case — Phase 3's aggregation logic must have an explicit, documented, tested rule for a `Boot` with no matching `Shutdown` before the next `Boot`.
- **`SessionEventCollector` (Lock/Unlock) was deliberately not built.** Lock/Unlock event IDs (4800/4801) live in the `Security` event log channel, which typically needs elevated permissions — different problem than the `System` channel used for Boot/Shutdown/Sleep/Wake. Not required for the three core questions; don't add it opportunistically without discussing the permissions issue first.
- **One table per data source doesn't scale indefinitely.** Works fine for two (`samples`, `systemEvents`). A third or fourth data source (e.g. game stats, a sleep tracker) would make repeated per-type repository methods tedious — if that happens, revisit with a generic `Entry` + JSON payload model rather than mechanically adding a fifth pair of `AddXAsync`/`GetXAsync` methods.
- **GUI framework is an open decision** (Avalonia vs. WebView2), deliberately deferred to Phase 6. Don't scaffold a GUI project preemptively.

---

## Testing conventions

- Every `IRepository` contract test must be runnable against **both** `MemoryRepository` and `SqliteRepository` with identical assertions — that's the actual point of the abstraction, not a formality.
- Use fixed, hand-picked `DateTime` values in tests (always `DateTimeKind.Utc` explicit), never `DateTime.UtcNow` — tests must be deterministic.
- SQLite-backed tests use a temp file path (`Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db")`), and must call `SqliteConnection.ClearAllPools()` before `Directory.Delete` — otherwise Windows throws `IOException` because Microsoft.Data.Sqlite keeps a pooled file handle open even after `using` disposes the logical connection.
- `ProfileManager` tests use a temp folder passed to its constructor, never the real `%APPDATA%` path.

---

## Build and run

```powershell
dotnet build
dotnet test
dotnet run --project src/Thalamus.Cli -- collect
dotnet run --project src/Thalamus.Cli -- list
dotnet run --project src/Thalamus.Cli -- profiles
```

CI runs on `windows-latest` (not `ubuntu-latest`) because `Thalamus.Collectors` targets `net10.0-windows`.

---

## Working style for this project

- Prefer small, focused commits. Ask before bundling unrelated changes into one commit.
- When a design decision is genuinely open (not yet resolved in `ARCHITECTURE.md`), say so explicitly and list the real trade-offs rather than picking one silently.
- When touching `IRepository` or `IProfileManager`, remember every implementation (`MemoryRepository`, `SqliteRepository`) and every existing contract test needs to stay in sync — a signature change is never a one-file change here.
- Don't introduce EF Core, AutoMapper, MediatR, or similar heavier abstractions without discussion — the project deliberately favors visible, hand-written code over convenience libraries, both as a design choice and a learning choice.