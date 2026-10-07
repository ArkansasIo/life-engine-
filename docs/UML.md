# UML

Diagrams for LifeEngine: class structure, runtime interactions, object
lifecycles, activities and use cases. All diagrams are Mermaid, so they render
directly on GitHub and in any Mermaid-aware Markdown viewer.

---

## Contents

- [1. Class diagram: simulation](#1-class-diagram-simulation)
- [2. Class diagram: biology](#2-class-diagram-biology)
- [3. Class diagram: animal kingdom](#3-class-diagram-animal-kingdom)
- [4. Class diagram: molecular biology](#4-class-diagram-molecular-biology)
- [5. Class diagram: ecology](#5-class-diagram-ecology)
- [6. Sequence: one generation](#6-sequence-one-generation)
- [7. Sequence: clicking a cell](#7-sequence-clicking-a-cell)
- [8. Sequence: building the default world](#8-sequence-building-the-default-world)
- [9. State: simulation lifecycle](#9-state-simulation-lifecycle)
- [10. State: a cell](#10-state-a-cell)
- [11. Activity: start-up](#11-activity-start-up)
- [12. Activity: sizing the grid](#12-activity-sizing-the-grid)
- [13. Use cases](#13-use-cases)

---

## 1. Class diagram: simulation

```mermaid
classDiagram
    class Program {
        +Main() void
    }

    class FormLife {
        -double m_probability
        -int m_gridSize
        -int m_simulationSpeed
        -int m_stepNumber
        -List~string~ m_gameRules
        -int m_zoomCount
        -int m_zoomRate
        -DateTime m_startTime
        +FormLife()
        -ResizeGrid() void
        -AddSizesToComboBox() void
        -AddSpeedsToComboBox() void
        -AddRulesToComboBox() void
        -ZoomView(ZoomType) void
        -UpdateFormVisuals() void
        -TimerTick(sender, e) void
        -LifeGridMouseDown(sender, e) void
        -ButtonStartClick(sender, e) void
        -ButtonStopClick(sender, e) void
        -ButtonStepClick(sender, e) void
        -ButtonResetClick(sender, e) void
        -ButtonClearClick(sender, e) void
        -ButtonRandomizeClick(sender, e) void
        -SizeComboboxSelectedIndexChanged(sender, e) void
        -SpeedComboboxSelectedIndexChanged(sender, e) void
    }

    class LifeGame {
        -bool[] m_currentStates
        -bool[] m_newStates
        -bool[] m_historyStates
        -bool[] m_startStates
        -int m_rows
        -int m_columns
        -int m_cells
        -int m_liveCells
        -List~int~ m_surviveRules
        -List~int~ m_birthRules
        -string m_unparsedRules
        +LifeGame(int gridSize)
        +LifeGame(int columns, int rows, string rules)
        +ChangeRules(string rules) void
        +ClearGrid() void
        +Randomize(double probability) void
        +ResetGrid() void
        +SetStart() void
        +Step() void
        +GetCellState(int x, int y) bool
        +ToggleCellState(int x, int y) void
        -AdvancePopulation() void
        -getNeighbors(int x, int y) int
        +ToString() string
    }

    class LifeGrid {
        -bool[] m_gridStates
        -bool[] m_historyStates
        -Color m_cellColorAlive
        -Color m_cellColorDead
        -Color m_cellColorHistory
        -bool m_gridLinesVisible
        -float m_gridLineThickness
        -int m_columns
        -int m_rows
        +int Columns
        +int Rows
        +bool LinesVisible
        +float LinesSize
        +Color CellColorAlive
        +Color CellColorDead
        +Color CellColorHistory
        +UpdateGrid(bool[], bool[]) void
        -CreateGrid() void
        -LifeGridPaint(sender, e) void
    }

    class Timer {
        +int Interval
        +bool Enabled
        +Start() void
        +Stop() void
    }

    Program ..> FormLife : creates
    FormLife *-- LifeGame : owns
    FormLife *-- LifeGrid : owns
    FormLife *-- Timer : owns
    FormLife ..> LifeGrid : UpdateGrid()
    FormLife ..> LifeGame : Step() Toggle()
```

`FormLife` is the only class that knows about both other parties. `LifeGame` and
`LifeGrid` never reference each other, which is why the arrays have to be copied
across by the form.

---

## 2. Class diagram: biology

```mermaid
classDiagram
    class Biosphere {
        -string m_name
        -AnimalKingdom m_animalKingdom
        -BiomeMap m_biomes
        -List~Organism~ m_organisms
        -List~Virus~ m_viruses
        -Dictionary~string,Organism~ m_organismIndex
        -long m_totalBiomassKg
        +Register(Organism) Organism
        +Find(string) Organism
        +GroupByKingdom() Dictionary
        +GetOrganismsInBiome(BiomeType) List
        +CreateDefaultWorld() Biosphere$
    }

    class AnimalKingdom {
        -Taxon m_root
        -Dictionary~AnimalPhylum,Phylum~ m_phyla
        +GetPhylum(AnimalPhylum) Phylum
        +RegisterAnimal(Animal) Animal
    }

    class BiomeMap {
        -string m_worldName
        -List~Biome~ m_biomes
        -double m_axialTiltDegrees
        -double m_dayLengthHours
        -double m_gravityMs2
        +AddBiome(Biome) Biome
        +GetBiomesOfType(BiomeType) List
        +CountOrganisms() int
    }

    class Organism {
        <<abstract>>
        #string m_name
        #Domain m_domain
        #Kingdom m_kingdom
        #Cellularity m_cellularity
        #DietType m_diet
        #double m_sizeMetres
        #double m_massKilograms
        #Dna m_dna
        +AddGene(Gene) void
        +AddRna(Rna) void
        +Transcribe() Rna
    }

    class Taxon {
        -TaxonomyRank m_rank
        -string m_name
        -Taxon m_parent
        -List~Taxon~ m_children
        -List~Species~ m_species
        +AddChild(Taxon) Taxon
        +AddSpecies(Species) Species
        +GetAllSpecies() List
        +CountSpecies() int
        +GetLineage() string
    }

    Biosphere *-- AnimalKingdom
    Biosphere *-- BiomeMap
    Biosphere o-- Organism : registry
    Biosphere ..> Taxon : via AnimalKingdom
    AnimalKingdom *-- Taxon : root
    Organism ..> Dna : has genome
```

---

## 3. Class diagram: animal kingdom

```mermaid
classDiagram
    class Organism { <<abstract>> }
    class MulticellularOrganism { <<abstract>>
        #long m_cellCount
        #int m_tissueTypes
        #bool m_hasNervousSystem
        #bool m_hasCirculatorySystem
        #bool m_hasSkeleton
        #double m_growthRate
    }
    class UnicellularOrganism {
        #CellDivisionType m_division
        #bool m_hasNucleus
        #double m_diameterMicrons
        #bool m_motile
    }
    class Virus {
        #bool m_hasCapsid
        #bool m_hasEnvelope
        #string m_hostSpecies
        #double m_incubationDays
    }
    class Animal {
        #AnimalPhylum m_phylum
        #ChordateClass? m_chordateClass
        #ArthropodClass? m_arthropodClass
        #MolluscClass? m_molluscClass
        #bool m_isVertebrate
        #double m_speedKph
        +ToSpecies() Species
    }
    class Vertebrate {
        #VertebrateClass m_class
        #bool m_hasFur
        #bool m_hasFeathers
        #bool m_hasScales
        #bool m_isWarmBlooded
        #bool m_laysEggs
        #int m_gestationDays
    }
    class Invertebrate {
        #bool m_hasExoskeleton
        #bool m_hasSegmentedBody
        #int m_legCount
    }
    class Mammal { #string m_order }
    class Bird { #double m_wingspanMetres }
    class Reptile { #bool m_isVenomous }
    class Amphibian { #bool m_needsWater }
    class Fish { #bool m_isFreshwater }
    class Insect { #int m_wingCount }
    class Arachnid { #bool m_spinsSilk }
    class Crustacean { #int m_clawCount }
    class Cephalopod { #int m_tentacleCount }
    class Gastropod { #bool m_hasShell }
    class Bivalve { #bool m_canProducePearl }
    class Cnidarian { #bool m_hasStingingCells }
    class Sponge { #bool m_hasSpicules }
    class Annelid { #bool m_isSegmented }

    class Species {
        -string m_name
        -string m_scientificName
        -string m_genus
        -string m_family
        -string m_conservationStatus
        -int m_populationEstimate
        -bool m_isExtinct
        -List~string~ m_traits
        +AddTrait(string) void
    }

    Organism <|-- UnicellularOrganism
    Organism <|-- MulticellularOrganism
    Organism <|-- Virus
    MulticellularOrganism <|-- Animal
    Animal <|-- Vertebrate
    Animal <|-- Invertebrate
    Vertebrate <|-- Mammal
    Vertebrate <|-- Bird
    Vertebrate <|-- Reptile
    Vertebrate <|-- Amphibian
    Vertebrate <|-- Fish
    Invertebrate <|-- Insect
    Invertebrate <|-- Arachnid
    Invertebrate <|-- Crustacean
    Invertebrate <|-- Cephalopod
    Invertebrate <|-- Gastropod
    Invertebrate <|-- Bivalve
    Invertebrate <|-- Cnidarian
    Invertebrate <|-- Sponge
    Invertebrate <|-- Annelid
    Animal ..> Species : projects to
```

`AnimalKingdom` files each concrete animal under its phylum by calling
`ToSpecies()`, so one registration populates both the object graph and the
taxonomy tree.

---

## 4. Class diagram: molecular biology

```mermaid
classDiagram
    class Nucleotide {
        <<enumeration>>
        Adenine
        Cytosine
        Guanine
        Thymine
        Uracil
    }
    class GeneOrientation {
        <<enumeration>>
        Forward
        Reverse
    }
    class NucleicAcidType {
        <<enumeration>>
        Dna
        Rna
        MessengerRna
        TransferRna
        RibosomalRna
    }
    class StrandType {
        <<enumeration>>
        SingleStranded
        DoubleStranded
    }

    class Gene {
        -string m_name
        -List~Nucleotide~ m_bases
        -GeneOrientation m_orientation
        -string m_trait
        -bool m_isDominant
        +SetSequence(string) void
        +GetSequence() string
        +int Length
    }

    class NucleicAcid {
        -NucleicAcidType m_type
        -StrandType m_strandType
        -List~Nucleotide~ m_bases
        -List~Gene~ m_genes
        -double m_mutationRate
        +AppendSequence(string) void
        +AppendGene(Gene) void
        +GetSequence() string
        +GetComplement() NucleicAcid
    }

    class Dna {
        +Dna()
        +Dna(string sequence)
        +FromChromosomes(IList~Dna~) Dna$
    }

    class Rna {
        +Rna(NucleicAcidType type)
        +Transcribe(Dna) Rna$
    }

    class NucleotideExtensions {
        <<static>>
        +ToLetter(Nucleotide) char$
        +FromLetter(char) Nucleotide$
        +DnaComplement(Nucleotide) Nucleotide$
        +RnaComplement(Nucleotide) Nucleotide$
    }

    NucleicAcid <|-- Dna
    NucleicAcid <|-- Rna
    NucleicAcid o-- Gene
    Gene o-- Nucleotide
    Nucleotide ..> NucleotideExtensions : helper
```

Complementarity: DNA pairs A-T and C-G; RNA pairs A-U and C-G. `Rna.Transcribe`
copies a `Dna` strand substituting thymine with uracil, which is the operation
`Organism.Transcribe()` exposes.

---

## 5. Class diagram: ecology

```mermaid
classDiagram
    class BiomeType {
        <<enumeration>>
        TropicalRainforest
        HotDesert
        Tundra
        Ocean
        Freshwater
    }
    class ClimateZone {
        <<enumeration>>
        Polar
        Temperate
        Tropical
        Arid
        Aquatic
    }
    class ZoneType {
        <<enumeration>>
        Epipelagic
        Mesopelagic
        Littoral
        Canopy
        Understory
        Soil
    }
    class HabitatTerrain {
        <<enumeration>>
        Ocean
        Forest
        Jungle
        Desert
        Tundra
        Ice
    }

    class Zone {
        -string m_name
        -ZoneType m_type
        -Zone m_parent
        -List~Zone~ m_subZones
        -double m_minAltitudeMetres
        -double m_maxAltitudeMetres
        -double m_minTemperatureCelsius
        -double m_maxTemperatureCelsius
        -double m_salinityPpt
        +AddSubZone(Zone) Zone
        +IsTemperatureSuitable(double) bool
    }

    class Biome {
        -string m_name
        -BiomeType m_type
        -ClimateZone m_climateZone
        -double m_averageTemperatureCelsius
        -double m_averageRainfallMm
        -bool m_isAquatic
        -bool m_isMarine
        -List~Zone~ m_zones
        -List~HabitatTerrain~ m_terrains
        -List~Organism~ m_organisms
        -List~Species~ m_species
        +AddZone(string, ZoneType) Zone
        +AddTerrain(HabitatTerrain) void
        +AddOrganism(Organism) void
        +AddSpecies(Species) void
        +CountOrganisms() int
    }

    BiomeMap *-- Biome
    Biome *-- Zone
    Zone o-- Zone : sub-zones
    Biome ..> Organism
    Biome ..> ZoneType
    Biome ..> ClimateZone
```

Zones nest, which is how an ocean gets an epipelagic band that itself contains
sub-zones, and how a forest gets a canopy above an understory above a floor.

---

## 6. Sequence: one generation

```mermaid
sequenceDiagram
    participant T as Timer
    participant F as FormLife
    participant G as LifeGame
    participant L as LifeGrid

    T->>F: Tick
    F->>G: Step()
    G->>G: AdvancePopulation()
    note over G: read m_currentStates,<br/>write m_newStates,<br/>set m_historyStates
    G->>G: m_newStates → m_currentStates
    F->>F: m_stepNumber += 1
    F->>F: UpdateFormVisuals()
    F->>F: update generation / population labels
    F->>L: UpdateGrid(GameGrid, HistoryGrid)
    L->>L: copy arrays, Invalidate()
    L-->>L: LifeGridPaint draws every cell
```

The important property is that `Step()` completes before any painting is
requested. The grid control is never asked to render a half-computed board.

---

## 7. Sequence: clicking a cell

```mermaid
sequenceDiagram
    participant U as User
    participant L as LifeGrid
    participant F as FormLife
    participant G as LifeGame

    U->>L: MouseDown at (px, py)
    L->>F: MouseDown event
    F->>F: x = px * Columns / Width
    F->>F: y = py * Rows / Height
    F->>G: ToggleCellState(x, y)
    alt inside bounds
        G->>G: flip m_currentStates[x + y*columns]
        G->>G: LiveCellCount ± 1
    else outside bounds
        G-->>F: ignored
    end
    F->>L: UpdateGrid(GameGrid, HistoryGrid)
    L->>L: Invalidate, repaint
```

`ToggleCellState` bounds-checks because the pixel-to-cell division can round to
`Columns` or `Rows` on the last pixel of the control.

---

## 8. Sequence: building the default world

```mermaid
sequenceDiagram
    participant C as Caller
    participant B as Biosphere
    participant M as BiomeMap
    participant K as AnimalKingdom
    participant O as Organism

    C->>B: CreateDefaultWorld()
    B->>B: new Biosphere("Homeworld")
    B->>M: AddBiome(...) ×5
    note over M: rainforest, desert, tundra,<br/>ocean, freshwater
    M->>M: AddZone(...) for each
    M->>M: AddTerrain(...) for each
    B->>B: BuildSampleOrganisms()
    loop 18 organisms
        B->>O: new Mammal / Tree / Bacterium ...
        B->>B: set kingdom, diet, size, mass, genes
        B->>B: Register(organism)
        B->>K: RegisterAnimal(animal) if Animal
        K->>K: GetPhylum(phylum)
        K->>K: phylum.AddSpecies(animal.ToSpecies())
    end
    B-->>C: fully populated Biosphere
```

---

## 9. State: simulation lifecycle

```mermaid
stateDiagram-v2
    [*] --> Ready : FormLife constructed,<br/>grid sized, model created

    Ready --> Running : Start, SetStart() + timer.Start()
    Running --> Paused : Stop, timer.Stop()
    Paused --> Running : Start
    Running --> Paused : Space / Escape
    Paused --> Ready : Clear or Resize

    Running --> Running : Tick → Step()
    Paused --> Paused : Step (one generation)
    Ready --> Ready : Step

    Paused --> Paused : Reset (restore start snapshot)
    Ready --> Ready : Randomize
    Paused --> Paused : Randomize

    note right of Running
        Start disabled, Stop enabled
    end note
    note right of Ready
        Start enabled, Stop disabled
    end note
```

`Ready` is the state after construction, after Clear and after a resize. `Paused`
is reached only by stopping a run, which is why Reset and Randomize are
available from `Paused` but not listed from `Ready`.

---

## 10. State: a cell

```mermaid
stateDiagram-v2
    [*] --> Dead
    Dead --> Alive : birth rule satisfied<br/>(neighbours in birthRules)
    Dead --> Alive : user toggle, Randomize
    Alive --> Alive : survive rule satisfied<br/>(neighbours in surviveRules)
    Alive --> Dead : death by underpopulation,<br/>overpopulation or no survive rule
    Alive --> Dead : user toggle, Clear
    Dead --> Dead : no birth rule satisfied

    note right of Alive
        historyStates[index] is set
        and never cleared
    end note
```

The history flag is a one-way latch. A cell that has ever been alive keeps
`historyStates[index] == true` until `ClearGrid()` runs, which is what produces
the fading trail behind a moving pattern.

---

## 11. Activity: start-up

```mermaid
flowchart TD
    A[Main] --> B[EnableVisualStyles]
    B --> C[SetCompatibleTextRenderingDefault false]
    C --> D[new FormLife]
    D --> E[InitializeComponent:<br/>build menus, sidebar, status bar, grid]
    E --> F[AddSizesToComboBox]
    F --> G[AddSpeedsToComboBox]
    G --> H[AddRulesToComboBox]
    H --> I[Select size index 3 and speed index 3]
    I --> J[Set zoom limits: max 5, rate 1.5]
    J --> K[Wire timer, buttons, comboboxes,<br/>grid mouse handlers, shortcuts]
    K --> L[Disable Stop button]
    L --> M[ResizeGrid → new LifeGame size 100]
    M --> N[UpdateFormVisuals → first paint]
    N --> O[Application.Run message loop]
```

---

## 12. Activity: sizing the grid

```mermaid
flowchart TD
    A[Size combobox changed] --> B{m_timer null?}
    B -- no --> C[Stop timer]
    B -- yes --> D[skip]
    C --> E{m_lifeGame null?}
    D --> E
    E -- no --> F[ClearGrid]
    E -- yes --> G[skip]
    F --> H[stepNumber = 0]
    G --> H
    H --> I{m_lifeGame null?}
    I -- no --> J[UpdateFormVisuals to repaint old size]
    I -- yes --> K[skip]
    J --> L[ResizeGrid:<br/>read combo, set Columns and Rows,<br/>new LifeGame]
    K --> L
    L --> M[Start enabled, Stop disabled]
    M --> N[Grid repainted at new size]
```

Both null checks matter: the combobox fires `SelectedIndexChanged` during
construction, before the timer and the model exist.

---

## 13. Use cases

```mermaid
flowchart LR
    User((User))
    Keyboard((Keyboard))
    Mouse((Mouse))

    subgraph LifeEngine
        UC1[Run the simulation]
        UC2[Advance one generation]
        UC3[Seed the grid]
        UC4[Clear or reset the grid]
        UC5[Choose a ruleset]
        UC6[Change grid size]
        UC7[Zoom the view]
        UC8[Toggle grid lines]
        UC9[Set cell colours]
        UC10[Draw cells by hand]
        UC11[Pan the view]
        UC12[Read statistics]
        UC13[Show or hide the sidebar]
        UC14[Read about the program]
    end

    Mouse --> UC1
    Mouse --> UC2
    Mouse --> UC3
    Mouse --> UC4
    Mouse --> UC5
    Mouse --> UC6
    Mouse --> UC7
    Mouse --> UC8
    Mouse --> UC9
    Mouse --> UC10
    Mouse --> UC11
    Mouse --> UC13
    Mouse --> UC14
    Mouse --> UC1
    Keyboard --> UC1
    Keyboard --> UC2
    Keyboard --> UC3
    Keyboard --> UC4
    Keyboard --> UC7
    Keyboard --> UC8
    Keyboard --> UC11
    Keyboard --> UC13
    User --> UC12
```

Every case reachable from the keyboard is also reachable from the mouse, with
the single exception of free scrolling by arrow key.

---

## Source of the model

The biological half of the design takes its cue from
[SproutLife](https://github.com/ShprAlex/SproutLife) by Alex Shapiro (MIT): cells
owned by an organism, organisms carrying a genome, mutations accumulating with
age. LifeEngine's `Organism`, `Genome`-equivalents (`Dna`, `Gene`) and
`Biosphere` registry are original C# taken in that direction — no SproutLife
code is copied, and SproutLife itself is Java and has no Windows Forms UI.
