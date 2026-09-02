# Project Thalamus: Personal API

Tha final goal is to create a personal API that collects personal data, elaborates it, and lets you read it.

**Status:** Phase 1 - Only the skeleton, not usable yet

---

## Requirements

- .NET 10 SDK
- Windows 11 (collectors are Windows-specific; the core is not)

## Build and run

```powershell
dotnet build
dotnet run --project src/Thalamus.Cli
dotnet test
```

---

## Layout

```
Thalamus/
├── Thalamus.sln
├── src/
│   ├── Thalamus.Core/     domain logic and storage contracts
│   └── Thalamus.Cli/      command-line interface
├── tests/
│   └── Thalamus.Tests/
└── docs/
```