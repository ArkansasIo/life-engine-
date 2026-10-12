# Data Model and Invariants

This document summarizes the principal data boundaries in LifeEngine. The source code remains authoritative; update this document when model behavior changes.

## Cellular automaton

The core model is implemented in `Sources/LifeGame.cs`.

- The grid is rectangular and stored as a one-dimensional array.
- A cell at coordinate `(x, y)` maps to `y * Columns + x`.
- The current generation is read while the next generation is written to a separate buffer.
- Rules use survive/birth notation. Conway's Life is `23/3`.
- The finite grid does not wrap at its edges; neighbors outside the board count as dead.
- Population counters should equal the number of true values in the current state array after every mutation or generation step.

Dimensions must be positive and their product must fit in the supported array size. Public methods should reject invalid coordinates or handle them according to their documented contract.

## Biological taxonomy and genomes

The biology namespace is `LifeEngine.Biology`. It models taxonomy, organisms, nucleic acids, genes, and ecological classification. Taxonomic identity and genome sequence are different concepts: changing a sequence should not implicitly rewrite an organism's taxonomic placement.

DNA uses A, C, G, and T bases; RNA uses A, C, G, and U. Complementation and transcription should be explicit operations with clear handling of invalid symbols.

## World map and ecosystem

World generation and ecological simulation are implemented under `Sources/World/` and `Sources/Ecosystem/`, with related model types under `Sources/Biology/`.

- A seed should be captured with generated-world output so a run can be reproduced.
- Every tile index must resolve to a valid tile.
- Species population values should be non-negative.
- Habitat, terrain, climate, migration, growth, and death are separate inputs to ecosystem updates.
- A tick should leave the world in a valid state even if a species reaches zero population.

## Wolf pack and combat

Wolf-pack behavior lives in `Sources/Biology/WolfPack.cs` and `Sources/Wolf/`. Combat support lives under `Sources/Combat/`.

Model updates should preserve valid health and energy bounds, avoid negative population-like counts, and record outcomes in a form that can be inspected or exported. Random behavior should use controllable seeds or injected random sources where reproducibility is required.

## Persistence and exports

Database schemas and seed data are under `Sources/Data/`. Export builders should treat text fields as data, escape CSV delimiters and quotes correctly, and avoid writing outside the intended export directory. Database schema changes should be coordinated with seed data and any code that reads or writes the affected tables.

## Compatibility

The project targets .NET Framework 4.5 and C# 7.3. New code must not assume APIs introduced in later .NET runtimes without a compatible dependency or a deliberate target-framework migration.
