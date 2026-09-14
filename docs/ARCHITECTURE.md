# Architecture

## The rule

The core knows nothing about what calls it — no UI, no network, no concrete database. CLI, GUI, AI, and collectors all sit outside and depend on the core.

---

## Layers

1. **Models** (`Core/Models`) — `Sample`, `SystemEvent`, `EventKind`, `DataKind`, `Profile`. Plain records, no dependencies.
2. **Storage** (`Core/Storage`) — `IRepository`, `IProfileManager`, `DateRange`, `MemoryRepository`, `SqliteRepository`, `Rows/` (SQLite-shaped DTOs).
3. **Collection contracts** (`Core/Collection`) — `ISampleCollector`, `ISystemEventCollector`. Platform-agnostic.
4. **Collectors** (`Thalamus.Collectors`, `net10.0-windows`) — `ActivityCollector`, `EventLogCollector`. The only Windows-specific code in the solution.
5. **CLI** (`Thalamus.Cli`) — currently a smoke test (`collect`, `list`, `profiles`), not the real command system.

---

## Decisions

**`record` for models** — value equality (free test assertions) and immutability. A measurement shouldn't be editable after the fact.

**Async everywhere in `IRepository`** — SQLite is async natively. Retrofitting later touches every call site.

**`IEnumerable<T>` in, `IReadOnlyList<T>` out** — permissive on input, restrictive on output. Callers can't mutate internal state.

**GUIDs, not incrementing IDs** — two machines both writing `Id = 1` can't be merged. Costs one line now.

**UTC timestamps everywhere** — merging data from two devices is impossible without a common clock.

**Soft delete (`IsDeleted`), filtered inside the repository** — never in calling code, so no consumer can forget. Costs disk space; a compaction command is a future problem.

**One SQLite file per profile**, not one shared database — moving a profile is copying a file. `profiles.json` holds only metadata (name, id, current profile); it lives outside any `.db` because it's what tells you *which* file to open.

**`DataKind` enum instead of a bool** for erase operations (`Sample`, `SystemEvent`, `All`) — a bool can't express "both," and `Erase(from, to, false)` is unreadable at the call site. Doesn't extend to read/write methods — those stay one-per-type because the return type must be known at compile time.

**Dapper over EF Core** — hand-written SQL is a skill worth having; query cost stays visible.

**SQLite row DTOs (`SampleRow`, `SystemEventRow`)** — Dapper can't materialize a positional record whose constructor types (`Guid`, `DateTime`, `TimeSpan`) don't match what SQLite returns (`string`, `long`). Rows sit only inside `SqliteRepository`; `Sample`/`SystemEvent` never leak the storage shape.

**`DateTime` stored as `TEXT` (ISO 8601 "o" format)** — readable in DB Browser, sortable as a string. `TimeSpan` stored as `INTEGER` seconds — durations don't need ISO complexity.

**`DateRange` is inclusive on both ends** `[From, To]` — chosen because it matched how test data was already written; enforced identically in `Contains()` and every SQL query (`<=`, not `<`). Getting this wrong between the two was a real bug caught by running the same contract tests against both repositories.

**Constructor injection for paths** — `DbConnectionFactory`, `SqliteRepository`, and `ProfileManager` all receive their storage location instead of computing it. Makes every layer testable with a temp folder instead of touching real `%APPDATA%`.

**Nullable `ProcessName`/`WindowTitle`** — `null` means idle, unambiguously. Empty string would collide with a window that legitimately has no title.

**`ActivityCollector` returns `null`** when `GetLastInputInfo` fails — an invented sample is worse than no sample.

---

## Known limits

- **Orphaned boot events are real**, not just theoretical — observed on this machine after a restart. Must decide how to treat a `Boot` with no matching `Shutdown` before the next one.
- **`EventLogCollector` re-reads full history every call** — no "since last time" filter yet. Fine for manual testing; must be fixed before running unattended.
- **One table per data source** (Model B rejected in favor of this) works for two sources. A third and fourth (game stats, sleep tracker) will make repeated per-type methods tedious — revisit with a generic `Entry` + JSON payload if that happens.
- **`SessionEventCollector` (Lock/Unlock) not built** — requires either a Windows message loop (uncertain from a console app) or the `Security` event log channel (needs elevated permissions). Deferred.
- **GUI framework undecided** — Avalonia vs. WebView2, deferred to the next phases.