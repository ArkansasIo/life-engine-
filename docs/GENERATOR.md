# Procedural Generation Design

LifeEngine already contains world-generation code. This document defines the expected contract for extending it into a more complete, reproducible procedural world and ecosystem generator.

## Goals

- Reproducible output from a recorded seed and configuration.
- Separate generation stages that can be tested independently.
- Valid terrain, climate, water, habitat, and species population data.
- Configurable world size and environmental parameters.
- Exportable metadata sufficient to reproduce a generated world.

## Suggested pipeline

1. **Normalize configuration** — validate dimensions, seed, sea level, mountain threshold, and terrain-detail parameters.
2. **Generate elevation** — use a seeded pseudo-random source and deterministic noise or a documented equivalent.
3. **Classify terrain** — derive water, plains, hills, and mountains from elevation thresholds.
4. **Generate hydrology** — route rivers from higher terrain toward lower terrain and ensure termination or a bounded iteration count.
5. **Generate climate** — derive temperature and precipitation from latitude, elevation, and seeded variation.
6. **Assign biomes** — map climate and terrain conditions to valid biome types.
7. **Register species** — use habitat compatibility and trophic role to select candidate organisms.
8. **Seed populations** — distribute non-negative population values within carrying-capacity constraints.
9. **Validate** — check dimensions, tile indexes, value ranges, water/river consistency, and species references.
10. **Report and export** — record seed, configuration, generator version, counts, and warnings.

## Determinism contract

For a fixed generator version, seed, and configuration, generation should return the same model state. Do not rely on ambient clock time, process-randomized hash codes, or unseeded random-number generators in the generation path. If an algorithm changes, record a generator version so old saves can be interpreted correctly.

## Error handling

Reject invalid dimensions and impossible parameter combinations before allocating large arrays. Put explicit bounds on loops such as river tracing. If a generation stage cannot satisfy a constraint, return a diagnostic or use a documented fallback rather than hanging or silently creating corrupt data.

## Performance

- Avoid repeated full-world scans inside per-tile loops where a cached pass will do.
- Keep rendering out of the model layer.
- Consider cancellation and progress reporting for large worlds.
- Benchmark generation separately from UI rendering and ecosystem ticking.

## Acceptance checklist

- [ ] Same seed/configuration yields identical output.
- [ ] Different seeds can produce different output.
- [ ] All tile and species references are valid.
- [ ] Numeric values remain within configured bounds.
- [ ] Rivers and other traversals terminate.
- [ ] Invalid inputs fail with useful diagnostics.
- [ ] Self-tests cover both typical and boundary cases.
