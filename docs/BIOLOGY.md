# The biological model

Everything under `LifeEngine.Biology` exists to answer one question: what would
it take for the cells in this program to be *alive* rather than merely on or off?
This document describes what is actually implemented today.

---

## Contents

- [What is here](#what-is-here)
- [Taxonomy](#taxonomy)
- [Kingdoms and organism classes](#kingdoms-and-organism-classes)
- [Molecular biology](#molecular-biology)
- [Ecology: biomes and zones](#ecology-biomes-and-zones)
- [The default world](#the-default-world)
- [Worked examples](#worked-examples)
- [What is not here yet](#what-is-not-here-yet)

---

## What is here

| Namespace / file | Gives you |
| --- | --- |
| `Enums.cs` | Every enumerations the model uses: ranks, kingdoms, phyla, classes, diets, reproduction, thermal strategy, cellularity, locomotion, symmetry, genes. |
| `NucleicAcid.cs` | Nucleotide letters and complementarity, genes with sequences, DNA and RNA strands, transcription. |
| `Taxonomy.cs` | The rank ladder as a tree of `Taxon` nodes, plus `Phylum`. |
| `Organism.cs` | The `Organism` base class, `UnicellularOrganism`, `MulticellularOrganism`, `Virus`, and the molecular machinery every organism carries. |
| `AnimalKingdom.cs` | `Species`, the `AnimalKingdom` registry, `Animal`, `Vertebrate`, `Invertebrate`. |
| `Animals.cs` | Fifteen concrete animals. |
| `Kingdoms.cs` | `Eukaryote`, `Plant` and its subtypes, `Fungus`, `Protist`, `Alga`, `Bacterium`, `Archaeon`. |
| `Biomes.cs` | Biome types, climate zones, zone types, terrains, trophic levels, `Zone`, `Biome`, `BiomeMap`. |
| `Biosphere.cs` | The world container and `CreateDefaultWorld()`. |

Nothing here references `LifeGame`, `LifeGrid` or `FormLife`. The biological
model is a self-contained library.

---

## Taxonomy

The ladder is eight ranks, from `TaxonomyRank`:

```
Domain → Kingdom → Phylum → Class → Order → Family → Genus → Species
```

`Taxon` is one rung. Each node holds its rank, a name, a description, its
parent, its child taxa and the `Species` records that sit directly at it.

```csharp
Taxon root = new Taxon(TaxonomyRank.Kingdom, "Animalia");
Phylum chordata = root.AddChild(new Phylum(AnimalPhylum.Chordata));
chordata.AddSpecies(species);

root.CountSpecies();   // total at any depth
root.GetLineage();     // "Animalia > Chordata > ..."
```

`GetLineage` walks up to the root and joins the names with `>`, so any node can
print its own full classification.

`Phylum` is a `Taxon` that additionally carries an `AnimalPhylum` identifier, so
a node can be found and compared by enum rather than by string.

### Domains and kingdoms

```
Domain:   Bacteria | Archaea | Eukaryota | Viruses
Kingdom:  Bacteria | Archaea | Protista | Fungi | Plantae | Animalia | Acellularia
```

The prokaryotic domains appear in the `Kingdom` enum as well, so a single enum
can hold every top-level group regardless of which classification scheme is
being used at the moment.

### Phyla and classes

`AnimalPhylum` covers the eighteen animal phyla, with the major ones first:
Porifera, Cnidaria, Ctenophora, Platyhelminthes, Nematoda, Annelida, Mollusca,
Arthropoda, Echinodermata, Hemichordata, Chordata, and the notable minor ones
(Rotifera, Tardigrada, Onychophora, Nemertea, Brachiopoda, Bryozoa,
Xenacoelomorpha).

Below the phyla sit three class enums, each covering its own phylum:

- `ChordateClass` — Mammalia, Aves, Reptilia, Amphibia, Actinopterygii,
  Sarcopterygii, Chondrichthyes, Myxini, Petromyzontida, Ascidiacea, Thaliacea,
  Appendicularia.
- `ArthropodClass` — Insecta, Arachnida, Malacostraca, Branchiopoda, Chilopoda,
  Diplopoda, Pauropoda, Symphyla, Merostomata, Pycnogonida, Remipedia,
  Cephalocarida.
- `MolluscClass` — Gastropoda, Bivalvia, Cephalopoda, Polyplacophora,
  Scaphopoda, Monoplacophora, Aplacophora.

`VertebrateClass` is a coarser convenience grouping used by the concrete
animals: Mammal, Bird, Reptile, Amphibian, BonyFish, LobeFinnedFish,
CartilaginousFish, JawlessFish.

---

## Kingdoms and organism classes

### The base classes

Every organism, living or acellular, derives from `Organism`. It carries:

- **Identity:** `Name`, `ScientificName`.
- **Classification:** `Domain`, `Kingdom`, `Cellularity` (protected setter — only
  a derived class may fix it).
- **Way of life:** `Diet`, `Reproduction`, `Thermal`, `Locomotion`, `Symmetry`.
- **Body:** `SizeMetres`, `MassKilograms`, `ChromosomePairs`, `LifespanYears`,
  `Energy`.
- **Molecules:** `Dna`, `Genes`, `Rna`.

Three derived shape of life sit below it:

| Class | Cellularity | For |
| --- | --- | --- |
| `UnicellularOrganism` | Unicellular | Bacteria, archaea, protozoa, most algae. Adds `Division`, `HasNucleus`, `DiameterMicrons`, `Motile`. |
| `MulticellularOrganism` | Multicellular | Animals and plants. Adds `CellCount`, `TissueTypes`, `HasNervousSystem`, `HasCirculatorySystem`, `HasSkeleton`, `GrowthRate`. |
| `Virus` | Acellular | Adds `HasCapsid` (default true), `HasEnvelope`, `HostSpecies`, `IncubationDays`. Sets its own domain to `Viruses`, kingdom `Acellularia`, reproduction `Viral`. |

### Plants

`Eukaryote` is the shared base for the non-animal eukaryotic kingdoms. It adds
`Clade`, `HasCellWall` and `CellWallMaterial`. Plants get cellulose walls,
fungi get chitin.

`Plant` adds `Division`, `HasVascularTissue`, `HasSeeds`, `HasFlowers`,
`HasRoots`, `MaxHeightMetres`, `PhotosyntheticPigment`, `IsDeciduous`, and
defaults to diet `Photosynthetic`, locomotion `Sessile`, symmetry `Radial`.

Four concrete plants:

| Class | Distinguishing members |
| --- | --- |
| `Tree` | `TrunkDiameterMetres`, `CanopyLayers` (default 3), `IsEvergreen`. Vascular, seeded, rooted. |
| `FloweringPlant` | `FlowerColor`, `PollinationVector` (default "Insect"), `IsFragrant`. |
| `Moss` | `NeedsMoisture`, `CushionHeightMm`. Non-vascular, no seeds, 5 cm. |
| `Fern` | `HasFronds`, `FrondCount`. Vascular, reproduces by `Sporulation`. |

### Fungi, protists and prokaryotes

- `Fungus` — `Phylum`, `IsSingleCelled`, `FormsMycelium`, `IsParasitic`,
  `IsSymbiotic`, `ProducesMushroom`, `CapDiameterCm`. Diet `Saprotroph`.
- `Protist` — `Group`, `IsPhotosynthetic`, `FormsColonies`, `HasFlagella`,
  `HasCilia`, `HasPseudopods`. Derives from `Organism` directly, not from
  `MulticellularOrganism`, because most protists are single cells.
- `Alga` — a `Protist` that is photosynthetic, with `Pigment` and
  `HasSilicaShell`.
- `Bacterium` — `Shape`, `IsGramPositive`, `FormsEndospores`, `IsPathogenic`,
  `HasFlagella`. `HasNucleus` is false.
- `Archaeon` — `IsExtremophile`, `Environment`. `HasNucleus` is false.

### Animals

`Animal` adds `Phylum`, the three optional class identifiers, `IsVertebrate`,
`IsDomesticated`, `OffspringPerCycle`, `SpeedKph`, and `ToSpecies()`, which
projects the animal into a taxonomy `Species` record with diet, phylum,
symmetry and class traits attached.

`Vertebrate` sets `IsVertebrate` and configures itself from its
`VertebrateClass`:

| Class | Fur | Feathers | Scales | Thermal | Eggs | Gestation |
| --- | --- | --- | --- | --- | --- | --- |
| Mammal | yes | | | Endotherm | | 180 days |
| Bird | | yes | | Endotherm | yes | |
| Reptile | | | yes | Ectotherm | yes | |
| Amphibian | | | | Ectotherm | yes | |
| Fish (default branch) | | | yes | Ectotherm | yes | |

`Invertebrate` sets `IsVertebrate` false, thermal `Ectotherm`, and adds
`HasExoskeleton`, `HasSegmentedBody`, `LegCount`.

Fifteen concrete animals ship with the program:

| Class | Phylum | Notable defaults |
| --- | --- | --- |
| `Mammal` | Chordata | Order "Unassigned", walking, bilateral. |
| `Bird` | Chordata | Can fly, flying, bilateral. |
| `Reptile` | Chordata | Crawling, bilateral. |
| `Amphibian` | Chordata | Needs water, has a tadpole stage, swimming. |
| `Fish` | Chordata | Chordate class chosen from the vertebrate class; cartilaginous and jawless handled explicitly. |
| `Insect` | Arthropoda | Six legs, exoskeleton, segmented, class Insecta. |
| `Arachnid` | Arthropoda | Eight legs, eight eyes, class Arachnida. |
| `Crustacean` | Arthropoda | Marine, carapace, claws, class Malacostraca. |
| `Cephalopod` | Mollusca | Eight tentacles, ink sac, colour change, swimming. |
| `Gastropod` | Mollusca | Shell, crawling. |
| `Bivalve` | Mollusca | Filter feeder, can produce pearls. |
| `Cnidarian` | Cnidaria | Stinging cells, radial symmetry, sessile. |
| `Sponge` | Porifera | Filter feeder, asymmetric, no true tissues, spicules. |
| `Annelid` | Annelida | Segmented, bristles, crawling. |

---

## Molecular biology

### Nucleotides

Five bases in `Nucleotide`: Adenine, Cytosine, Guanine, Thymine, Uracil, with
the conventional letters `A C G T U`. `NucleotideExtensions` provides
`ToLetter`, `FromLetter` and the two complement rules:

- `DnaComplement` — A↔T, C↔G.
- `RnaComplement` — A↔U, C↔G.

`FromLetter` throws on an unknown letter rather than returning a placeholder, so
a typo in a sequence is caught at the point it is written.

### Genes

A `Gene` has a name, an ordered list of nucleotides, an orientation
(`Forward`/`Reverse`), a trait string describing the phenotype it contributes
to, and a dominance flag. `SetSequence` parses a string such as `"ATGCGTACGTAG"`
into bases, skipping whitespace, so sequences can be written in readable blocks.

### Strands

`NucleicAcid` is a strand: a type (`Dna`, `Rna`, `MessengerRna`, `TransferRna`,
`RibosomalRna`), a strand type, a base list, a gene list and a mutation rate
(default 1e-9 per base per replication).

- `Dna` is always double-stranded. `Dna.FromChromosomes(list)` concatenates
  several chromosome strands into one genome, carrying their genes along.
- `Rna` is single-stranded. `Rna.Transcribe(dna)` copies the bases, substituting
  thymine for uracil, and copies the genes across.

`Organism.Transcribe()` calls that and files the result in the organism's RNA
list, so an organism can hold every transcript it has produced.

### How a genome is built

```csharp
Mammal wolf = new Mammal("Grey Wolf");
wolf.AddGene(new Gene("FurColor", "ATGCGTACGTAG"));
wolf.Transcribe();          // messenger RNA with U in place of T
wolf.Dna.Genes.Count;       // 1
wolf.Rna.Count;             // 1
```

---

## Ecology: biomes and zones

### Enumerations

- `BiomeType` — sixteen biomes from TropicalRainforest and HotDesert through
  TemperateDeciduousForest and BorealForest to Tundra, IceSheet, Mountain,
  Wetland, Freshwater and Ocean.
- `ClimateZone` — Polar, Subpolar, Temperate, Subtropical, Tropical, Arid,
  Alpine, Aquatic, DeepSea, Hydrothermal.
- `ZoneType` — the vertical banding of a biome: Epipelagic, Mesopelagic,
  Bathypelagic, Abyssopelagic, Hadalpelagic, Littoral, Limnetic, Profundal,
  Canopy, Understory, ForestFloor, Riparian, Intertidal, Neritic, Oceanic, Soil,
  Subterranean, Aeriel.
- `HabitatTerrain` — the surfaces an organism can live on: Ocean, Coast, River,
  Lake, Wetland, Grassland, Forest, Jungle, Desert, Mountain, Tundra, Ice, Cave,
  Urban, Farmland, Soil, Host.
- `TrophicLevel` — Producer, PrimaryConsumer, SecondaryConsumer,
  TertiaryConsumer, ApexPredator, Decomposer, Detritivore.

### Zones

A `Zone` is a band inside a biome, described by a name, a `ZoneType`, an
altitude or depth range in metres, a temperature range in Celsius and a salinity
in parts per thousand. Zones nest through `AddSubZone`, which sets the child's
`Parent`. `IsTemperatureSuitable(celsius)` tests a temperature against the band.

### Biomes

A `Biome` holds its type, climate zone, average temperature and yearly rainfall,
latitude, aquatic and marine flags, its zones, its terrains, the organisms
recorded in it and the species recorded in it. Organisms and species are added
without duplication, and `CountOrganisms()` reports how many are filed.

### The map

`BiomeMap` is the world: a name, the list of biomes, and three planetary
parameters that would drive a climate model — `AxialTiltDegrees` (default 23.44),
`DayLengthHours` (24) and `GravityMs2` (9.81). `GetBiomesOfType` filters by
biome type, and `CountOrganisms` totals across every biome.

---

## The default world

`Biosphere.CreateDefaultWorld()` builds a small Earth-like planet called
"Homeworld". It is the fastest way to see the whole model working.

**Biomes and their zones:**

| Biome | Type | Climate | Temp | Rainfall | Zones |
| --- | --- | --- | --- | --- | --- |
| Equatorial Rainforest | TropicalRainforest | Tropical | 27 °C | 2500 mm | Canopy (20–50 m, 20–35 °C), Understory, Forest Floor |
| Subtropical Desert | HotDesert | Arid | 30 °C | 150 mm | Dunes |
| Arctic Tundra | Tundra | Polar | −5 °C | 250 mm | — |
| Open Ocean | Ocean | Aquatic | 12 °C | 0 mm | Epipelagic (0–200 m, 35 ppt), Mesopelagic (200–1000 m), Bathypelagic (1000–4000 m, 0–6 °C) |
| Temperate Lake | Freshwater | Aquatic | — | — | Littoral, Limnetic, Profundal |

Terrains are attached per biome: Jungle and River for the rainforest, Desert for
the desert, Tundra and Ice for the tundra, Ocean and Coast for the sea, Lake and
Wetland for the lake. The ocean is flagged marine, the lake is not.

**Organisms registered, eighteen in all:**

| Organism | Class | Scientific name | Notes |
| --- | --- | --- | --- |
| Grey Wolf | `Mammal` | Canis lupus | Carnivore, 40 kg, 39 chromosome pairs, 13 years, 60 kph, 50 billion cells, nervous / circulatory / skeletal systems, one gene. |
| Golden Eagle | `Bird` | Aquila chrysaetos | Carnivore, 2.2 m wingspan, 30 years, migratory, hooked beak, 240 kph, one gene. |
| King Cobra | `Reptile` | Ophiophagus hannah | Carnivore, venomous, 4 m, 6 kg. |
| Common Frog | `Amphibian` | Rana temporaria | Carnivore, 10 cm, 20 g. |
| Atlantic Salmon | `Fish` | Salmo salar | Bony fish, not freshwater, 1 m, 4.5 kg. |
| Honey Bee | `Insect` | Apis mellifera | Herbivore, flies, four wings, social, metamorphosis. |
| Garden Spider | `Arachnid` | Araneus diadematus | Carnivore, venomous, spins silk. |
| Common Octopus | `Cephalopod` | Octopus vulgaris | Carnivore, 28 chromosome pairs. |
| English Oak | `Tree` | Quercus robur | 40 m, deciduous, 500 years, one gene. |
| Dog Rose | `FloweringPlant` | Rosa canina | Pink flowers, insect pollinated, fragrant. |
| Peat Moss | `Moss` | Sphagnum palustre | Needs moisture. |
| Chanterelle | `Fungus` | Cantharellus cibarius | Mushroom, symbiotic, 8 cm cap. |
| Boat Diatom | `Alga` | Navicula radiosa | Silica shell, 50 µm. |
| Gut Bacterium | `Bacterium` | Escherichia coli | Rod, Gram-negative, flagella, not pathogenic. |
| Thermal Vent Archaeon | `Archaeon` | Pyrococcus furiosus | Extremophile, hydrothermal vent. |
| Influenza A | `Virus` | Alphainfluenzavirus influenzae | Enveloped, birds and mammals, 2-day incubation, one gene. |

Registering each one does three things: it lands in the flat organism list, it
lands in the name index, and if it is an animal it is filed in the kingdom under
its phylum as a `Species` projection.

---

## Worked examples

### Inspect the world

```csharp
Biosphere world = Biosphere.CreateDefaultWorld();

Console.WriteLine(world);                    // Homeworld (17 organisms, 5 biomes)
Console.WriteLine(world.AnimalKingdom);      // Animalia (7 phyla, 8 species)

Organism wolf = world.Find("Grey Wolf");
Console.WriteLine(wolf.ScientificName);      // Canis lupus
Console.WriteLine(wolf.Kingdom);             // Animalia
Console.WriteLine(wolf.Genes[0]);            // FurColor (12 bp)
```

### Group by kingdom

```csharp
Dictionary<Kingdom, List<Organism>> groups = world.GroupByKingdom();

foreach (KeyValuePair<Kingdom, List<Organism>> group in groups)
    Console.WriteLine(group.Key + ": " + group.Value.Count);
```

### Find everything that lives in an ocean

```csharp
List<Organism> marine = world.GetOrganismsInBiome(BiomeType.Ocean);
```

This reads the organisms attached to the ocean biome. Organisms registered in
the biosphere are not automatically attached to a biome, so the list is empty in
the default world until something files them there — see the next example.

### Add an organism to a biome

```csharp
Mammal orca = new Mammal("Orca");
orca.ScientificName = "Orcinus orca";
orca.Order = "Artiodactyla";
orca.Diet = DietType.Carnivore;
orca.SizeMetres = 8.0;
orca.MassKilograms = 5400.0;
orca.SpeedKph = 56.0;
orca.IsMarine = true;

world.Register(orca);
world.Biomes.GetBiomesOfType(BiomeType.Ocean)[0].AddOrganism(orca);
```

### Build a genome and read it back

```csharp
Dna chromosome = new Dna("ATGCGTACGTTAGCCATA");
chromosome.AppendGene(new Gene("EyeColor", "ATGCGTACGTTAGCCATA"));

Rna transcript = Rna.Transcribe(chromosome);

Console.WriteLine(chromosome.GetSequence());     // ATGCGTACGTTAGCCATA
Console.WriteLine(transcript.GetSequence());     // AUGCGUACGUUAGCCAUA
Console.WriteLine(chromosome.GetComplement().GetSequence()); // TACGCATGCAATCGGTAT
```

Thymine (`T`) becomes uracil (`U`) in the transcript; the complement inverts
every base.

---

## What is not here yet

- **No link to the grid.** Organisms do not occupy cells, do not consume
  anything, do not compete and do not die. The biological model describes life;
  it does not yet simulate it.
- **No mutation.** `NucleicAcid.MutationRate` exists and is settable, but
  nothing applies it.
- **No inheritance.** `AddGene` files a gene, but no code copies a parent's
  genome into an offspring and mutates it.
- **No food web.** `TrophicLevel` is defined but never assigned or read.
- **Thermal and reproductive strategy are inert.** They are set correctly per
  species and then never consulted.
- **No persistence.** A biosphere cannot be saved to or loaded from disk.

The gap between this and a working ecosystem is the roadmap's first item: put
`Organism`s on grid cells, give them a cell list, let them grow, compete and
inherit a genome.
