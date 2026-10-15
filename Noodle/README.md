# LifeEngine — Noodle / NUDL Source Port

This folder contains a NUDL-language port of the initial human-biology and synthetic mutation module. It sits alongside the existing C# implementation under `Sources/Biology/Genetics/`; it does not replace or alter the C# build.

## Files

- `HumanBiologyCatalog.nudl` — human organ-system reference records.
- `MutationModels.nudl` — sequence and mutation types.
- `SequenceValidator.nudl` — DNA/RNA sequence normalization and alphabet validation.
- `MutationEngine.nudl` — seeded mutation operations and per-base substitution trials.
- `examples.nudl` — small usage example.

## Language status

These files use the NUDL 0.3-style readable typed syntax (`module`, `struct`, `enum`, `fn`, braces for blocks, square brackets for lists, and parentheses for calls). They are source-port/reference files; they have not been compiled because the NUDL compiler is not integrated into this repository's build pipeline. Check the language specification and compiler version before relying on exact runtime/library names.

## Scientific scope

The mutation engine is a synthetic educational simulation. Caller-supplied probabilities are model parameters, not validated human mutation-rate estimates. No clinical variant interpretation or medical conclusions are produced. The example sequences are artificial and are not human samples or reference genome fragments.

## Relationship to C#

The C# implementation remains the executable integration for the current LifeEngine application. Keep behaviour aligned between both versions and add cross-implementation tests when a NUDL compiler/runtime is available.
