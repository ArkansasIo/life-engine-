# Noodle Life Engine — Migration Status

A path-mapped Noodle source file has been generated for every C# source entry in `Sources/`, including host adapter contracts for WinForms, settings/resources, database/export, update networking, and GUI drawing. Existing Noodle modules contain partial algorithms; newly mirrored modules with `PortStatus` schemas are migration contracts, not completed translations.

## Runnable targets

- `Noodle/BootstrapCpp/` builds a C++23 bootstrap CLI on compatible toolchains; it supports only a subset of NUDL.
- `docs/noodle.html` runs a browser implementation of a limited Noodle subset.
- `docs/life-engine.html` is the browser Life Engine application. Its UI and host simulation adapter are JavaScript because the current Noodle runtime cannot yet compile or execute the complete project.
- The cellular automaton is the primary executable-domain migration; biology, combat, scripting, configuration, database, and host UI modules need behavior-parity work.

## Required before claiming a full native Noodle port

1. Implement arrays/indexing, mutable records, modules/imports, standard library, type checking, and host I/O APIs in the compiler/runtime.
2. Compile every Noodle module and resolve all diagnostics.
3. Port C# tests and compare seeded simulation/combat/ecology results against the reference.
4. Verify desktop/browser host adapters and persistence/export paths.
5. Make the Noodle-native compiler compile itself and run conformance tests.

C# sources remain intact as the behavioral reference. No source is marked runtime-verified unless tested.
