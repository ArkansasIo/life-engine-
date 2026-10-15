# LifeEngine — Noodle / NUDL

The `Noodle/` tree contains the genetics source port, broader LifeEngine subsystem ports, a C++23 bootstrap compiler/runtime executable target, and a separated Noodle-native compiler/runtime source tree.

## Compiler/runtime entry points

- `BootstrapCpp/` — bootstrap source and CLI executable target (`noodle`, plus `nudlc` alias); see `BootstrapCpp/README.md`.
- `Runtime/` — Noodle-native runtime source, currently a scaffold.
- `Compiler/Native/` — Noodle-native compiler passes (token/AST, lexer/parser, semantic analysis, IR, optimizer, backend, driver), currently incomplete.
- `Examples/` — hello-world and LifeEngine smoke programs for the bootstrap CLI.
- `BUILD.md` — build, test, run, and code-generation instructions.
- `PORT_MANIFEST.md` and `PORT_STATUS.md` — track the C# migration separately from compiler/runtime implementation.

## Build and run

```sh
cmake -S Noodle/BootstrapCpp -B build/noodle -DCMAKE_BUILD_TYPE=Release
cmake --build build/noodle --parallel
ctest --test-dir build/noodle --output-on-failure
build/noodle/noodle run Noodle/Examples/hello.nudl
```

The executable is a build target, not a checked-in platform-specific binary. CI is configured to build and test the bootstrap on GitHub Actions.

## Important status distinction

The bootstrap is a limited C++ implementation of the NUDL front end, interpreter, and C++ emitter. The compiler/runtime written in Noodle itself is **not yet complete or self-hosting**. Noodle-native source files are scaffolding for the later self-hosting stage and cannot yet be compiled as a whole by the current bootstrap. Do not treat every `.nudl` file in the LifeEngine port as executable today; many LifeEngine subsystem files use language features the current bootstrap does not support.
