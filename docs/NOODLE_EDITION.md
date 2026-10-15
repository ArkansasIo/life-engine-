# Life Engine — Noodle Edition

The browser application is `life-engine.html`. It provides a cellular automaton grid, B/S rule editing, step/run/pause, randomization, cell editing, generation/density metrics, preview world indicators, and a Noodle source workbench.

## Implementation boundary

The browser page is a host application, not proof that all Life Engine logic is compiled from Noodle. The current C++23 bootstrap supports only a small language subset and cannot compile all Noodle modules. Source modules under `Noodle/` are in varying stages of partial implementation or migration-contract scaffolding.

GitHub Pages deploys `docs/` from `master` through `.github/workflows/pages.yml`.
