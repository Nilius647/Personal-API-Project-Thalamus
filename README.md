# Thalamus

A personal data API. It collects signals from the machine it runs on, stores them locally, and turns them into something you can actually read.
The thalamus is where sensory signals converge before being routed onward. Same idea here: collectors in, one coherent model out.

**Status:** Phase 2 complete — storage, profiles, and the first two collectors work end to end. No aggregation or real CLI yet.

---

## What it does right now

```
> thalamus collect
Collecting for profile 'Default'. Press Ctrl+C to stop.
Code  idle=3s
Discord  idle=2s
Opera idle=0s
...
Stopped. Data saved.

> thalamus list
4 sample(s) in the last 24 hours:
Sample { Id = ..., ProcessName = Code, IsIdle = False, IdleTime = 00:00:04, ... }
```

It samples the foreground window and idle time every few seconds, reads boot/shutdown/sleep/wake events from the Windows Event Log, and stores everything in a local SQLite file — one per profile.

---

## Requirements

- .NET 10 SDK
- Windows 11 (collectors are Windows-specific; the core storage layer is not)

---

## Build and run

Send from main folder (Thalamus/).

```powershell
dotnet build                                                    #Compile
dotnet test                                                     #Test
dotnet run --project src/Thalamus.Cli -- collect                #Collect samples
dotnet run --project src/Thalamus.Cli -- list                   #Show collected samples
dotnet run --project src/Thalamus.Cli -- profiles               #Show profiles
```

---

## Layout

```
Thalamus/
├── Thalamus.slnx
├── src/
│   ├── Thalamus.Core/          domain, storage, contracts (net10.0, portable)
│   │   ├── Models/
│   │   ├── Storage/
│   │   └── Collection/
│   ├── Thalamus.Collectors/
|   |   └── Windows/            Windows-specific implementations (net10.0-windows)
│   └── Thalamus.Cli/           command-line entry point
├── tests/
│   └── Thalamus.Tests/
└── docs/
    └── ARCHITECTURE.md
```

---

## Roadmap

| Phase | Scope |
|-------|-------|
| 1 | ✅ Build skeleton |
| 2 | ✅ Models, SQLite storage, profiles, first collectors |
| 3 | `DataManager` and aggregation — turn raw samples into answers |
| 4 | Real CLI with a command registry |
| 5 | Daily use — no new features, only friction notes |
| 6 | GUI, AI advisor, voice, automation |

---

## Design constraints

- The core never references UI, network, or a concrete database driver
- Platform-specific code lives only in `Thalamus.Collectors`, behind `ISampleCollector` / `ISystemEventCollector`
- Every record carries a GUID and a UTC timestamp, so profiles can move between machines later
- Soft delete only — records are never physically removed by application code
- Extensions (AI, automation, more collectors) are opt-in modules, not core features

See `docs/ARCHITECTURE.md` for the reasoning behind each decision.

---

## License

MIT

"Thalamus" refers to this project. If you fork or redistribute a
modified version, please retain a link to the original repository
and avoid presenting it as an unmodified original.