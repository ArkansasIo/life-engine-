# Release notes

What changed, build by build. The same content also lives in
`Updates/updates.xml`, which is the machine-readable copy the updater reads;
this file is the one a person reads.

Build names are `yyyy.MM.dd`, with `-rN` when there is more than one build in a
day.

---

## 2026.10.07-r3 — 1.1.0.0 — "Rivers and Rain"

*Patch. Requires 1.0.0.0 or later. 1.4 MB.*

The biology stops being a library and becomes a world.

### Added

- **World map** (`Sources/Biology/WorldMap.cs`). A grid of tiles with a
  latitude gradient, value-noise continents, an elevation field, hillslopes
  that cut rivers from high ground down to the sea, coastlines marked, and a
  biome decided per tile from its temperature and rainfall. Deterministic: the
  same seed always builds the same planet.
- **Latitude bands.** Twelve bands from pole to pole, each with a base
  temperature, a base rainfall and a climate zone, so the tropics are hot and
  wet and the poles are cold and dry without anyone hard-coding a map.
- **Ecosystem** (`Sources/Ecosystem/Ecosystem.cs`). Species with a habitat
  envelope, a carrying capacity per tile and a growth rate; populations move
  logistically toward what each tile can support; herbivores graze the
  producers and predators graze the herbivores; a consumer with nothing to eat
  starves.
- **Species catalogue.** Twelve species in three trophic tiers, from Grassland
  Grass and Algal Bloom through Honey Bee, Common Frog, Atlantic Salmon and
  Garden Spider to Grey Wolf, Golden Eagle, Common Octopus and Orca.
- **World factory** (`Sources/Ecosystem/WorldFactory.cs`). Builds a default
  world, registers the default species, seeds them, and renders the map as text.
- **Animal catalogue** (`Sources/Biology/AnimalCatalogue.cs`). Every animal
  phylum, the classes under the ones with creatures, and the orders under those
  classes, each with its rank, its scientific name and its common name. Adds a
  `Class` rank type and an `Order` type alongside `Phylum`.
- **Settings file** (`Sources/LifeEngine.settings`). Every setting the program
  reads, with its default, grouped by category and commented.
- **Version and build identity** (`Sources/Configuration/VersionInfo.cs`).
  Product, version, build name, build date, developer and copyright, read from
  the assembly, with a build-name parser and comparator.
- **Update and patch system** (`Sources/Configuration/UpdateSystem.cs`).
  Releases, channels, and a planner that decides whether a release may be
  applied to the build in hand and in what order.
- **Update manifest** (`Updates/updates.xml`). Channels and releases with
  checksums, sizes and change lists.

### Changed

- `Sources/App.config` unchanged; the program's own settings now live in
  `LifeEngine.settings` rather than in the .NET settings system.

### Fixed

- Three source files had a corrupted byte sequence before the licence header
  (`Biosphere.cs`, `Biomes.cs`, `Kingdoms.cs`); the headers are intact again.
- `Biosphere.Find` used a `ContainsKey` lookup followed by an indexer read.

---

## 2026.09.21-r1 — 1.0.1.0 — "Interface Rebuild"

*Patch. Requires 1.0.0.0 or later. 512 KB.*

### Added

- Ribbon menu across the top: File, Simulation, View, Options, Help, with
  sub-menus, checkable items and shortcuts.
- Control sidebar on the left: simulation buttons, speed and size drop-downs,
  the ruleset list and the three cell colours.
- Status bar along the bottom: state, rule, generation, population, size,
  speed and zoom.
- Keyboard shortcuts (`F5` `F6` `F7`, `Ctrl+R`, `Ctrl+G`, `Ctrl+B`, `Ctrl+0`)
  and mouse gestures on the grid: paint, pan and wheel zoom.

### Changed

- The zoom controls moved from the bottom row into the View menu and the
  sidebar.

---

## 2026.09.01-r1 — 1.0.0.0 — "Project Restart"

*Full release. 8.4 MB.*

### Added

- Conway's Life engine with twenty-four rulesets from Conway's Life through
  HighLife and Day & Night to Walled Cities.
- Grid from 25 to 250 cells a side, with a history layer, grid lines and
  configurable colours.
- Start, stop, step, reset, clear and randomize.
- The biological model: taxonomy, organisms, animal kingdom, plants, fungi,
  protists, prokaryotes, genomes, genes, nucleotides, biomes and zones.
- `Biosphere.CreateDefaultWorld()`, an Earth-like starter world with five
  biomes and eighteen organisms.

### Changed

- The project is maintained as ArkansasIo from this build onward.

### Removed

- The old RangeMachine copyright line, replaced throughout with ArkansasIo.

---

## How to read a kind

| Kind | Meaning |
| --- | --- |
| **Full** | Replaces the install. Everything before it becomes unnecessary. |
| **Patch** | A change on top of an existing build. Applied in order. |
| **Breaking** | Alters or removes existing behaviour. Held back from the stable channel. |
