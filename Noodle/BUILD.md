# Build and execute LifeEngine Noodle

## Prerequisites

- CMake 3.20 or newer
- A C++23 compiler
- Git checkout of this repository

## Build

```sh
cmake -S Noodle/BootstrapCpp -B build/noodle -DCMAKE_BUILD_TYPE=Release
cmake --build build/noodle --parallel
ctest --test-dir build/noodle --output-on-failure
```

## Run a Noodle program

```sh
build/noodle/noodle run Noodle/Examples/hello.nudl
```

## Parse/check a program

```sh
build/noodle/noodle check Noodle/Examples/lifeengine-smoke.nudl
```

## Emit C++ source

```sh
build/noodle/noodle compile Noodle/Examples/hello.nudl --target cpp --emit build/hello.cpp
c++ -std=c++23 build/hello.cpp -o build/hello
./build/hello
```

The current bootstrap is intentionally a limited subset. It does not compile the entire Noodle-native compiler or all LifeEngine `.nudl` modules yet. CI results are the source of truth for build/test status; a repository commit by itself does not establish that the executable built successfully.
