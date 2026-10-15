# Noodle Bootstrap Compiler and Runtime

This directory contains the C++23 bootstrap implementation adapted from the public `ArkansasIo/NUDL-0.3` compiler. It is deliberately separate from the Noodle-native source under `Noodle/Compiler/Native/` and `Noodle/Runtime/`.

## Build

Requirements: CMake 3.20+ and a C++23 compiler (GCC, Clang, or MSVC with C++23 support).

```sh
cmake -S Noodle/BootstrapCpp -B build/noodle -DCMAKE_BUILD_TYPE=Release
cmake --build build/noodle --parallel
ctest --test-dir build/noodle --output-on-failure
```

The executable is created at `build/noodle/noodle` on Linux/macOS or under the active configuration directory on Visual Studio generators. A second `nudlc` target is provided as a familiar compiler command name.

## Commands

```sh
build/noodle/noodle run Noodle/Examples/hello.nudl
build/noodle/noodle check Noodle/Examples/lifeengine-smoke.nudl
build/noodle/noodle compile Noodle/Examples/hello.nudl --target cpp --emit build/hello.cpp
```

`run` executes the supported subset through the embedded interpreter. `check` lexes and parses source. `compile` emits C++ source; it does not invoke a native C++ compiler automatically.

## Current supported subset and limitations

The bootstrap parser supports functions, typed parameters, bindings, return, if/else, while, basic arithmetic/comparison, calls, strings, booleans, null, and basic array syntax (the C++ code generator does not yet support array literals). The interpreter and C++ backend are incomplete relative to the full NUDL 0.3 specification. Struct/schema declarations, modules, member/index access, complete type checking, imports, and a full package/runtime system are not implemented. Unsupported code should fail rather than be described as fully compiled.

## Self-hosting status

The Noodle-native compiler and runtime source is staged under `Noodle/Compiler/Native/` and `Noodle/Runtime/`. Those files are not yet self-hosted or executable by this bootstrap; they contain contracts and incomplete passes. Full self-hosting requires expanding the language/runtime, completing the passes, then compiling the Noodle compiler with an earlier compiler and comparing outputs.
