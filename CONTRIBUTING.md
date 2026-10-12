# Contributing to LifeEngine

Thank you for helping improve LifeEngine. The project is a Windows Forms application targeting .NET Framework 4.5, with a cellular automaton, biology and ecosystem models, simulation tools, and a desktop interface.

## Before changing code

1. Read [README.md](README.md) for setup and user-facing behavior.
2. Read [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for component boundaries.
3. Read [docs/SYSTEMS.md](docs/SYSTEMS.md) and [docs/CONTROLS.md](docs/CONTROLS.md) when changing those areas.
4. Check the existing implementation and tests before introducing a parallel abstraction.

## Development environment

- Windows 10 or later recommended.
- A recent .NET SDK and Visual Studio 2019 or later are suitable.
- The project targets .NET Framework 4.5 and restores its reference assemblies through NuGet.

Build from the repository root:

```powershell
dotnet build Sources/LifeEngine.csproj -c Release
```

Run the built executable with the self-test switch. The output directory can vary by SDK, so locate the executable under `Binaries/Release/` if the path differs:

```powershell
& .\Binaries\Release\LifeEngine.exe --selftest
```

## Engineering expectations

- Keep changes compatible with the project's C# language version (7.3) and .NET Framework 4.5 APIs.
- Prefer deterministic, seedable algorithms for procedural generation.
- Validate dimensions, probabilities, rules, and external input at public API boundaries.
- Do not silently swallow exceptions. Report errors with enough context to diagnose the failing operation.
- Keep simulation logic independent of UI controls wherever practical.
- Preserve existing behavior unless the change explicitly documents a behavior change.
- Add or update self-tests for bug fixes and new deterministic behavior.
- Update relevant Markdown documentation when behavior, configuration, commands, or file formats change.
- Do not commit generated binaries, build folders, local IDE state, secrets, or machine-specific configuration.

## Pull requests

A useful pull request includes:

1. A concise explanation of the problem and intended result.
2. The main design choice and any compatibility impact.
3. Tests run and their actual results.
4. Screenshots for user-visible UI changes, where useful.
5. Follow-up limitations that remain.

Never report a build or test as passing unless it was actually run and its result observed.
