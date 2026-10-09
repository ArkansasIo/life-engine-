# LifeEngine

A **Game of Life** laboratory for Windows: Conway's cellular automaton, a full
biological-simulation layer on top of it, and a desktop UI built around a ribbon
menu, a left-hand control sidebar and a status bar.

---

## Table of contents

- [What it is](#what-it-is)
  - [Two generations of interface](#two-generations-of-interface)
- [Features](#features)
- [Requirements](#requirements)
- [Building and running](#building-and-running)
- [Using the program](#using-the-program)
  - [Top ribbon menu](#top-ribbon-menu)
  - [Rulesets](#rulesets)
  - [Left sidebar](#left-sidebar)
  - [Status bar](#status-bar)
  - [Mouse controls](#mouse-controls)
  - [Keyboard controls](#keyboard-controls)
- [Settings](#settings)
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

### Two generations of interface

There is more than one window in this program, and which one you see depends
on two things.

- **The current interface** is `LifeEngine.Gui.MainForm`. It is what `Program`
  starts, it is the one this document describes, and it adds World / Wolves /
  Tools / Updates menus, a four-tab control panel, an ecosystem, a wolf pack, a
  script console, a settings editor and a scheduler on top of the automaton.
- **The original 2017 form** is `LifeEngine.FormLife`. It is the fallback: if
  `MainForm` throws while it is being built, `Program` shows the error and opens
  `FormLife` instead. It is the grid, the six buttons, the rules list and the
  Options menu, and nothing else.

A full run may also be started headless for testing:

```powershell
.\Binaries\Release\net45\LifeEngine.exe --selftest
```

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

### World, wolves, tools and settings

- **Ecosystem:** a generated world map of terrain and climate with species that
  carry populations per tile, grow, die, migrate and can go extinct.
- **Wolf pack:** a ranked pack with an alpha pair, traits, ages, kills, fights,
  hunts by night and seasons that age and breed the pack.
- **Script console:** a small scripting language with classes, functions and a
  standard library, run either from the editor or from loaded script files.
- **Settings editor:** every setting listed with its value, type, default and a
  plain-English description, editable in place, with per-setting and global
  reset, save and reload.
- **Scheduler:** timed and repeating jobs that run on their own while the
  program is open.
- **Updates:** an update manifest can be loaded and its effect described before
  anything is applied.

### Interface

- Dark ribbon-style menu strip across the top with File / Simulation / View /
  World / Wolves / Tools / Updates / Help menus, sub-menus, checkable items and
  keyboard shortcuts.
- Tabbed control panel down the left: World, Wolves, Tools and Log.
- Grid in the middle, event log across the bottom, status bar under that.
- Every command reachable from the mouse *and* the keyboard.

---

## Requirements

- Windows 7 or later.
- .NET Framework 4.5 (Runtime or Developer Pack).
- To build from source: the .NET SDK (any recent version) or Visual Studio
  2015 or later. The project is SDK-style, so `dotnet build` is the supported
  route; the reference assemblies for .NET Framework 4.5 come from the
  `Microsoft.NETFramework.ReferenceAssemblies` package, so no targeting pack
  is required.

---

## Building and running

From a command line, at the solution root:

```powershell
dotnet build Sources/LifeEngine.csproj -c Release
.\Binaries\Release\net45\LifeEngine.exe
```

Or open `LifeEngine.sln` in Visual Studio and press **F5**.

The project is a Windows-only WinForms application on .NET Framework, so it
will not build on Linux or macOS.

Build output goes to `Binaries\Debug\net45\` or `Binaries\Release\net45\`
depending on the configuration.

---

## Using the program

### Top ribbon menu

Eleven menus across the top, in this order. An underlined letter in the name is
the `Alt` accelerator; an em dash means no shortcut.

**File**

| Item | Shortcut | What it does |
| --- | --- | --- |
| **New grid** | `Ctrl+N` | Empties the grid and stops the run. |
| **Randomize** | `Ctrl+R` | Fills the grid at the random density (`Simulation.RandomDensity`, 15% by default). |
| *separator* | | |
| **Clear grid** | `Ctrl+Del` | Empties the grid and stops the run. |
| **Reset to start** | `Ctrl+Backspace` | Restores the grid to the state it had when Start was last pressed. |
| *separator* | | |
| **Save settings** | — | Writes the settings file. |
| **Reload settings** | — | Reads the settings back in, replacing runtime changes. |
| *separator* | | |
| **Exit** | `Alt+F4` | Closes the program, saving settings first if `Application.SaveSettingsOnExit` is on. |

**Simulation**

| Item | Shortcut | What it does |
| --- | --- | --- |
| **Start** | `F5` | Locks in the start state and begins running. |
| **Stop** | `F6` | Pauses the run; the grid is left as it is. |
| **Step** | `F7` | Advances one generation without starting the timer. |
| *separator* | | |
| **Speed** | — | Sub-menu: `1`, `2`, `5`, `10`. The timer interval is `(11 - Speed) * 10` ms. |
| **Grid size** | — | Sub-menu: `25`, `50`, `75`, `100`, `150`, `200`, `250` cells a side. Choosing one stops the run and clears the grid. |
| **Rules** | — | Sub-menu of all 24 rulesets, with the running one ticked. Changing it takes effect immediately, without stopping the run. |
| **Cadence** | — | Sub-menu of the four speeds, each shown with the millisecond interval it stands for. |
| *separator* | | |
| **Reset zoom** | `Ctrl+0` | Returns the grid to 1x. |

**View**

| Item | Shortcut | What it does |
| --- | --- | --- |
| **Zoom in** | `Ctrl++` | Magnifies the grid by 1.5x, up to 5x. |
| **Zoom out** | `Ctrl+-` | Shrinks the grid back toward its base size. |
| *separator* | | |
| **Show grid lines** | `Ctrl+G` | Toggles the cell border lines. Checkable. |
| **Show sidebar** | `Ctrl+B` | Shows or hides the control panel. Checkable. |
| *separator* | | |
| **Live cell colour** | — | Colour picker for living cells. |
| **Dead cell colour** | — | Colour picker for empty cells. |
| **History cell colour** | — | Colour picker for recently-live cells. |

**World**

| Item | Shortcut | What it does |
| --- | --- | --- |
| **New world** | `Ctrl+W` | Builds a world from the seed in the settings, seeds it with life, and shows the World tab. |
| **New world (no life)** | — | Builds the same world but registers the species without seeding them. |
| *separator* | | |
| **Run 100 ticks** | `Ctrl+T` | Runs the ecosystem 100 ticks. |
| **Run ticks from settings** | — | Runs the ecosystem `Ecosystem.TicksPerRun` ticks (100 by default). |
| **Step one tick** | — | Runs the ecosystem one tick. |
| *separator* | | |
| **Clear life** | — | Empties the world of life but keeps the species registered. |
| **Reseed with a new seed** | — | Picks a new random seed, stores it and rebuilds the world. |
| *separator* | | |
| **World report** | — | Opens a read-only window with the world summary. |

**Ecosystem**

| Item | Shortcut | What it does |
| --- | --- | --- |
| **Build world** | — | Builds a world from the current seed and seeds it with life, then shows the World tab. |
| **Build world (barren)** | — | Builds the same world with the species registered but nothing living on it. |
| *separator* | | |
| **Run one tick** | — | Advances the ecosystem a single tick. |
| **Run ten ticks** | — | Advances it ten. |
| **Run hundred ticks** | — | Advances it a hundred. |
| *separator* | | |
| **Growth rate** | — | Sub-menu: `0.1x`, `0.25x`, `0.5x`, `1x`, `2x`, `4x`, `8x`, `12x`. Writes `Ecosystem.GrowthMultiplier`. |
| **Death rate** | — | Sub-menu on the same ladder. Writes `Ecosystem.DeathMultiplier`. |
| *separator* | | |
| **Clear life** | — | Empties the world of life but keeps the species registered. |
| **Census** | — | Opens a read-only window with the world summary. |

**Wolves**

| Item | Shortcut | What it does |
| --- | --- | --- |
| **Found pack** | — | Founds "The Grey Company" with "Fenrir" as its first alpha, and shows the Wolves tab. |
| *separator* | | |
| **Run one night** | — | Runs one night, with no full moon. |
| **Run a night under a full moon** | — | Runs one night with the full moon on. |
| **Run a season** | — | Runs a season: ageing, breeding and dispersal. |
| *separator* | | |
| **Pack report** | — | Opens a read-only window with the pack report. |

**Bestiary**

Four sub-menus, grouping the preset creatures by the kind of creature they are.
Choosing one opens a read-only window with its statistics.

| Sub-menu | Contents |
| --- | --- |
| **Prey** | The five animals a pack hunts. |
| **Rivals** | The five wolves and strangers a pack fights. |
| **Predators** | The four creatures that are dangerous to a wolf. |
| **Others** | Anything that fits none of the other three. |
| **Bestiary summary** | Opens a read-only window describing the whole bestiary. |

**Data**

| Item | Shortcut | What it does |
| --- | --- | --- |
| **Export everything (CSV)** | — | Writes every table as its own CSV file in `Exports\`. |
| **Export everything (Excel workbook)** | — | Writes every non-empty table into one `.xls` workbook. Falls back with an explanation if the Jet provider is not installed. |
| *separator* | | |
| **Export version / grid summary / generation / world / biome census / species / tile sample / pack / wolves / settings / schedules / releases / events** | — | Writes that one table as a CSV file. |
| *separator* | | |
| **What would be exported** | — | Lists every table with its row count, without writing anything. |

Export reads the live objects, so a table is only as full as the state it is
built from — a biome census needs a world, a pack table needs a pack.

**Tools**

| Item | Shortcut | What it does |
| --- | --- | --- |
| **Script console** | `Ctrl+E` | Shows the Tools tab. |
| **Settings editor** | — | Shows the Tools tab, where every setting can be edited. |
| *separator* | | |
| **Install default jobs** | — | Puts the stock jobs back into the scheduler. |
| **Run due jobs now** | — | Runs every job that is due and logs how many ran. |
| *separator* | | |
| **Event log** | — | Opens a read-only window with the whole event log. |

**Updates**

| Item | Shortcut | What it does |
| --- | --- | --- |
| **Load manifest** | — | Reads the update manifest and describes what it holds. |
| **What would apply** | — | Describes which updates would be applied, without applying them. |
| *separator* | | |
| **Version information** | — | Opens a read-only window with the version details. |

**Help**

| Item | Shortcut | What it does |
| --- | --- | --- |
| **Controls** | `F1` | Opens the keyboard and mouse reference. |
| **About** | — | Version and licence information. |

### Rulesets

The 24 rulesets, in the order they appear in the list. The left column is the
raw `survive/birth` string the engine reads; the right is the name shown beside
it. The ruleset can be changed while the simulation runs.

| Rule | Name | Rule | Name |
| --- | --- | --- | --- |
| `23/3` | Conway's Life | `23/36` | HighLife |
| `125/36` | 2x2 | `34678/0123478` | Inverse Life |
| `34/34` | 34 Life | `5/345` | Long Life |
| `1358/357` | Amoeba | `12345/3` | Maze |
| `35678/4678` | Anneal | `1234/3` | Mazectric |
| `4567/345` | Assimilation | `245/368` | Move |
| `235678/378` | Coagulations | `238/357` | Pseudo Life |
| `45678/3` | Coral | `1357/1357` | Replicator |
| `34678/3678` | Day & Night | `/2` | Seeds |
| `5678/35678` | Diamoeba | `/234` | Serviettes |
| `012345678/3` | Flakes | `235678/3678` | Stains |
| `1/1` | Gnarl | `2345/45678` | Walled cities |

### Left sidebar

The sidebar docks to the left edge and is shown or hidden with `Ctrl+B`. It is
a tab control with four pages, and it is re-sized by dragging the splitter
between it and the grid (`Window.SidebarWidth`, default 220 pixels).

**World tab** — a terrain map on the left and, on the right, a species table
(species, trophic level, head count, tiles occupied) over a text summary of the
world. Under the map is a line naming the last tile you clicked. Along the
bottom:

| Control | What it does |
| --- | --- |
| **Show life** | Draws the living population over the terrain. On by default. |
| **Show rivers** | Draws rivers. On by default. |
| **Ticks** | Number of ticks used by Run ticks. 1 to 100000, default 100. |
| **Run ticks** | Runs the ecosystem the number of ticks in the box. |
| **Step tick** | Runs the ecosystem one tick. |
| **Clear life** | Empties the world of life but keeps the species registered. |

**Wolves tab** — a gold banner naming the alpha, the pack size, its cohesion and
how many times the alpha has changed; a roster of every wolf ranked by fighting
power with rank, name, age, health, energy, strength, aggression, kills and
wins/losses, sorted the same way the pack itself sorts; a hunt log and a pack
report. The alpha is drawn in gold, the omega in grey-blue and the dead in dull
red. Along the bottom:

| Control | What it does |
| --- | --- |
| **Found pack** | Founds "The Grey Company" with "Fenrir" as its first alpha. |
| **Nights** | How many nights Run night repeats. 1 to 10000. |
| **Run night** | Runs that many nights, under a full moon if the box is ticked. |
| **Run season** | Runs a season: ageing, breeding and dispersal. |
| **Full moon** | Whether those nights are run under a full moon. |
| **Report** | Writes the pack report into the pane below. |

**Tools tab** — four sub-tabs.

| Sub-tab | Controls |
| --- | --- |
| **Scripts** | A script name list, and **Run editor**, **Run loaded**, **Reload folder** and **Clear output**. Source on top, output underneath. |
| **Settings** | The full settings table (setting, value, type, default, description; changed rows in gold). **Apply** (or double-click a row), **Reset this**, **Reset all**, **Save**, **Reload**. |
| **Scheduler** | A checkable list of jobs (job, action, repeat, runs, last run, next run). **Install default jobs**, **Run due now**, **Run selected**, **Refresh**. |
| **Updates** | **Load manifest**, **What would apply**, **Version info**. |

**Log tab** — the running event log, newest at the bottom.

Above the sidebar and below it, the window is split horizontally so the grid
sits over the event log, and the splitter can be dragged. The menu starts the
Wolves tab, Ctrl+T starts the World tab, Ctrl+E starts the Tools tab.

### Status bar

Reads, from left to right:

```
Ready | Rule: 23/3 | Gen: 0 | Pop: 0 | Size: 100 | Speed: 10 | Zoom: 1x | No world
```

The rule, generation, population, size, speed, zoom and world fields update on
every step. The first field reads `Running` while the simulation is running and
`Ready` when it is not. The last field reads `No world` until a world is built,
then `World: <head> head, tick <n>`.

### Mouse controls

| Where | Action | Result |
| --- | --- | --- |
| Grid, left click | Toggle | Flips one cell between alive and dead. |
| Grid, left drag | Paint | Keeps drawing the state of the first cell you touched. |
| Grid, right drag | Pan | Scrolls the view while zoomed in. |
| Grid, middle drag | Pan | Same as right drag, for mice without a usable right button. |
| Grid, wheel | Zoom | Each notch zooms in or out one step. |
| World map | Click | Selects a tile and describes it in the line under the map. |
| Sidebar tabs | Click | Switches between World, Wolves, Tools and the Log. |
| Settings row | Double click | Loads the row into the value box, ready to edit. |
| Scheduler row | Check | Enables or disables that job. |
| Report window | Scroll | Reports open in a read-only, scrollable text window. |

### Keyboard controls

| Key | Action |
| --- | --- |
| `F5` | Start / resume |
| `F6` | Stop / pause |
| `F7` | Single step |
| `F1` | Opens the Controls report |
| `Ctrl+N` | New grid |
| `Ctrl+R` | Randomize |
| `Ctrl+Del` | Clear grid |
| `Ctrl+Backspace` | Reset to start state |
| `Ctrl+G` | Toggle grid lines |
| `Ctrl+B` | Toggle the sidebar |
| `Ctrl+W` | New world |
| `Ctrl+T` | Run 100 ecosystem ticks |
| `Ctrl+E` | Script console |
| `Ctrl++` / `Ctrl+-` | Zoom in / out |
| `Ctrl+0` | Reset zoom |
| `Space` | Start or stop, whichever applies |
| `Escape` | Stop |
| `Left` `Right` `Up` `Down` | Scroll the grid 40 pixels |
| `PageUp` / `PageDown` | Scroll the grid 400 pixels vertically |
| `Home` | Scroll the grid back to the top left |
| `Tab` | Move focus between the panes |
| `Alt` + letter | Open a menu from its underlined letter |
| `Alt+F4` | Exit |

---

## Settings

The Tools tab, Settings sub-tab lists every setting with its key, current value,
type, default and a sentence saying what it does. A row whose value has been
changed from the default is drawn in gold.

| Key | Default | What it does |
| --- | --- | --- |
| `Application.Name` | `LifeEngine` | Name shown in the title bar and the About box. |
| `Application.Developer` | `ArkansasIo` | Who builds this program. |
| `Application.UpdateChannel` | `stable` | Channel updates come from: `stable` or `beta`. |
| `Application.CheckForUpdates` | `true` | Whether to look for a newer build on start-up. |
| `Application.SaveSettingsOnExit` | `true` | Whether to write the settings file back when the program closes. |
| `Application.SettingsFile` | *(empty)* | A settings file to load instead of the default one. |
| `Window.Width` | `604` | Starting width of the window, 240 to 10000 pixels. |
| `Window.Height` | `688` | Starting height of the window, 240 to 10000 pixels. |
| `Window.CenterOnStart` | `true` | Whether the window opens centred on the screen. |
| `Window.SidebarVisible` | `true` | Whether the control sidebar is shown. |
| `Window.SidebarWidth` | `220` | Width of the sidebar, 120 to 800 pixels. |
| `Simulation.GridSize` | `100` | Cells per side: 25 to 250. |
| `Simulation.Speed` | `10` | Speed preset, 1 to 10. Interval is `(11 - Speed) * 10` ms. |
| `Simulation.Rules` | `23/3` | The ruleset, in survive/birth notation. |
| `Simulation.RandomDensity` | `0.15` | Density used when the grid is randomised, 0.01 to 1.0. |
| `Simulation.AutoStart` | `false` | Whether the simulation starts as soon as the program opens. |
| `Appearance.ShowGrid` | `true` | Whether the grid lines between cells are drawn. |
| `Appearance.GridLineSize` | `1.0` | Thickness of those lines, 0 to 8 pixels. |
| `Appearance.LiveColor` | `#00bfff` | Colour of a live cell. |
| `Appearance.DeadColor` | `#ffffff` | Colour of an empty cell. |
| `Appearance.HistoryColor` | `#90ee90` | Colour of a cell that has been alive before. |
| `Appearance.BackgroundColor` | `#ffffff` | Colour behind the cells. |
| `Appearance.Zoom` | `1` | Zoom at start-up, 1 to 5. |
| `World.Seed` | `20261007` | Seed for world generation. The same seed always builds the same planet. |
| `World.SeaLevel` | `0.0` | Fraction of the elevation range at or below which everything is water. |
| `World.MountainLine` | `2000.0` | Height in metres at which mountains become impassable. |
| `World.CutRivers` | `true` | Whether rivers are cut from the high ground down to the sea. |
| `World.TerrainDetail` | `1.0` | Scale factor for elevation detail, 0.1 to 4.0. |
| `Ecosystem.TicksPerRun` | `100` | How many ticks the ecosystem runs for "Run ticks from settings". |
| `Ecosystem.FollowSimulation` | `false` | Whether the ecosystem advances on every simulation step. |
| `Ecosystem.GrowthMultiplier` | `1.0` | Multiplier on every species' growth rate, 0.1 to 12.0. |
| `Ecosystem.DeathMultiplier` | `1.0` | Multiplier on every species' death rate, 0.1 to 12.0. |
| `Ecosystem.AllowExtinction` | `true` | Whether a species with nobody left is dropped from the registry. |
| `Ecosystem.Migration` | `true` | Whether species spread into empty neighbouring tiles. |
| `Updates.ManifestPath` | `Updates\updates.xml` | Path or URL of the update manifest. |
| `Updates.AllowPrerelease` | `false` | Whether prerelease builds are offered. |
| `Updates.VerifyChecksums` | `true` | Whether a package's SHA-256 is checked before it is applied. |
| `Updates.RestartAfterUpdate` | `true` | Whether the program may restart itself after applying an update. |
| `Updates.ApplyPatchesIncrementally` | `true` | Whether only the changed files in a patch are written. |
| `Diagnostics.LogLevel` | `info` | Log verbosity: `silent`, `error`, `warning`, `info` or `debug`. |
| `Diagnostics.LogFile` | *(empty)* | Where the log is written. Empty means the settings folder. |
| `Diagnostics.LogUnhandledExceptions` | `true` | Whether a crash is written to the log before the program closes. |
| `Diagnostics.ProfileSimulation` | `false` | Whether timings are printed around each generation. |

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
|   |-- SYSTEMS.md            Wolf, combat, ecosystem and data subsystems
|   `-- CONTROLS.md           Full mouse and keyboard reference
|-- Sources/
|   |-- LifeEngine.csproj     SDK-style project (WinForms, .NET Framework 4.5)
|   |-- Program.cs            Entry point
|   |-- FormLife.cs           Main window logic
|   |-- FormLife.Designer.cs  Main window layout
|   |-- FormIcon.cs           Application icon helper
|   |-- LifeGrid.cs           The cell grid control (custom painting)
|   |-- LifeGrid.Designer.cs
|   |-- LifeGame.cs           Rules engine: stepping, rules parsing, history
|   |-- Ai/                   Cell AI and difficulty tuning
|   |-- Attributes/           Attribute definitions
|   |-- Biology/              Biological model (taxonomy, kingdoms, biomes,
|   |   |                       animal catalogue, wolf packs)
|   |   |-- Enums.cs          Taxonomy, diet, locomotion, symmetry ...
|   |   |-- NucleicAcid.cs    Nucleotide, Gene, Dna, Rna, transcription
|   |   |-- Taxonomy.cs       Taxon and Phylum
|   |   |-- Organism.cs       Organism base, unicellular, multicellular, virus
|   |   |-- AnimalKingdom.cs  Species, AnimalKingdom, Animal, Vertebrate ...
|   |   |-- AnimalRanks.cs    Animal rank ladder
|   |   |-- AnimalCatalogue.cs Registered animal species
|   |   |-- Animals.cs        Mammal, Bird, Reptile, Insect, Cephalopod ...
|   |   |-- Kingdoms.cs       Plants, fungi, protists, bacteria, archaea
|   |   |-- Biomes.cs         BiomeType, Zone, Biome, BiomeMap
|   |   |-- WorldMap.cs       Tile map of the world
|   |   |-- WolfPack.cs       Wolf pack behaviour
|   |   `-- Biosphere.cs      World container and default world builder
|   |-- Combat/               Combat engine, effects, bestiary, battle log
|   |-- Configuration/        Config file, defaults, scheduler, updates
|   |-- Data/                 Database, export builder, spreadsheet writer,
|   |   |                       schema.sql and seed.sql
|   |-- Ecosystem/            Ecosystem state and world factory
|   |-- Gui/                  Panels: main form, world view, wolf, tools
|   |-- Scripting/            Script host, interpreter, parser, lexer
|   |-- Talents/              Talent tree
|   |-- Tools/                Icon generator and self-test harness
|   |-- Wolf/                 Wolf logic
|   |-- World/                Weather and day/night cycle
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
| [docs/SYSTEMS.md](docs/SYSTEMS.md) | Wolf, combat, ecosystem and data subsystems. |
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