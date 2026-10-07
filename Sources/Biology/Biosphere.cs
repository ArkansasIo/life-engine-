/**
 * LifeEngine is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * LifeEngine is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 *
 * Copyright © 2026 ArkansasIo
 */

using System;
using System.Collections.Generic;

namespace LifeEngine.Biology
{
    /// <summary>
    /// The catalogue of everything biological in the engine. It owns the animal
    /// kingdom, the biome map and a flat registry of every organism and every
    /// biome zone, and it can build a default Earth-like world out of the box.
    /// </summary>
    public sealed class Biosphere
    {
        private string m_name;
        private AnimalKingdom m_animalKingdom;
        private BiomeMap m_biomes;
        private List<Organism> m_organisms;
        private List<Virus> m_viruses;
        private Dictionary<string, Organism> m_organismIndex;
        private long m_totalBiomassKg;

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public AnimalKingdom AnimalKingdom
        {
            get { return m_animalKingdom; }
        }

        public BiomeMap Biomes
        {
            get { return m_biomes; }
        }

        public IList<Organism> Organisms
        {
            get { return m_organisms; }
        }

        public IList<Virus> Viruses
        {
            get { return m_viruses; }
        }

        public long TotalBiomassKg
        {
            get { return m_totalBiomassKg; }
            set { m_totalBiomassKg = value; }
        }

        public Biosphere(string name)
        {
            m_name = name;
            m_animalKingdom = new AnimalKingdom();
            m_biomes = new BiomeMap(name);
            m_organisms = new List<Organism>();
            m_viruses = new List<Virus>();
            m_organismIndex = new Dictionary<string, Organism>();
        }

        /// <summary>Registers an organism and files it in the kingdom/index.</summary>
        public Organism Register(Organism organism)
        {
            if (organism == null)
                return null;

            if (!m_organisms.Contains(organism))
                m_organisms.Add(organism);

            if (!string.IsNullOrEmpty(organism.Name))
                m_organismIndex[organism.Name] = organism;

            Animal animal = organism as Animal;

            if (animal != null)
                m_animalKingdom.RegisterAnimal(animal);

            Virus virus = organism as Virus;

            if (virus != null && !m_viruses.Contains(virus))
                m_viruses.Add(virus);

            m_totalBiomassKg += (long)(organism.MassKilograms * 1000.0);

            return organism;
        }

        public Organism Find(string name)
        {
            Organism found;

            if (name != null && m_organismIndex.TryGetValue(name, out found))
                return found;

            return null;
        }

        /// <summary>Groups the registered organisms by kingdom.</summary>
        public Dictionary<Kingdom, List<Organism>> GroupByKingdom()
        {
            Dictionary<Kingdom, List<Organism>> groups =
                new Dictionary<Kingdom, List<Organism>>();

            for (int i = 0; i < m_organisms.Count; i++)
            {
                Organism organism = m_organisms[i];

                if (!groups.ContainsKey(organism.Kingdom))
                    groups[organism.Kingdom] = new List<Organism>();

                groups[organism.Kingdom].Add(organism);
            }

            return groups;
        }

        /// <summary>Returns every registered organism that lives in a biome type.</summary>
        public List<Organism> GetOrganismsInBiome(BiomeType type)
        {
            List<Biome> matching = m_biomes.GetBiomesOfType(type);
            List<Organism> result = new List<Organism>();

            for (int i = 0; i < matching.Count; i++)
                result.AddRange(matching[i].Organisms);

            return result;
        }

        /// <summary>
        /// Builds a small Earth-like starter world: a handful of biomes, their
        /// zones, and one representative organism per major kingdom.
        /// </summary>
        public static Biosphere CreateDefaultWorld()
        {
            Biosphere biosphere = new Biosphere("Homeworld");

            biosphere.BuildBiomes();
            biosphere.BuildSampleOrganisms();

            return biosphere;
        }

        private void BuildBiomes()
        {
            Biome rainforest = m_biomes.AddBiome(
                "Equatorial Rainforest", BiomeType.TropicalRainforest, ClimateZone.Tropical);
            rainforest.AverageTemperatureCelsius = 27.0;
            rainforest.AverageRainfallMm = 2500.0;
            rainforest.AddTerrain(HabitatTerrain.Jungle);
            rainforest.AddTerrain(HabitatTerrain.River);
            Zone canopy = rainforest.AddZone("Canopy", ZoneType.Canopy);
            canopy.MinAltitudeMetres = 20.0;
            canopy.MaxAltitudeMetres = 50.0;
            canopy.MinTemperatureCelsius = 20.0;
            canopy.MaxTemperatureCelsius = 35.0;
            rainforest.AddZone("Understory", ZoneType.Understory);
            rainforest.AddZone("Forest Floor", ZoneType.ForestFloor);

            Biome desert = m_biomes.AddBiome(
                "Subtropical Desert", BiomeType.HotDesert, ClimateZone.Arid);
            desert.AverageTemperatureCelsius = 30.0;
            desert.AverageRainfallMm = 150.0;
            desert.AddTerrain(HabitatTerrain.Desert);
            desert.AddZone("Dunes", ZoneType.Soil);

            Biome tundra = m_biomes.AddBiome(
                "Arctic Tundra", BiomeType.Tundra, ClimateZone.Polar);
            tundra.AverageTemperatureCelsius = -5.0;
            tundra.AverageRainfallMm = 250.0;
            tundra.AddTerrain(HabitatTerrain.Tundra);
            tundra.AddTerrain(HabitatTerrain.Ice);

            Biome ocean = m_biomes.AddBiome(
                "Open Ocean", BiomeType.Ocean, ClimateZone.Aquatic);
            ocean.IsAquatic = true;
            ocean.IsMarine = true;
            ocean.AverageTemperatureCelsius = 12.0;
            ocean.AverageRainfallMm = 0.0;
            ocean.AddTerrain(HabitatTerrain.Ocean);
            ocean.AddTerrain(HabitatTerrain.Coast);

            Zone epipelagic = ocean.AddZone("Epipelagic", ZoneType.Epipelagic);
            epipelagic.MinAltitudeMetres = 0.0;
            epipelagic.MaxAltitudeMetres = 200.0;
            epipelagic.SalinityPpt = 35.0;

            Zone mesopelagic = ocean.AddZone("Mesopelagic", ZoneType.Mesopelagic);
            mesopelagic.MinAltitudeMetres = 200.0;
            mesopelagic.MaxAltitudeMetres = 1000.0;
            mesopelagic.SalinityPpt = 35.0;
            mesopelagic.MinTemperatureCelsius = 2.0;
            mesopelagic.MaxTemperatureCelsius = 20.0;

            Zone bathypelagic = ocean.AddZone("Bathypelagic", ZoneType.Bathypelagic);
            bathypelagic.MinAltitudeMetres = 1000.0;
            bathypelagic.MaxAltitudeMetres = 4000.0;
            bathypelagic.MinTemperatureCelsius = 0.0;
            bathypelagic.MaxTemperatureCelsius = 6.0;

            Biome freshwater = m_biomes.AddBiome(
                "Temperate Lake", BiomeType.Freshwater, ClimateZone.Aquatic);
            freshwater.IsAquatic = true;
            freshwater.IsMarine = false;
            freshwater.AddTerrain(HabitatTerrain.Lake);
            freshwater.AddTerrain(HabitatTerrain.Wetland);
            freshwater.AddZone("Littoral", ZoneType.Littoral);
            freshwater.AddZone("Limnetic", ZoneType.Limnetic);
            freshwater.AddZone("Profundal", ZoneType.Profundal);
        }

        private void BuildSampleOrganisms()
        {
            Mammal wolf = new Mammal("Grey Wolf");
            wolf.ScientificName = "Canis lupus";
            wolf.Order = "Carnivora";
            wolf.Diet = DietType.Carnivore;
            wolf.SizeMetres = 1.2;
            wolf.MassKilograms = 40.0;
            wolf.ChromosomePairs = 39;
            wolf.LifespanYears = 13;
            wolf.SpeedKph = 60.0;
            wolf.CellCount = 50000000000L;
            wolf.HasNervousSystem = true;
            wolf.HasCirculatorySystem = true;
            wolf.HasSkeleton = true;
            wolf.AddGene(new Gene("FurColor", "ATGCGTACGTAG"));
            Register(wolf);

            Bird eagle = new Bird("Golden Eagle");
            eagle.ScientificName = "Aquila chrysaetos";
            eagle.Diet = DietType.Carnivore;
            eagle.WingspanMetres = 2.2;
            eagle.MassKilograms = 5.0;
            eagle.LifespanYears = 30;
            eagle.IsMigratory = true;
            eagle.BeakType = "Hooked";
            eagle.SpeedKph = 240.0;
            eagle.AddGene(new Gene("WingSpan", "GGCTAGCTAA"));
            Register(eagle);

            Reptile cobra = new Reptile("King Cobra");
            cobra.ScientificName = "Ophiophagus hannah";
            cobra.Diet = DietType.Carnivore;
            cobra.IsVenomous = true;
            cobra.SizeMetres = 4.0;
            cobra.MassKilograms = 6.0;
            Register(cobra);

            Amphibian frog = new Amphibian("Common Frog");
            frog.ScientificName = "Rana temporaria";
            frog.Diet = DietType.Carnivore;
            frog.SizeMetres = 0.1;
            frog.MassKilograms = 0.02;
            Register(frog);

            Fish salmon = new Fish("Atlantic Salmon", VertebrateClass.BonyFish);
            salmon.ScientificName = "Salmo salar";
            salmon.IsFreshwater = false;
            salmon.Diet = DietType.Carnivore;
            salmon.SizeMetres = 1.0;
            salmon.MassKilograms = 4.5;
            Register(salmon);

            Insect bee = new Insect("Honey Bee");
            bee.ScientificName = "Apis mellifera";
            bee.Diet = DietType.Herbivore;
            bee.CanFly = true;
            bee.WingCount = 4;
            bee.IsSocial = true;
            bee.Metamorphosis = true;
            bee.SizeMetres = 0.015;
            bee.MassKilograms = 0.0001;
            Register(bee);

            Arachnid spider = new Arachnid("Garden Spider");
            spider.ScientificName = "Araneus diadematus";
            spider.Diet = DietType.Carnivore;
            spider.IsVenomous = true;
            spider.SpinsSilk = true;
            Register(spider);

            Cephalopod octopus = new Cephalopod("Common Octopus");
            octopus.ScientificName = "Octopus vulgaris";
            octopus.Diet = DietType.Carnivore;
            octopus.ChromosomePairs = 28;
            Register(octopus);

            Tree oak = new Tree("English Oak");
            oak.ScientificName = "Quercus robur";
            oak.MaxHeightMetres = 40.0;
            oak.IsDeciduous = true;
            oak.LifespanYears = 500;
            oak.AddGene(new Gene("LeafShape", "TTGACCGTA"));
            Register(oak);

            FloweringPlant rose = new FloweringPlant("Dog Rose");
            rose.ScientificName = "Rosa canina";
            rose.FlowerColor = "Pink";
            rose.PollinationVector = "Insect";
            rose.IsFragrant = true;
            Register(rose);

            Moss sphagnum = new Moss("Peat Moss");
            sphagnum.ScientificName = "Sphagnum palustre";
            sphagnum.NeedsMoisture = true;
            Register(sphagnum);

            Fungus chanterelle = new Fungus("Chanterelle");
            chanterelle.ScientificName = "Cantharellus cibarius";
            chanterelle.ProducesMushroom = true;
            chanterelle.IsSymbiotic = true;
            chanterelle.CapDiameterCm = 8.0;
            Register(chanterelle);

            Alga diatom = new Alga("Boat Diatom");
            diatom.ScientificName = "Navicula radiosa";
            diatom.HasSilicaShell = true;
            diatom.SizeMetres = 0.00005;
            Register(diatom);

            Bacterium eColi = new Bacterium("Gut Bacterium");
            eColi.ScientificName = "Escherichia coli";
            eColi.Shape = "Rod";
            eColi.IsGramPositive = false;
            eColi.HasFlagella = true;
            eColi.IsPathogenic = false;
            Register(eColi);

            Archaeon thermophile = new Archaeon("Thermal Vent Archaeon");
            thermophile.ScientificName = "Pyrococcus furiosus";
            thermophile.IsExtremophile = true;
            thermophile.Environment = "Hydrothermal vent";
            Register(thermophile);

            Virus flu = new Virus("Influenza A");
            flu.ScientificName = "Alphainfluenzavirus influenzae";
            flu.HasEnvelope = true;
            flu.HostSpecies = "Birds, mammals";
            flu.IncubationDays = 2.0;
            flu.AddGene(new Gene("Hemagglutinin", "AUGCUUAAUGG"));
            Register(flu);
        }

        public override string ToString()
        {
            return m_name + " (" + m_organisms.Count + " organisms, " +
                   m_biomes.Biomes.Count + " biomes)";
        }
    }
}
