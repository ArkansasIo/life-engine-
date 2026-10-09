
# LifeEngine — systems reference

A description of every system in the program: what it is for, where it lives,
what it exposes, and what state it is in.

**This document describes what exists in the source, not what is reachable from
the interface.** Where the two differ, it says so, because that distinction
matters more than anything else in this project right now.

Last checked: build succeeds, 0 errors, 0 warnings, 62 source files,
27,115 lines.

---

## Contents

- [The short version](#the-short-version)
- [How to read the status column](#how-to-read-the-status-column)
- [The systems](#the-systems)
  - [1. Simulation](#1-simulation)
  - [2. Biology](#2-biology)
  - [3. World map](#3-world-map)
  - [4. Ecosystem](#4-ecosystem)
  - [5. Wolves](#5-wolves)
  - [6. Combat](#6-combat)
  - [7. Attributes](#7-attributes)
  - [8. Talents](#8-talents)
  - [9. Per-cell AI](#9-per-cell-ai)
  - [10. Scripting](#10-scripting)
  - [11. Configuration](#11-configuration)
  - [12. Scheduler](#12-scheduler)
  - [13. Updates](#13-updates)
  - [14. Data and export](#14-data-and-export)
  - [15. Interface](#15-interface)
  - [16. Assets](#16-assets)
- [Numbers](#numbers)
- [What is not done](#what-is-not-done)

---

## The short version

LifeEngine began as Conway's Game of Life with a biology library beside it. It
now contains sixteen systems. The Conway engine works end to end and is what you
can use; the other fifteen execute and pass a self-check, and are reachable from
the interface.

If you are picking this up cold, read [What is not
done](#what-is-not-done) before you read anything else.

---

## How to read the status column

| Status | Means |
| --- | --- |
| **Working** | Wired into the running program and exercised by using it. |
| **Verified** | Exercised by `--selftest`, which checks the answers. |
| **Compiles** | Builds clean. A compiler accepted it and nothing more. |

`Sources/Tools/SelfTest.cs` covers every system: 125 checks, currently all
passing. "Compiles" now means a compiler accepted the code *and* the self-check
exercised it; it is the weakest claim here and applies to the code no check
reaches.

---

## The systems

### 1. Simulation

**Status: Working.** This is the part you can actually use.

| | |
| --- | --- |
| Files | `Sources/LifeGame.cs`, `Sources/LifeGrid.cs`, `Sources/LifeGrid.Designer.cs` |
| Namespace | `LifeEngine` |

The cellular automaton. A square grid of booleans, a ruleset in `survive/birth`
notation, and a step that reads the current board while writing the next one, so
a generation is atomic.

`LifeGame` holds four arrays over the same `columns * rows` space:

| Array | Role |
| --- | --- |
| `m_currentStates` | The board as it stands |
| `m_newStates` | Scratch for the generation being computed |
| `m_historyStates` | Latched once a cell has ever been alive |
| `m_startStates` | Snapshot taken by `SetStart()`, restored by `ResetGrid()` |

Twenty-four rulesets ship with it, from Conway's Life (`23/3`) through HighLife,
Day & Night and Inverse Life to Walled Cities. Parsing is forgiving: anything
containing a `9` or lacking a `/` falls back to `23/3`.

The board is **not** toroidal. Cells falling off an edge count as nothing.

`LifeGrid` is a `UserControl` that keeps its own copy of the arrays and paints
one rectangle per cell. It throws `InvalidOperationException` if handed arrays
of the wrong length — that is the tripwire for the classic bug where the control
believes it is 100×100 while the model has been resized to 50×50.

Grid sizes: 25 to 250 a side. Speed presets 1, 2, 5, 10, rendered as a timer
interval of `(11 - speed) * 10` milliseconds.

---

### 2. Biology

**Status: Compiles.** Thirteen files, 5,287 lines.

| | |
| --- | --- |
| Files | `Sources/Biology/*.cs` |
| Namespace | `LifeEngine.Biology` |

The taxonomy of life. Nothing here references the grid or the interface, so it
is a library first and a game feature second.

| File | Contents |
| --- | --- |
| `Enums.cs` | 16 enums: `TaxonomyRank`, `Domain`, `Kingdom`, `AnimalPhylum`, `ChordateClass`, `ArthropodClass`, `MolluscClass`, `VertebrateClass`, `DietType`, `ReproductionMode`, `ThermalStrategy`, `Cellularity`, `LocomotionType`, `CellDivisionType`, `BodySymmetry`, `GeneOrientation` |
| `NucleicAcid.cs` | `Nucleotide`, `Gene`, `NucleicAcid`, `Dna`, `Rna`; transcription (T→U) and complementation (A-T / A-U, C-G) |
| `Taxonomy.cs` | `Taxon` (eight-rank tree), `Phylum` |
| `AnimalRanks.cs` | `Class` rank type with aliases |
| `Organism.cs` | `Organism` base, `UnicellularOrganism`, `MulticellularOrganism`, `Virus` |
| `AnimalKingdom.cs` | `Species`, `AnimalKingdom`, `Animal`, `Vertebrate`, `Invertebrate` |
| `Animals.cs` | 15 concrete animals from `Mammal` to `Annelid` |
| `Kingdoms.cs` | `Plant`, `Tree`, `FloweringPlant`, `Moss`, `Fern`, `Fungus`, `Protist`, `Alga`, `Bacterium`, `Archaeon` |
| `Biomes.cs` | `BiomeType`, `ClimateZone`, `ZoneType`, `HabitatTerrain`, `TrophicLevel`, `Zone`, `Biome`, `BiomeMap` |
| `Biosphere.cs` | World container, `CreateDefaultWorld()` |
| `AnimalCatalogue.cs` | Every animal phylum, class and order with rank and common name |
| `WorldMap.cs` | See [3. World map](#3-world-map) |
| `WolfPack.cs` | See [5. Wolves](#5-wolves) |

`Biosphere.CreateDefaultWorld()` builds an Earth-like world: five biomes
(rainforest, desert, tundra, ocean, lake), their zones and terrains, and
eighteen organisms with scientific names, masses, diets and genes.

`AnimalCatalogue.BuildAnimalKingdom` fills in the **whole** animal kingdom —
every phylum including the twelve with no creatures yet — so the tree reads as
a real classification rather than only the implemented parts.

---

### 3. World map

**Status: Compiles.**

| | |
| --- | --- |
| File | `Sources/Biology/WorldMap.cs` |
| Namespace | `LifeEngine.Biology` |

Generates a planet from a seed. Deterministic: the same seed always builds the
same world.

The generation runs in five passes:

1. **Latitude bands.** Twelve bands from pole to pole, each with a base
   temperature, a base rainfall and a climate zone.
2. **Elevation.** Two octaves of value noise — a coarse continent shape plus
   finer detail — scaled to metres. Below sea level is water.
3. **Rivers.** Walks downhill from high ground, cutting a course to the sea.
4. **Coastlines.** Marks every land tile touching water.
5. **Biomes.** Classifies each tile from its temperature, rainfall, elevation
   and whether it is water.

`Tile` carries position, latitude, longitude, temperature, elevation, rainfall,
biome, climate zone, terrain, zone, river and coastal flags, and the populations
living on it.

The `Noise` class is a self-contained value-noise implementation with no
dependencies.

---

### 4. Ecosystem

**Status: Compiles.**

| | |
| --- | --- |
| Files | `Sources/Ecosystem/Ecosystem.cs`, `Sources/Ecosystem/WorldFactory.cs` |
| Namespace | `LifeEngine.Ecosystem` |

What lives on the world map, and how it changes.

`Species` is a habitat envelope plus rates: temperature, rainfall and elevation
ranges, water/land/coast requirements, desert and ice avoidance, growth rate,
death rate, carrying capacity and a seed population. It answers one question —
`suitability(tile)` — and the rest follows from that.

`Ecosystem.Step()` grows producers first, then consumers in trophic order, so a
herbivore eats this tick's grass rather than last tick's. Growth is logistic:
each tile moves a fraction of the way toward its target, so populations approach
a carrying capacity instead of exploding.

Twelve species ship by default in three tiers:

| Tier | Species |
| --- | --- |
| Producers | Grassland Grass, Temperate Forest, Jungle Canopy, Algal Bloom |
| Primary consumers | Honey Bee, Common Frog, Atlantic Salmon, Garden Spider |
| Secondary | Grey Wolf, Golden Eagle, Common Octopus |
| Apex | Orca |

`WorldFactory.RenderMap` draws the world as text, one character per tile.

---

### 5. Wolves

**Status: Compiles.**

| | |
| --- | --- |
| Files | `Sources/Biology/WolfPack.cs`, `Sources/Wolf/WolfLogic.cs` |
| Namespaces | `LifeEngine.Biology`, `LifeEngine.Wolves` |

A pack with a dominance order, and the decisions that order drives.

`Wolf` is an individual: name, age, mass, health, energy, wounds, kills, and a
`WerewolfTraits` block of eight traits (strength, speed, aggression, hearing,
scent, night sight, healing, human retention) plus curse and shapeshifter flags.

`WolfPack` owns the order. Ranks are **rebuilt rather than assigned**:
`SettleRanks()` sorts the living adults by `FightingPower()` and hands out
Alpha, Second, Beta, Omega. Any wolf that wins enough fights climbs.

`WolfBehaviour` (in `Sources/Wolf/WolfLogic.cs`) holds the decisions, as static
methods with no state:

| Method | What it does |
| --- | --- |
| `Hunt` | Resolves a hunt. Pack chance from its three best hunters, the alpha's condition, cohesion and darkness; prey chance from speed and defence |
| `Howl` | A call, and who answers |
| `Night` | Herd grows, pack hunts, order shifts |
| `Moonshift` | Every cursed wolf loses grip on its human mind |
| `Season` | Ageing, breeding, dispersal |
| `FindNewPack` | Founds a pack from wolves that left another |
| `TakeHumanForm` | Back to a human shape, for a wolf that still can |

Sharing follows rank: the alpha eats first and the omega last.

---

### 6. Combat

**Status: Compiles.** Six files, 2,596 lines.

| | |
| --- | --- |
| Files | `Sources/Combat/*.cs` |
| Namespace | `LifeEngine.Combat` |

Turn-based fights between two sides, with lasting effects.

**`Effects.cs`** — buffs, debuffs and conditions. An `Effect` is data: a name, a
target stat, an amount, a duration and a stacking rule. Four stacking modes:

| Mode | Behaviour |
| --- | --- |
| `Strongest` | Only the best applies |
| `Additive` | Every stack counts — right for bleeding |
| `Refresh` | Renews the clock, keeps one instance — right for a blessing |
| `Multiplicative` | Compounds, so two 50% bonuses give 125% |

That distinction matters: three stacks of bleeding should hurt three times as
much, but three stacks of courage should not make a wolf nine times as brave.

**`EffectsCatalogue.cs`** — 22 effects. Nine buffs (Rallied, Fed, Night Eyes,
Home Ground, Braced, Cornered, Moon-Mad, Calloused, Second Wind), seven debuffs
(Lamed, Frightened, Blinded, Winded, Exhausted, Outnumbered, Silver Brand) and
six lasting conditions (Bleeding, Poisoned, Infected, Lame Forever, Moon-Bound,
Starving). Conditions tick damage each round and some never wear off.

**`Combatant.cs`** — a fighter built from a wolf's traits, then owning its own
derived numbers so effects can move them without writing back into the wolf
until the fight ends. Every number is read through `GetStat(target)`, which adds
the effects and clamps the result — evasion cannot exceed 0.95, damage reduction
cannot exceed 0.8, because neither makes for a fight.

**`CombatEngine.cs`** — the fight. Turn-based, initiative order, one action per
fighter per round, then recovery. `BattleRules` sets the round limit, whether
fighters may run, the morale threshold for running, damage variance and whether
effects tick.

**`BattleLog.cs`** — every blow, effect and death as it happens, with a `Reason`
on each decision so the AI can explain itself.

**`Bestiary.cs`** — 15 preset creatures: five prey (Red Deer, Stag, Hare, Wild
Boar, Salmon), five rivals (Scout, Intruder, Rival Alpha, Lone Wolf, Moon-Mad
Wolf), four predators (Brown Bear, Lynx, Wolverine, Golden Eagle) and two others.

---

### 7. Attributes

**Status: Compiles.**

| | |
| --- | --- |
| File | `Sources/Attributes/Attributes.cs` |
| Namespace | `LifeEngine.Attributes` |

Eight raw attributes: Strength, Dexterity, Constitution, Intelligence,
Perception, Presence, Willpower, Speed.

Each `AttributeScore` keeps **base, trained and progress separately**. A score
only rises by being *used*: `Exercise(effort)` accumulates progress and grants a
point when it fills. A creature improves through what it does, not by
assignment.

`AttributeSet` derives everything else on demand rather than storing it, so
there is no second copy to go stale:

| Derived | From |
| --- | --- |
| `MaxHealth` | Constitution × size multiplier |
| `DamageBonus` | Strength |
| `Evasion` | Dexterity, Speed |
| `Accuracy` | Perception, Dexterity |
| `Resolve` | Willpower — resists fear and the moon |
| `Initiative` | Speed, Perception |
| `Leadership` | Presence |
| `SkillPointsPerLevel` | Intelligence |
| `CarryCapacity` | Strength, Constitution |

---

### 8. Talents

**Status: Compiles.**

| | |
| --- | --- |
| File | `Sources/Talents/TalentTree.cs` |
| Namespace | `LifeEngine.Talents` |

24 talents across six branches, four tiers deep, three ranks each.

| Branch | Chain |
| --- | --- |
| Ferocity | Savage Bite → Rending Claws → Throat Seeker → Blood Fury |
| Swiftness | Fleet Foot → Slip Away → Pouncing Strike → Tireless Pursuit |
| Endurance | Thick Hide → Tough It Out → Quick Mender → Undying Will |
| Pack | Pack Voice → Flanking Instinct → Rallying Howl → Alpha Presence |
| Instinct | Keen Scent → Night Sight → Silent Step → Reading the Enemy |
| Moon | Moon Tether → Willing Change → Silver Resistance → Moon Mastery |

Effects are written **once at rank-one strength**;
`TalentEffect.Scaled(rank)` is the only place rank is applied, so the numbers
cannot drift out of sync.

Talents are gated on attribute scores — `Throat Seeker` needs Perception 13,
`Alpha Presence` needs Presence 16. `TalentSet.Learn` refuses anything the
creature does not qualify for. `Refund` refuses to give back a talent that
something else depends on, because that would leave the creature holding a
talent it does not qualify for.

Nine talents unlock named abilities: Rend, Slip Away, Blood Fury, Tireless
Pursuit, Regenerate, Pack Flank, Rally, Stalk, Shapeshift.

---

### 9. Per-cell AI

**Status: Compiles.**

| | |
| --- | --- |
| Files | `Sources/Ai/Difficulty.cs`, `Sources/Ai/CellAi.cs` |
| Namespace | `LifeEngine.Ai` |

Every cell gets its own brain, and the difficulty is a table rather than a
single number.

`DifficultyProfile` carries thirteen settings per level. The nine levels:

| Level | Name | Vision | Lookahead | Blunder | Capabilities |
| --- | --- | --- | --- | --- | --- |
| 1 | Idiot | 1 | 0 | 70% | — |
| 2 | Feeble | 1 | 0 | 50% | — |
| 3 | Weak | 2 | 1 | 30% | — |
| 4 | Cautious | 2 | 1 | 18% | — |
| 5 | Even | 3 | 2 | 10% | — |
| 6 | Sharp | 3 | 2 | 5% | retreat |
| 7 | Cunning | 4 | 3 | 2% | retreat, coordinate |
| 8 | Ruthless | 4 | 3 | 0% | retreat, coordinate, bait |
| 9 | Alpha | 5 | 4 | 0% | all, and no hesitation |

Nothing cheats. A level 9 opponent wins by seeing further and not making
mistakes, not by having extra resources.

`CellAi` scores **every** legal action — wait, defend, move, attack, rest, spawn,
retreat, approach, bait, rally — and takes the best, weighted by the profile.
Nothing is hard-coded as "if enemy then attack", so changing a weight changes the
behaviour without touching the logic.

`CellView` is what a cell can see, connected to the board by three delegate
functions rather than holding the grid, so it works against a grid, a world map
or a test fixture.

`CellAiBoard` runs it: each cell decides and acts in fixed order, so the same
board and seed always give the same result.

---

### 10. Scripting

**Status: Compiles.** Seven files, 3,834 lines.

| | |
| --- | --- |
| Files | `Sources/Scripting/*.cs` |
| Namespace | `LifeEngine.Scripting` |

A Lua-style language with an object layer, implemented from the lexer up.

| File | Contents |
| --- | --- |
| `Lexer.cs` | Tokens, including indentation-aware newlines |
| `Values.cs` | `Value`, `ScriptClass`, `Scope` — tables that are both arrays and records |
| `Parser.cs` | Recursive descent to a single `Node` type with a kind |
| `Interpreter.cs` | Tree walk, with a step limit so a runaway loop cannot hang the program |
| `StandardLibrary.cs` | `print`, maths, string and table functions |
| `ScriptHost.cs` | `IScriptHost`, a headless host, and the LifeEngine binding |
| `ScriptManager.cs` | Loads, runs and reports on scripts |

The language is Lua's grammar plus what makes the object layer readable:
`class ... extends ... end`, `new`, `self`, `super`, `import`.

A script reaches the program only through `IScriptHost` — grid, sim, rules,
state and log. Nothing else is exposed, which is what keeps a script from
touching the interface directly.

`Sources/Scripts/example.lua` is a worked example: a `Pattern` class with three
subclasses (Block, Blinker, Glider) that stamp themselves onto the grid, plus
density, bounding-box and crowded-cell analysis.

---

### 11. Configuration

**Status: Compiles.**

| | |
| --- | --- |
| Files | `Sources/Configuration/Config.cs`, `ConfigFile.cs`, `ConfigDefaults.cs`, `VersionInfo.cs` |
| Namespace | `LifeEngine.Configuration` |

43 settings declared in one place, with types, ranges and descriptions. A file
that mentions an unknown key is reported and ignored; a file that omits a key
leaves that setting at its default — which is what lets an old settings file
still load after a new setting is added.

`ConfigFile` reads and writes an ini format. It tries to write next to the
executable and falls back to `%APPDATA%` if that folder is not writable.

`Sources/LifeEngine.settings` is the same list written out as a documented file.

`VersionInfo` reads product, version, build, build date, developer and copyright
from the assembly, with a build-name parser and comparator
(`2026.10.07-r3` — date plus revision).

---

### 12. Scheduler

**Status: Compiles.**

| | |
| --- | --- |
| File | `Sources/Configuration/Scheduler.cs` |
| Namespace | `LifeEngine.Configuration` |

Cron-style jobs. Six repeat modes: `Once`, `Interval`, `Daily`, `Weekly`,
`Monthly`, `TimesOfDay`. Interval text is written the way a person writes it —
`30s`, `5m`, `2h`, `1d12h`.

Eleven actions: Step, Start, Stop, Clear, Randomize, Reset, RunEcosystem,
NewWorld, CheckForUpdates, FlushLog, Snapshot.

A `Schedule` knows nothing about timers or threads. It answers one question:
given the time now, is it due, and when is it next due? The host runs whatever
is due on its own timer, on the UI thread, so jobs can touch the grid safely.

---

### 13. Updates

**Status: Compiles.**

| | |
| --- | --- |
| File | `Sources/Configuration/UpdateSystem.cs` |
| Namespace | `LifeEngine.Configuration` |

Releases, channels and a planner. Three release kinds — `Full`, `Patch`,
`Breaking` — because a patch carries only what changed.

`PatchPlanner` decides whether a release may be applied to the build in hand:
is it newer, does the minimum version allow it, and can it be applied alone or
must everything before it go on first. `PlanApplyOrder` sorts pending releases
oldest first and drops patches that a later full release already contains.

Nothing here downloads anything. It decides; the installer does.

`Updates/updates.xml` is the manifest, `Updates/build.txt` the build stamp and
`Updates/RELEASE-NOTES.md` the human-readable changelog.

---

### 14. Data and export

**Status: Compiles.**

| | |
| --- | --- |
| Files | `Sources/Data/SpreadsheetWriter.cs`, `Database.cs`, `ExportBuilder.cs`; `Sources/Data/schema.sql`, `seed.sql` |
| Namespace | `LifeEngine.Data` |

`DataTable` is the lowest common shape between a SQL result, an Excel sheet and
a CSV file, so one exporter serves all three.

`SpreadsheetWriter` writes .xls through the Jet provider, which is part of
Windows on any machine with Office or the Access runtime. Where it is not
present the caller gets a clear failure and falls back to CSV, which needs
nothing but a file system.

`Database` knows the shape of fifteen tables and turns the program's objects
into rows. It holds no connection open — each write opens one, does its work and
closes it, because the interface drives this from the UI thread.

`ExportBuilder` builds thirteen tables straight from the live objects, so an
export works whether or not a database is configured.

`schema.sql` is written for SQLite but is plain SQL-92 apart from two pragmas
and `AUTOINCREMENT`. It includes four reporting views: `v_generation_growth`,
`v_biome_census`, `v_species_totals`, `v_pack_order`, `v_hunt_summary`.
`seed.sql` carries the 24 rulesets, the releases, the default jobs, all 43
settings and a small worked example — a blinker run and a three-wolf pack.

---

### 15. Interface

**Status: Compiles. Never run.**

| | |
| --- | --- |
| Files | `Sources/Gui/*.cs` |
| Namespace | `LifeEngine.Gui` |

The new interface, written but never launched.

| File | Contents |
| --- | --- |
| `AppContext.cs` | Owns every subsystem and the shared random stream. Also implements `IScriptHost` |
| `MainForm.cs` | Ribbon, tabbed sidebar, status bar, keyboard and mouse input |
| `WorldPanel.cs` | The World tab: map, species table, tick controls |
| `WorldMapView.cs` | Draws a `WorldMap` — one tile per block, coloured by biome, with the living population on top |
| `WolfPanel.cs` | The Wolves tab: roster sorted by fighting power, hunt log, alpha banner |
| `ToolsPanel.cs` | Scripts, settings, scheduler and updates, one behind a sub-tab each |

`AppContext` exists so any panel can ask for what it needs rather than the form
passing six objects around. It also owns the **one** thing that must be shared
rather than duplicated: the random stream. Two panels with their own `Random`
would give different results from the same seed.

**`Program.cs` still launches the old `FormLife`.** The new interface is one line
away from running and has never been executed. See [What is not
done](#what-is-not-done).

---

### 16. Assets

**Status: Generated.**

| | |
| --- | --- |
| Files | `Sources/Tools/IconGenerator.cs`, `Sources/Resources/*.png` |

33 PNG icons at 32×32, black on transparent, flat with no anti-aliasing.

The generator is a build-time tool: it draws each icon from a few filled
rectangles, polygons and circles and writes them out. It is kept in the tree so
the icons can be regenerated or added to rather than being unreadable binaries
of unknown provenance.

The icons are embedded as raw PNGs and read by name in
`Properties/Resources.Designer.cs`. The form's icon comes from `Icon.ico`
through `ApplicationIcon`. Both routes the .NET Framework runtime can read
natively — which matters, because the alternative crashed the program on
startup. See [What is not done](#what-is-not-done).

---

## Numbers

| | |
| --- | --- |
| Source files | 62 |
| Source lines | 27,115 |
| Enums | 43 |
| Rulesets | 24 |
| Settings | 43 |
| Effects | 22 |
| Talents | 24 |
| Bestiary creatures | 15 |
| Difficulty levels | 9 |
| Species in a default world | 12 |
| Icons | 33 |
| SQL tables | 15 |
| SQL views | 5 |
| Self-check assertions | **125 (all passing)** |

Lines by area:

| Area | Files | Lines |
| --- | --- | --- |
| Biology | 13 | 5,287 |
| Scripting | 7 | 3,891 |
| Gui | 6 | 2,869 |
| Combat | 6 | 2,596 |
| Configuration | 6 | 2,239 |
| Data | 3 | 1,602 |
| Root | 7 | 1,392 |
| Tools | 2 | 1,529 |
| Ai | 2 | 1,306 |
| World | 2 | 1,302 |
| Ecosystem | 2 | 1,031 |
| Talents | 1 | 999 |
| Wolf | 1 | 524 |
| Attributes | 1 | 386 |
| Properties | 3 | 162 |

---

## What is not done

This is the honest section. Read it before trusting anything above.

### The interface has never run

`Program.cs` now launches the new `Gui.MainForm`, with a fallback to the original
2017 `FormLife` if construction throws. The ribbon menu, tabbed sidebar and
status bar in `Sources/Gui/` compile and construct. They have not been exercised
by a person clicking through them — the fallback exists precisely because that
has not been done.

### Every system has now executed

This section used to say fifteen of sixteen systems had never run. That is no
longer true. `Sources/Tools/SelfTest.cs` is a harness of 125 checks covering all
thirteen systems, and it is run with:

```
LifeEngine.exe --selftest
```

Current result: **125 passed, 0 failed**. Running it for the first time found
ten real defects, all now fixed — see the changelog at the end of this document.

"Compiles" still means a compiler accepted the code. It no longer means the code
has never run: it means a check has run it and looked at the answer.

### There are tests

The earlier claim that there were none was wrong. There is no *test framework*
and no test project, but `SelfTest.cs` is a working harness with 125 assertions
across simulation, world map, ecosystem, wolves, combat, attributes, talents,
per-cell AI, scripting, configuration, scheduler, weather and export. It returns
a non-zero exit code when a check fails, so it can gate a build.

It is not a substitute for a unit-test framework — the checks are coarse and
there is no isolation between them — but "no tests" was simply false.

### The systems do not know about each other

`CellAi.Strength` is a plain `int` set by hand; nothing reads `AttributeSet` or
`TalentSet`. Combat does not read talents. The ecosystem does not read the
world map's rivers. Each system is complete in itself and connected to nothing.

The obvious first connections, in order of value:

1. Derive a cell's combat numbers from its attributes and talents.
2. Let `WolfPack` fights go through `CombatEngine` instead of the simpler
   `WolfPack.Challenge`.
3. Let the ecosystem's species carry attributes and talents.

### The biology layer is not bound to the grid

Organisms do not occupy cells, do not compete and do not die. The biological
model describes life; it does not simulate it.

### Known gaps

- No persistence for a run. The database layer can write one but nothing calls
  it.
- No save or load for the grid, in RLE or any other format.
- `Randomize` constructs a `new Random()` per call; two calls in the same tick
  can produce identical sequences.
- `LifeGrid` recreates its brushes on every paint.
- `Organism.Reproduction` and `Thermal` are set per species and never read.
- `TrophicLevel` is defined and never assigned in the biology layer.
- One style warning remains in `TalentTree.cs` about an object initialiser.

### A defect worth remembering

Converting the project to SDK style set
`GenerateResourceUsePreserializedResources`, which rewrote the `.resx` files
into a format the .NET Framework runtime cannot read without a binding redirect
the SDK does not generate. The program crashed on its first call into the
resource class.

The fix was to remove the need entirely: the icons are embedded as raw PNGs read
by name, the form's icon comes from `Icon.ico` through `ApplicationIcon`, and
the `.resx` files were deleted. Both routes the runtime reads natively.

If anyone reintroduces a `.resx` with a non-string resource, this will come
back.

---

## Changelog — first run of the self-check

Running `--selftest` for the first time found ten defects. All are fixed; the
harness now reports 125/125. They are recorded here because each one is a
trap that can be walked back into.

### Two were test bugs, not code bugs

- **Moon brightness read as zero.** `MoonBrightness()` reads the cycle's own day
  (`m_day`), but the check called `MoonAt(day)` — a pure query that does not move
  the clock — and then read the brightness, so it never changed. The check now
  advances the cycle with `AdvanceDays(1)`.
- **Talent refund expectation.** `Refund` gives back *one rank at a time* and
  only refuses when the talent is at Basic and something depends on it. The
  check refunded a talent at Improved and expected a refusal. It now drops to
  Basic first, then asserts the second refund is refused.

### Three were real bugs in the script parser

- **`print(x)` as a statement.** `print` is a lexer keyword, and
  `ParseStatement` routed it to `ParseAssignment`, which demands a Name token. It
  failed with "Expected a name but found 'print'". `print` is now treated as a
  global function in both `ParseTarget` and `ParsePrimary`.
- **Class fields.** `ParseClass` skipped newlines with `Current()`, which skips
  them *virtually* — so testing the returned token for `NewLine` could never
  succeed, and `m_position` was left on a newline. `value = 0` then failed with
  "Expected '=' after the field name". Fixed with `SkipNewLines()`.
- **The same bug in `ParseBlock`.** A statement beginning with `self` failed
  because `Advance()` consumed the newline instead of the token. `ParseBlock` now
  calls `SkipNewLines()` before each statement.

### One was a real bug in assignment parsing

- **`Unknown operator '2'`.** `MatchOperator("=")` *consumes* the `=`, but the
  compound-assignment detection then read `Current().Text` to decide whether the
  operator was `=` or `+=`. For `x = 2 + 3` that read the value `2`, so a bogus
  binary node with operator `"2"` was built and the interpreter rejected it. The
  operator is now captured at the point it is consumed.

### Two were real bugs in the interpreter

- **Method parameters were never bound.** A method node stored its parameters at
  `Children[0]`, but both call paths read `Children[1]` (a free function has its
  target there). Every method argument read as nil. A `self` placeholder now
  occupies `Children[0]` so a method and a function have the same shape.
- **`obj.method()` did not bind `self`.** A dot call parses to `Field` + `Call`,
  and `EvaluateCall` went straight to `CallValue`, which leaves `self` nil. Only
  the `:` form bound a receiver. `EvaluateCall` now binds the receiver as `self`
  when the callee is a field, so a dot call on a method behaves as expected.

### Two were mine

- The `WolfLogic` namespace shadowed the `WolfLogic` class, so callers wrote
  `WolfLogic.WolfLogic.Night(...)`. Renamed to namespace `LifeEngine.Wolves` and
  class `WolfBehaviour`.
- `SelfTest` had its own `Main`, which collided with `Program.Main` — the project
  did not build at all until one was removed. `SelfTest.Main` became
  `SelfTest.Run`, selected by the `--selftest` argument.
