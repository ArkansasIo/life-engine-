# LifeEngine — Noodle / NUDL Source Port

This folder is the additive NUDL migration area for LifeEngine. It contains the initial human-biology/mutation port plus starter modules for simulation, attributes, ecosystem, weather/time, combat, AI difficulty, configuration defaults, and scripting contracts. The complete C# implementation remains under `Sources/` as the behavioral reference; the Noodle folder is not yet a complete replacement.

## Files

- `HumanBiologyCatalog.nudl` — human organ-system reference records.
- `MutationModels.nudl` — sequence and mutation types.
- `SequenceValidator.nudl` — DNA/RNA sequence normalization and alphabet validation.
- `MutationEngine.nudl` — seeded mutation operations and per-base substitution trials.
- `examples.nudl` — small usage example.
- `Core/`, `Attributes/`, `Biology/`, `Ecosystem/`, `World/`, `Combat/`, `Ai/`, `Wolf/`, `Talents/`, `Configuration/`, `Scripting/`, `Data/`, and `Host/` — initial subsystem ports and migration contracts; see `PORT_STATUS.md` for exact limitations.

## Port coverage

- `PORT_MANIFEST.md` maps all 66 C# source files to intended Noodle destinations.
- `PORT_STATUS.md` distinguishes starter/partial modules from the work still outstanding.
- These modules are initial translations and modeling passes, not a claim that every C# method has been ported.

## Language status

These files use the NUDL 0.3-style readable typed syntax (`module`, `struct`, `enum`, `fn`, braces for blocks, square brackets for lists, and parentheses for calls). They are source-port/reference files; they have not been compiled because the NUDL compiler is not integrated into this repository's build pipeline. Check the language specification and compiler version before relying on exact runtime/library names.

## Scientific scope

The mutation engine is a synthetic educational simulation. Caller-supplied probabilities are model parameters, not validated human mutation-rate estimates. No clinical variant interpretation or medical conclusions are produced. The example sequences are artificial and are not human samples or reference genome fragments.

## Relationship to C#

The C# implementation remains the executable integration for the current LifeEngine application. Keep behaviour aligned between both versions and add cross-implementation tests when a NUDL compiler/runtime is available.
