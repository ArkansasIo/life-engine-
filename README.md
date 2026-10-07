# LifeEngine

A **Game of Life** laboratory for Windows: Conway's cellular automaton, a full
biological-simulation layer on top of it, and a desktop UI built around a ribbon
menu, a left-hand control sidebar and a status bar.

---

## Table of contents

- [What it is](#what-it-is)
- [Features](#features)
- [Requirements](#requirements)
- [Building and running](#building-and-running)
- [Using the program](#using-the-program)
  - [Top ribbon menu](#top-ribbon-menu)
  - [Left sidebar](#left-sidebar)
  - [Status bar](#status-bar)
  - [Mouse controls](#mouse-controls)
  - [Keyboard controls](#keyboard-controls)
- [Project layout](#project-layout)
- [Documentation](#documentation)
- [Roadmap](#roadmap)
- [Credits](#credits)
- [License](#license)

---

## What it is

LifeEngine has two halves that share one window.

1. **The cellular automaton.** A square grid of cells, each alive or dead. A
   *ruleset* decides which cells are born and which survive, written in the
   standard `survive/birth` notation (`23/3` is Conway's Life). Twenty-four
   rulesets ship with the program, from Seeds through Day & Night to Walled
   Cities. The grid keeps a *history* layer so you can see where cells used to
   be, and the simulation can be started, stopped, single-stepped, cleared or
   randomly seeded.

2. **The biological model.** Under the `LifeEngine.Biology` namespace lives a
   taxonomy of life: domains, kingdoms, phyla, classes, species, genomes, genes,
   nucleotides, biomes, climate zones, habitat zones and organisms. A whole
   Earth-like starter world (rainforest, desert, tundra, ocean, lake) with
   eighteen representative organisms can be spun up by
   `Biosphere.CreateDefaultWorld()`. This is the layer the engine is growing
   into: not just cells that live and die, but *things that live and die*.

LifeEngine is a Windows Forms application targeting .NET Framework 4.5. It was
originally published by RangeMachine in 2017 and is developed here as
**ArkansasIo** from 2026 onward.

---

## Features

### Simulation

- Square grid, 25 to 250 cells a side, re-sizable at runtime.
- 24 built-in rulesets, and the rules can be changed while the simulation runs.
- Four speed presets (1, 2, 5, 10).
- Start / Stop / Step / Reset / Clear / Randomize.
- Live-cell, dead-cell and history-cell colours, each independently configurable.
- Optional grid lines with adjustable thickness.
- Zoom from 1x to 5x, with live generation and population counters.
- History trail: recently-live cells are drawn in their own colour.

### Biology layer

- **Taxonomy:** `Taxon`, `Phylum`, `Species`, full eight-rank ladder
  (Domain, Kingdom, Phylum, Class, Order, Family, Genus, Species).
- **Kingdoms:** plants (tree, flowering plant, moss, fern), fungi, protists and
  algae, bacteria, archaea.
- **Animal kingdom:** vertebrates (mammal, bird, reptile, amphibian, fish) and
  invertebrates (insect, arachnid, crustacean, cephalopod, gastropod, bivalve,
  cnidarian, sponge, annelid), each registered under its phylum.
- **Molecular biology:** `Nucleotide`, `Gene`, `Dna`, `Rna`, transcription
  (T to U) and complementation (A-T / A-U, C-G).
- **Ecology:** `BiomeMap`, `Biome`, `Zone`, `ClimateZone`, `HabitatTerrain`,
  `TrophicLevel`; organisms can be filed per biome and grouped per kingdom.

### Interface

- Ribbon-style menu strip across the top with File / Simulation / View / Options
  / Help menus, sub-menus, checkable items and keyboard shortcuts.
- Docked control sidebar on the left: Simulation, Rules, Appearance and
  Statistics groups.
- Status bar along the bottom: state, generation, population, grid size, speed
  and zoom.
- Every command reachable from the mouse *and* the keyboard.

---

## Requirements

- Windows 7 or later.
- .NET Framework 4.5 (Runtime or Developer Pack).
- To build from source: Visual Studio 2015 or later, or MSBuild 14+.
  `dotnet build` will not work, because this is an old-style Windows Forms
  project rather than an SDK-style one.

---

## Building and running

Open `LifeEngine.sln` in Visual Studio and press **F5**.

Or from a command line:

```powershell
msbuild LifeEngine.sln /p:Configuration=Release
.\Binaries\Release\LifeEngine.exe
```

Build output goes to `Binaries\Debug\` or `Binaries\Release\` depending on the
configuration.

---

## Using the program

### Top ribbon menu

| Menu | Item | Shortcut | What it does |
| --- | --- | --- | --- |
| **File** | Randomize | `Ctrl+R` | Fills the grid at 15% density. |
| | Clear | `Ctrl+Del` | Empties the grid and stops the run. |
| | Reset | `Ctrl+Backspace` | Restores the grid to the state it had when Start was last pressed. |
| | *separator* | | |
| | Exit | `Alt+F4` | Closes the program. |
| **Simulation** | Start | `F5` | Locks in the start state and begins running. |
| | Stop | `F6` | Pauses the run; the grid is left as it is. |
| | Step | `F7` | Advances one generation without starting the timer. |
| | *separator* | | |
| | Speed | — | Sub-menu with `1`, `2`, `5`, `10`. |
| | Grid Size | — | Sub-menu with `25` to `250` cells a side. |
| **View** | Zoom In | `Ctrl++` | Magnifies the grid by 1.5x, up to 5x. |
| | Zoom Out | `Ctrl+-` | Shrinks the grid back toward its base size. |
| | Reset Zoom | `Ctrl+0` | Returns to 1x. |
| | Show Grid Lines | `Ctrl+G` | Toggles the cell border lines. |
| | Show Sidebar | `Ctrl+B` | Shows or hides the left control sidebar. |
| **Options** | Rules | — | Sub-menu listing all 24 rulesets; the current one is ticked. |
| | Live Cell Colour | — | Colour picker for living cells. |
| | Dead Cell Colour | — | Colour picker for empty cells. |
| | History Cell Colour | — | Colour picker for recently-live cells. |
| **Help** | About | — | Version and licence information. |

### Left sidebar

The sidebar docks to the left edge and is shown or hidden with `Ctrl+B`.

- **Simulation** — Start, Stop, Step, Reset, Clear and Randomize buttons, plus
  the Speed and Size drop-downs. The same commands as the menu, kept on screen
  for quick access.
- **Rules** — the ruleset drop-down, with the raw `survive/birth` string shown
  next to its plain-language name.
- **Appearance** — grid lines on or off, and the three colour swatches.
- **Statistics** — generation, population, live-cell ratio, elapsed running
  time and current zoom.

### Status bar

Reads, from left to right:

```
Ready | Rule: 23/3 - Conway's Life | Gen: 0 | Pop: 0 | Size: 100 | Speed: 10 | Zoom: 1x
```

The rule and generation fields update on every step; `Running`, `Paused` and
`Ready` reflect the simulation state.

### Mouse controls

| Where | Action | Result |
| --- | --- | --- |
| Grid, left click | Toggle | Flips one cell between alive and dead. |
| Grid, left drag | Paint | Keeps drawing the state of the first cell you touched. |
| Grid, right drag | Pan | Scrolls the view while zoomed in. |
| Grid, middle drag | Pan | Same as right drag, for mice without a usable right button. |
| Grid, wheel | Zoom | Each notch zooms in or out one step. |
| Sidebar, menu | Click | Runs the corresponding command. |

### Keyboard controls

| Key | Action |
| --- | --- |
| `F5` | Start / resume |
| `F6` | Stop / pause |
| `F7` | Single step |
| `Ctrl+R` | Randomize |
| `Ctrl+Del` | Clear |
| `Ctrl+Backspace` | Reset to start state |
| `Ctrl+G` | Toggle grid lines |
| `Ctrl+B` | Toggle the sidebar |
| `Ctrl++` / `Ctrl+-` | Zoom in / out |
| `Ctrl+0` | Reset zoom |
| `Space` | Start or stop, whichever applies |
| `Escape` | Stop |
| `Left` `Right` `Up` `Down` | Scroll the grid view |
| `Tab` | Move focus between sidebar and grid |
| `Alt` + letter | Open a menu from its underlined letter |
| `Alt+F4` | Exit |

---

## Project layout

```
LifeEngine/
|-- LifeEngine.sln            Solution file
|-- README.md                 This document
|-- LICENSE.md                Licence summary
|-- docs/
|   |-- ARCHITECTURE.md       How the pieces fit together
|   |-- UML.md                Class, sequence, state and use-case diagrams
|   |-- BIOLOGY.md            The biological model, in detail
|   `-- CONTROLS.md           Full mouse and keyboard reference
|-- Sources/
|   |-- Program.cs            Entry point
|   |-- FormLife.cs           Main window logic
|   |-- FormLife.Designer.cs  Main window layout
|   |-- LifeGrid.cs           The cell grid control (custom painting)
|   |-- LifeGrid.Designer.cs
|   |-- LifeGame.cs           Rules engine: stepping, rules parsing, history
|   |-- Biology/              Biological model
|   |   |-- Enums.cs          Taxonomy, diet, locomotion, symmetry ...
|   |   |-- NucleicAcid.cs    Nucleotide, Gene, Dna, Rna, transcription
|   |   |-- Taxonomy.cs       Taxon and Phylum
|   |   |-- Organism.cs       Organism base, unicellular, multicellular, virus
|   |   |-- AnimalKingdom.cs  Species, AnimalKingdom, Animal, Vertebrate ...
|   |   |-- Animals.cs        Mammal, Bird, Reptile, Insect, Cephalopod ...
|   |   |-- Kingdoms.cs       Plants, fungi, protists, bacteria, archaea
|   |   |-- Biomes.cs         BiomeType, Zone, Biome, BiomeMap
|   |   `-- Biosphere.cs      World container and default world builder
|   |-- Properties/           Assembly info, resources, settings
|   `-- Resources/            Icons used by the menus
`-- Binaries/                 Build output (git-ignored)
```

---

## Documentation

| Document | Contents |
| --- | --- |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | Layers, responsibilities, data flow, ruleset format. |
| [docs/UML.md](docs/UML.md) | Class, sequence, state, activity and use-case diagrams. |
| [docs/BIOLOGY.md](docs/BIOLOGY.md) | Kingdoms, taxonomy, molecular biology, biomes, sample world. |
| [docs/CONTROLS.md](docs/CONTROLS.md) | Every mouse gesture and keyboard shortcut. |

---

## Roadmap

- [ ] Bind the biology layer to the grid so organisms occupy cells and compete.
- [ ] Genome mutation between generations, with selection pressure.
- [ ] Save and load grids in RLE and plain-text formats.
- [ ] Export a run as an animated GIF.
- [ ] Record and replay a run.

---

## Credits

- **RangeMachine** — original LifeEngine, 2017.
- **ArkansasIo** — maintenance and development, 2026 onward.
- Inspiration for the ecosystem and genome half comes from
  [SproutLife](https://github.com/ShprAlex/SproutLife) by Alex Shapiro
  (MIT licence): organisms that own cells, carry a genome and mutate across
  generations. No SproutLife code is used in this project.

---

## License

> LifeEngine is free software: you can redistribute it and/or modify
> it under the terms of the GNU General Public License as published by
> the Free Software Foundation, either version 3 of the License, or
> (at your option) any later version.
> 
> LifeEngine is distributed in the hope that it will be useful,
> but WITHOUT ANY WARRANTY; without even the implied warranty of
> MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
> GNU General Public License for more details.
> 
> You should have received a copy of the GNU General Public License
> along with this program. If not, see <http://www.gnu.org/licenses/>.
>
> Copyright (c) 2017 RangeMachine
> Copyright (c) 2026 ArkansasIo