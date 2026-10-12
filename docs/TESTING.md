# Testing and Verification

This document describes repeatable checks for LifeEngine. A command listed here is a procedure, not a claim that it has already passed in every environment.

## 1. Build

Run from the repository root on Windows with a recent .NET SDK:

```powershell
dotnet build Sources/LifeEngine.csproj --configuration Release
```

The project targets .NET Framework 4.5. NuGet restores `Microsoft.NETFramework.ReferenceAssemblies`; the WinForms application itself is Windows-only.

## 2. Run the self-test harness

The executable routes `--selftest` and `-t` to `Sources/Tools/SelfTest.cs` and returns the harness exit code:

```powershell
$exe = Get-ChildItem .\Binaries\Release -Filter LifeEngine.exe -Recurse |
  Select-Object -First 1 -ExpandProperty FullName
if (-not $exe) { throw "Build output LifeEngine.exe not found." }
& $exe --selftest
if ($LASTEXITCODE -ne 0) { throw "Self-test failed: $LASTEXITCODE" }
```

Use the actual build output path if your local configuration differs.

## 3. What to cover when changing the simulation

- Rule parsing accepts valid survive/birth notation and handles invalid input predictably.
- A step computes the next generation from the old generation, not partially updated cells.
- Cell counts remain within `0..Rows*Columns`.
- Edge cells follow the documented finite-grid boundary behavior.
- Clear, randomize, set-start, and reset keep population counts and state arrays consistent.
- History state matches the documented meaning of a cell having been alive.
- Invalid dimensions, probabilities, and null rules do not produce unhandled exceptions.

## 4. Biology and ecosystem checks

- World generation with the same seed produces the same world.
- Terrain and climate values remain within documented bounds.
- Species populations never become negative.
- Migration only targets valid tiles and respects habitat rules.
- Extinction behavior agrees with the configured policy.
- Repeated runs do not corrupt taxonomy registrations or species collections.

## 5. UI smoke test

On Windows, launch the release executable without arguments and check:

- The main form opens without an exception dialog.
- Start, stop, and single-step commands work.
- Changing grid size and rules updates the displayed state.
- Sidebar tabs and world/wolf tools do not freeze the UI.
- Settings and export operations report failures clearly.
- Closing the application completes without an unhandled exception.

## 6. Reporting results

For each change, record the command, environment, exit code, and any remaining failure. A successful compile does not prove that every UI path or simulation subsystem works correctly.
