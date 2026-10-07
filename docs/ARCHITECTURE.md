# Architecture

How LifeEngine is put together: the layers, what each one owns, how a tick
travels through them, and the file formats involved.

---

## Contents

- [Overview](#overview)
- [Layer 1: presentation](#layer-1-presentation)
- [Layer 2: simulation](#layer-2-simulation)
- [Layer 3: biology](#layer-3-biology)
- [Data flow](#data-flow)
- [The ruleset format](#the-ruleset-format)
- [Threading and timing](#threading-and-timing)
- [Design decisions and their reasons](#design-decisions-and-their-reasons)
- [Known gaps](#known-gaps)

---

## Overview

```
+---------------------------------------------------------------+
|  LifeEngine.exe  (Windows Forms, .NET Framework 4.5)          |
|                                                               |
|  +------------------------- Presentation --------------------+ |
|  |  FormLife      menu ribbon, sidebar, status bar, commands| |
|  |  LifeGrid      cell grid control, double-buffered paint  | |
|  +----------------------------------------------------------+ |
|                              |                                |
|  +------------------------- Simulation ----------------------+ |
|  |  LifeGame      rules, stepping, history, population      | |
|  +----------------------------------------------------------+ |
|                              |                                |
|  +-------------------------- Biology ------------------------+ |
|  |  Biosphere  BiomeMap  AnimalKingdom  organisms  genome   | |
|  +----------------------------------------------------------+ |
+---------------------------------------------------------------+
```

Three layers, one direction of dependency: presentation depends on simulation,
simulation depends on nothing, and biology stands beside simulation as the model
the program is growing toward. The biology layer does not reference the
automaton at all today, which is deliberate: it can be tested and developed
without the UI, and the two can be joined later without unpicking either.

---

## Layer 1: presentation

Files: `Sources/Program.cs`, `Sources/FormLife.cs`, `Sources/FormLife.Designer.cs`,
`Sources/LifeGrid.cs`, `Sources/LifeGrid.Designer.cs`.

### `Program`

```
[STAThread] static void Main()
  Application.EnableVisualStyles()
  Application.SetCompatibleTextRenderingDefault(false)
  Application.Run(new FormLife())
```

Nothing else lives here. Startup state is built by `FormLife`'s constructor.

### `FormLife`

The orchestrator. It owns:

| Field | Type | Meaning |
| --- | --- | --- |
| `m_lifeGame` | `LifeGame` | The simulation model. |
| `m_timer` | `Timer` | Drives automatic stepping. |
| `m_gameRules` | `List<string>` | The 24 rulesets offered in the menu. |
| `m_gridSize` | `int` | Cells per side. |
| `m_simulationSpeed` | `int` | Speed preset, 1 to 10. |
| `m_stepNumber` | `int` | Generation counter. |
| `m_probability` | `double` | Density used by Randomize (0.15). |
| `m_zoomCount`, `m_zoomRate`, `m_zoomMax` | | Zoom state. |
| `m_startTime` | `DateTime` | When Start was last pressed. |

Responsibilities, in order of importance:

1. **Own the model's lifetime.** `ResizeGrid()` is the only place a `LifeGame`
   is constructed, so grid size changes always produce a fresh, consistent
   model rather than a half-cleared one.
2. **Translate intent into model calls.** Every button, menu item and key
   shortcut funnels into one of a small set of methods: `ButtonStartClick`,
   `ButtonStopClick`, `ButtonStepClick`, `ButtonResetClick`, `ButtonClearClick`,
   `ButtonRandomizeClick`, `ZoomView`.
3. **Keep the screen honest.** `UpdateFormVisuals()` is the single redraw path.
   It refreshes the generation, population and zoom labels and then pushes the
   model's arrays into the control with `LifeGrid.UpdateGrid`.
4. **Handle input.** Menu clicks, button clicks, grid mouse events and
   `ProcessCmdKey` all arrive here.

`FormLife` never paints cells itself and never computes a generation itself. All
arithmetic about cells belongs to `LifeGame`; all pixels belong to `LifeGrid`.

### `LifeGrid`

A `UserControl` that renders a boolean grid. It holds its own copy of the state
so that painting is independent of the model being mutated mid-tick.

| Member | Purpose |
| --- | --- |
| `Columns`, `Rows` | Grid dimensions. Setting either calls `CreateGrid()`, which reallocates the two state arrays. |
| `LinesVisible` | Draw the one-pixel gutter around each cell. |
| `LinesSize` | Thickness of that gutter. |
| `CellColorAlive`, `CellColorDead`, `CellColorHistory` | The three brushes. |
| `UpdateGrid(gridStates, historyStates)` | Copies model arrays in, then invalidates. Throws if the sizes do not match. |
| `LifeGridPaint` | The paint handler. |

Painting is a straightforward nested loop over rows and columns, filling one
rectangle per cell. `DoubleBuffered` is set in the constructor, which is what
keeps the animation from flickering at speed 10. The `Paint` handler is wired in
`LifeGrid.Designer.cs`, not in `FormLife`.

**Why the control copies the data.** `UpdateGrid` throws
`InvalidOperationException` when the incoming arrays are the wrong length. That
is the tripwire for the classic bug in this kind of program: the grid control
believes it is 100x100 while the model has been resized to 50x50. Making it
throw rather than silently truncate means the failure shows up at the point of
the mistake instead of as a corrupted picture.

---

## Layer 2: simulation

File: `Sources/LifeGame.cs`.

`LifeGame` is a sealed class holding four parallel arrays over the same
`columns * rows` space:

| Array | Role |
| --- | --- |
| `m_currentStates` | The board as it stands. |
| `m_newStates` | Scratch space for the generation being computed. |
| `m_historyStates` | Set once a cell has ever been alive; never cleared except by `ClearGrid`. |
| `m_startStates` | A snapshot taken by `SetStart()`, restored by `ResetGrid()`. |

Double buffering the board (`current` against `new`) is what makes a step
atomic: neighbours are read from `current` while the result is written to `new`,
so no cell can see another cell's new value within the same generation. `Step()`
then copies `new` over `current`.

### Public surface

```
LifeGame(int gridSize)                        // square, rules 23/3
LifeGame(int columns, int rows)
LifeGame(int columns, int rows, string rules)

int    Columns, Rows, Cells, LiveCellCount
string Rules
bool[] GameGrid, HistoryGrid

void ChangeRules(string rules)
void ClearGrid()      // zero everything, keep dimensions
void Randomize(double probability)
void ResetGrid()      // restore the SetStart snapshot
void SetStart()       // take the snapshot
void Step()           // advance one generation
bool GetCellState(int x, int y)
void ToggleCellState(int x, int y)
override string ToString()   // ASCII rendering, '*' alive, '.' dead
```

### Rules representation

A rule string such as `23/3` is parsed once by `ChangeRules` into two
`List<int>`s, `m_surviveRules` and `m_birthRules`. Parsing is deliberately
forgiving: any input containing a `9` or lacking a `/` falls back to `23/3`,
because 9 neighbours is impossible on an 8-neighbourhood and a rule without a
separator is meaningless.

During a step the decision for one cell is exactly:

```csharp
if ((alive && m_surviveRules.Contains(neighbors)) ||
    (!alive && m_birthRules.Contains(neighbors)))
```

`getNeighbors` sums the eight surrounding cells, guarding each edge individually
rather than using a padded array. The board is **not** toroidal: cells falling
off an edge simply count as nothing.

`AdvancePopulation` short-circuits and returns immediately when
`m_liveCells == 0`, so an empty or dead board costs nothing to run.

---

## Layer 3: biology

Files: `Sources/Biology/*.cs`. Nine files, no dependency on the UI or on
`LifeGame`. Full detail is in [BIOLOGY.md](BIOLOGY.md); this section covers the
shape of it.

| File | Contents |
| --- | --- |
| `Enums.cs` | `TaxonomyRank`, `Domain`, `Kingdom`, `AnimalPhylum`, `ChordateClass`, `ArthropodClass`, `MolluscClass`, `VertebrateClass`, `DietType`, `ReproductionMode`, `ThermalStrategy`, `Cellularity`, `LocomotionType`, `CellDivisionType`, `BodySymmetry`, `GeneOrientation`. |
| `NucleicAcid.cs` | `Nucleotide`, `NucleotideExtensions`, `StrandType`, `NucleicAcidType`, `Gene`, `NucleicAcid`, `Dna`, `Rna`. |
| `Taxonomy.cs` | `Taxon` tree with `AddChild`, `AddSpecies`, `GetAllSpecies`, `CountSpecies`, `GetLineage`; `Phylum` adds an `AnimalPhylum` identifier. |
| `Organism.cs` | `Organism` base, `UnicellularOrganism`, `MulticellularOrganism`, `Virus`. |
| `AnimalKingdom.cs` | `Species`, `AnimalKingdom` (phylum registry and `ToSpecies` projection), `Animal`, `Vertebrate`, `Invertebrate`. |
| `Animals.cs` | Fifteen concrete animals from `Mammal` to `Annelid`. |
| `Kingdoms.cs` | `Eukaryote`, `Plant`, `Tree`, `FloweringPlant`, `Moss`, `Fern`, `Fungus`, `Protist`, `Alga`, `Bacterium`, `Archaeon`. |
| `Biomes.cs` | `BiomeType`, `ClimateZone`, `ZoneType`, `HabitatTerrain`, `TrophicLevel`, `Zone`, `Biome`, `BiomeMap`. |
| `Biosphere.cs` | The world container, plus `CreateDefaultWorld()`. |

The inheritance spine for animals is:

```
Organism
└── MulticellularOrganism
    └── Animal                 (phylum, vertebrate flag, speed)
        ├── Vertebrate         (fur/feathers/scales, warm blood, gestation)
        │   ├── Mammal  Bird  Reptile  Amphibian  Fish
        └── Invertebrate       (exoskeleton, segmentation, leg count)
            ├── Insect  Arachnid  Crustacean
            ├── Cephalopod  Gastropod  Bivalve
            └── Cnidarian  Sponge  Annelid
```

Plants climb `Organism → MulticellularOrganism → Eukaryote → Plant`, and
`Protist`, `Bacterium` and `Archaeon` branch off at `Organism` and
`UnicellularOrganism` respectively because they are not multicellular.

### Biosphere as a registry

`Biosphere.Register(organism)` does four things at once:

1. adds it to the flat `m_organisms` list,
2. files it in `m_organismIndex` under its name for `Find`,
3. if it is an `Animal`, hands it to `AnimalKingdom.RegisterAnimal`, which
   resolves its phylum and stores the projected `Species`,
4. if it is a `Virus`, files it in the virus list.

It also accumulates `TotalBiomassKg`. Because `RegisterAnimal` calls
`animal.ToSpecies()`, the taxonomic tree is generated from the objects rather
than maintained by hand: change a `Mammal`'s phylum and the tree follows.

---

## Data flow

### One generation

```
Timer.Tick  ──────────────┐
                          v
                   FormLife.TimerTick
                          |
             m_lifeGame.Step()
                          |
              +-----------+------------+
              v                        v
    LifeGame.AdvancePopulation   m_newStates → m_currentStates
      read m_currentStates
      write m_newStates
      mark m_historyStates
              |
              v
       FormLife.UpdateFormVisuals
        - labels: generation, population, zoom
        - m_lifeGrid.UpdateGrid(...)
              |
              v
       LifeGrid.Invalidate → LifeGridPaint
```

### A click on a cell

```
MouseDown on LifeGrid
      |
      v
FormLife.LifeGridMouseDown
  screen → cell:  x = e.X * Columns / Grid.Width
                  y = e.Y * Rows   / Grid.Height
      |
      v
LifeGame.ToggleCellState(x, y)   ← also fixes up LiveCellCount
      |
      v
LifeGrid.UpdateGrid(...)
```

The screen-to-cell conversion is done in `FormLife`, not in `LifeGrid`, because
only `FormLife` knows the model's dimensions. `LifeGrid` deliberately has no
notion of what a "live" cell means beyond the colour it paints.

### Restart paths

| Command | Effect on `current` | Effect on `start` | Effect on `history` |
| --- | --- | --- | --- |
| `ClearGrid` | zeroed | untouched | zeroed |
| `Randomize` | randomised | untouched | untouched |
| `SetStart` | untouched | replaced with `current` | untouched |
| `ResetGrid` | replaced with `start` | untouched | untouched |
| `ChangeRules` | untouched | untouched | untouched |
| `ResizeGrid` | new `LifeGame` entirely | new | new |

---

## The ruleset format

`survive/birth`, optionally followed by a space and a description:

```
23/3          - Conway's Life
1358/357      - Amoeba
34678/0123478 - Inverse Life
/2            - Seeds            (nothing survives; 2 neighbours births)
```

Rules shipped with the program:

| Rule | Name | Rule | Name |
| --- | --- | --- | --- |
| `23/3` | Conway's Life | `34678/0123478` | Inverse Life |
| `125/36` | 2x2 | `5/345` | Long Life |
| `34/34` | 34 Life | `12345/3` | Maze |
| `1358/357` | Amoeba | `1234/3` | Mazectric |
| `35678/4678` | Anneal | `245/368` | Move |
| `4567/345` | Assimilation | `238/357` | Pseudo Life |
| `235678/378` | Coagulations | `1357/1357` | Replicator |
| `45678/3` | Coral | `/2` | Seeds |
| `34678/3678` | Day & Night | `/234` | Serviettes |
| `5678/35678` | Diamoeba | `235678/3678` | Stains |
| `012345678/3` | Flakes | `2345/45678` | Walled cities |
| `1/1` | Gnarl | `23/36` | HighLife |

To add one, append the formatted string to `m_gameRules` in
`FormLife.AddRulesToComboBox()`. Nothing else needs changing: the menu, the
sidebar drop-down and the change-rules path are all driven from that list.

---

## Threading and timing

The program is single-threaded. `System.Windows.Forms.Timer` is used rather than
`System.Timers.Timer` or a background worker, for one reason: its `Tick` fires
on the UI thread, so no lock is needed to touch the grid or the controls.

Interval is computed from the speed preset:

```
m_timer.Interval = (11 - m_simulationSpeed) * 10;
```

giving 100 ms at speed 1 down to 10 ms at speed 10.

The cost of this choice is that a slow step (250x250 is 62,500 cells, each
reading eight neighbours) blocks the message loop for the duration of the step.
At the shipped sizes it is comfortably fast enough. Moving the model onto a
worker thread would require marshalling paints back to the UI thread and locking
around `UpdateGrid`; that is the natural first step if the grid ever grows much
larger.

`m_startTime` is recorded on Start and is the basis for the elapsed-time readout
in the sidebar's statistics group.

---

## Design decisions and their reasons

**Boolean arrays rather than object cells.** 62,500 `bool`s is 62 KB, and
stepping is a tight loop over contiguous memory with no pointer chasing and no
allocation per generation. An object per cell would allocate hundreds of
thousands of objects per second.

**The history layer is separate from the live layer.** Keeping three arrays
rather than one array of a three-state enum means the painting loop stays a
simple branch on two booleans and the history array can be swapped out without
touching the live logic.

**The grid control owns its own copy.** It keeps the paint path independent of
the model, at the cost of one array copy per update. See the note under
[Layer 1](#layer-1-presentation) for why the size mismatch throws.

**Rules parsed to integer lists, not compiled delegates.** Parsing costs a few
microseconds once, and `List<int>.Contains` over at most nine entries is a
handful of comparisons. This keeps `ChangeRules` able to run mid-simulation
without invalidating anything.

**Biology kept separate from the automaton.** The biological model is a
library of types first and a game feature second. Wiring organisms onto grid
cells would force dimensions, coordinates and lifetimes into classes that do not
need them.

**Namespaced as `LifeEngine.Biology`.** The presentation and simulation types sit
in the root namespace; everything biological sits below it, so it is obvious at
a glance which half of the project a file belongs to.

---

## Known gaps

- The biology layer is not yet connected to the grid. Organisms do not occupy
  cells, do not compete and do not affect a simulation. This is the next major
  piece of work and is listed first in the README roadmap.
- `Organism.Reproduction` and `Thermal` are set per concrete species but never
  consulted by any simulation logic.
- There is no persistence. Neither the grid nor a biosphere can be saved or
  loaded.
- `Randomize` constructs a `new Random()` on every call. Called twice within the
  same clock tick, that can produce identical sequences. A single shared
  instance would fix it.
- `LifeGrid` recreates its brushes on every paint. Fine at these sizes, wasteful
  if the grid grows.
