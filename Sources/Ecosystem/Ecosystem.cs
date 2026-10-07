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

namespace LifeEngine.Ecosystem
{
    /// <summary>
    /// What a species is, in the ecosystem layer: the rules that decide where it
    /// can live and whether it thrives or dies there.
    ///
    /// A species does not know about the grid or the world. It answers one
    /// question, repeatedly: given this tile, what happens to me on it?
    /// </summary>
    public sealed class Species
    {
        private string m_name;
        private Biology.Kingdom m_kingdom;
        private Biology.DietType m_diet;
        private Biology.LocomotionType m_locomotion;
        private Biology.TrophicLevel m_trophicLevel;

        private double m_minTemperatureCelsius;
        private double m_maxTemperatureCelsius;
        private double m_minRainfallMm;
        private double m_maxRainfallMm;
        private double m_maxElevationMetres;

        private bool m_needsWaterTile;
        private bool m_needsLandTile;
        private bool m_needsCoast;
        private bool m_avoidsDesert;
        private bool m_avoidsIce;

        private double m_growthRate;
        private double m_deathRate;
        private int m_maxPopulation;
        private int m_seedPopulation;

        private char m_symbol;
        private string m_colourKey;

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public Biology.Kingdom Kingdom
        {
            get { return m_kingdom; }
            set { m_kingdom = value; }
        }

        public Biology.DietType Diet
        {
            get { return m_diet; }
            set { m_diet = value; }
        }

        public Biology.LocomotionType Locomotion
        {
            get { return m_locomotion; }
            set { m_locomotion = value; }
        }

        public Biology.TrophicLevel TrophicLevel
        {
            get { return m_trophicLevel; }
            set { m_trophicLevel = value; }
        }

        /// <summary>Coldest tile this species will live on.</summary>
        public double MinTemperatureCelsius
        {
            get { return m_minTemperatureCelsius; }
            set { m_minTemperatureCelsius = value; }
        }

        public double MaxTemperatureCelsius
        {
            get { return m_maxTemperatureCelsius; }
            set { m_maxTemperatureCelsius = value; }
        }

        public double MinRainfallMm
        {
            get { return m_minRainfallMm; }
            set { m_minRainfallMm = value; }
        }

        public double MaxRainfallMm
        {
            get { return m_maxRainfallMm; }
            set { m_maxRainfallMm = value; }
        }

        public double MaxElevationMetres
        {
            get { return m_maxElevationMetres; }
            set { m_maxElevationMetres = value; }
        }

        /// <summary>Only live in water, i.e. on a tile whose terrain is aquatic.</summary>
        public bool NeedsWaterTile
        {
            get { return m_needsWaterTile; }
            set { m_needsWaterTile = value; }
        }

        /// <summary>Only live on dry land.</summary>
        public bool NeedsLandTile
        {
            get { return m_needsLandTile; }
            set { m_needsLandTile = value; }
        }

        /// <summary>Only live on a tile that touches water.</summary>
        public bool NeedsCoast
        {
            get { return m_needsCoast; }
            set { m_needsCoast = value; }
        }

        /// <summary>Refuses to live in a desert biome.</summary>
        public bool AvoidsDesert
        {
            get { return m_avoidsDesert; }
            set { m_avoidsDesert = value; }
        }

        /// <summary>Refuses to live on an ice sheet.</summary>
        public bool AvoidsIce
        {
            get { return m_avoidsIce; }
            set { m_avoidsIce = value; }
        }

        /// <summary>Fraction the population grows by per tick when conditions are good.</summary>
        public double GrowthRate
        {
            get { return m_growthRate; }
            set { m_growthRate = value; }
        }

        /// <summary>Fraction the population loses per tick when conditions are poor.</summary>
        public double DeathRate
        {
            get { return m_deathRate; }
            set { m_deathRate = value; }
        }

        /// <summary>The carrying capacity of a single tile: no more than this per tile.</summary>
        public int MaxPopulation
        {
            get { return m_maxPopulation; }
            set { m_maxPopulation = value; }
        }

        /// <summary>Headcount placed on a good tile when the ecosystem is seeded.</summary>
        public int SeedPopulation
        {
            get { return m_seedPopulation; }
            set { m_seedPopulation = value; }
        }

        /// <summary>One character drawn on the map to stand for this species.</summary>
        public char Symbol
        {
            get { return m_symbol; }
            set { m_symbol = value; }
        }

        /// <summary>A name for the colour this species is drawn in.</summary>
        public string ColourKey
        {
            get { return m_colourKey; }
            set { m_colourKey = value; }
        }

        public Species(string name)
        {
            m_name = name;
            m_kingdom = Biology.Kingdom.Animalia;
            m_diet = Biology.DietType.Herbivore;
            m_locomotion = Biology.LocomotionType.Walking;
            m_trophicLevel = Biology.TrophicLevel.PrimaryConsumer;

            m_minTemperatureCelsius = -40.0;
            m_maxTemperatureCelsius = 60.0;
            m_minRainfallMm = 0.0;
            m_maxRainfallMm = 10000.0;
            m_maxElevationMetres = 9000.0;

            m_growthRate = 0.02;
            m_deathRate = 0.01;
            m_maxPopulation = 40;
            m_seedPopulation = 20;

            m_symbol = '?';
            m_colourKey = "Green";
        }

        /// <summary>
        /// The single decision this class exists to make. Positive means the
        /// tile suits the species, negative means it does not, and the size says
        /// how strongly.
        /// </summary>
        public double Suitability(Biology.Tile tile)
        {
            if (tile == null)
                return -1.0;

            double score = 1.0;

            if (tile.TemperatureCelsius < m_minTemperatureCelsius ||
                tile.TemperatureCelsius > m_maxTemperatureCelsius)
                return -1.0;

            if (tile.RainfallMm < m_minRainfallMm || tile.RainfallMm > m_maxRainfallMm)
                return -1.0;

            if (tile.ElevationMetres > m_maxElevationMetres)
                return -1.0;

            if (m_needsWaterTile && !tile.IsWater)
                return -1.0;

            if (m_needsLandTile && !tile.IsLand)
                return -1.0;

            if (m_needsCoast && !tile.IsCoastal)
                return -1.0;

            if (m_avoidsDesert && IsDesert(tile.Biome))
                return -1.0;

            if (m_avoidsIce && tile.Biome == Biology.BiomeType.IceSheet)
                return -1.0;

            // The middle of the comfortable range is the best place to be.
            score = Band(tile.TemperatureCelsius, m_minTemperatureCelsius, m_maxTemperatureCelsius);

            if (m_maxRainfallMm > m_minRainfallMm + 1.0)
                score = (score + Band(tile.RainfallMm, m_minRainfallMm, m_maxRainfallMm)) / 2.0;

            return score;
        }

        public bool CanLiveOn(Biology.Tile tile)
        {
            return Suitability(tile) >= 0.0;
        }

        /// <summary>
        /// How much food a tile offers. Producers make their own, so terrain and
        /// rainfall are the whole story for them; consumers do better on rich,
        /// wet, low ground.
        /// </summary>
        public double FoodYield(Biology.Tile tile)
        {
            if (tile == null)
                return 0.0;

            double productivity = 0.4;

            if (tile.RainfallMm > 2000.0)
                productivity += 0.4;
            else if (tile.RainfallMm > 1000.0)
                productivity += 0.3;
            else if (tile.RainfallMm > 400.0)
                productivity += 0.2;

            if (tile.ElevationMetres < 500.0)
                productivity += 0.2;
            else if (tile.ElevationMetres > 2000.0)
                productivity -= 0.3;

            if (tile.HasRiver)
                productivity += 0.2;

            if (productivity < 0.0)
                productivity = 0.0;

            return productivity;
        }

        /// <summary>
        /// The ideal headcount for one tile: its carrying capacity scaled by both
        /// how suitable the tile is and how productive it is.
        /// </summary>
        public int TargetPopulation(Biology.Tile tile)
        {
            double suitability = Suitability(tile);

            if (suitability < 0.0)
                return 0;

            double target = m_maxPopulation * suitability * (0.5 + FoodYield(tile));

            if (target < 0.0)
                target = 0.0;

            if (target > m_maxPopulation)
                target = m_maxPopulation;

            return (int)Math.Round(target);
        }

        private static double Band(double value, double minimum, double maximum)
        {
            if (maximum <= minimum)
                return 1.0;

            double middle = (minimum + maximum) / 2.0;
            double halfRange = (maximum - minimum) / 2.0;
            double distance = Math.Abs(value - middle);

            return 1.0 - (distance / halfRange) * 0.5;
        }

        private static bool IsDesert(Biology.BiomeType biome)
        {
            return biome == Biology.BiomeType.HotDesert ||
                   biome == Biology.BiomeType.ColdDesert;
        }

        public override string ToString()
        {
            return m_name + " (" + m_trophicLevel + ")";
        }
    }

    /// <summary>
    /// The living world. An ecosystem holds a <see cref="Biology.WorldMap"/> and
    /// the species that live on it, and it ticks: once per tick every tile of
    /// every species grows toward its target, is cut back by what has not been
    /// eaten, and is grazed by whatever eats it.
    ///
    /// Growth is logistic rather than arithmetic: each tile moves a fraction of
    /// the way toward its target, so populations approach a carrying capacity
    /// instead of exploding, and a tile that has been stripped of plants fills
    /// back up at a rate that depends on how empty it is.
    /// </summary>
    public sealed class Ecosystem
    {
        private string m_name;
        private Biology.WorldMap m_world;
        private List<Species> m_species;
        private Dictionary<string, Species> m_index;
        private int m_tick;
        private readonly int m_seed;
        private int m_births;
        private int m_deaths;
        private int m_extinctions;

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public Biology.WorldMap World
        {
            get { return m_world; }
        }

        /// <summary>Every species ever registered, alive or extinct.</summary>
        public IList<Species> Species
        {
            get { return m_species; }
        }

        /// <summary>How many ticks the ecosystem has run.</summary>
        public int Tick
        {
            get { return m_tick; }
        }

        public int Seed
        {
            get { return m_seed; }
        }

        /// <summary>Total headcounts that have grown, since the run began.</summary>
        public int Births
        {
            get { return m_births; }
        }

        /// <summary>Total headcounts that have died, since the run began.</summary>
        public int Deaths
        {
            get { return m_deaths; }
        }

        /// <summary>How many species have been wiped out.</summary>
        public int Extinctions
        {
            get { return m_extinctions; }
        }

        public Ecosystem(string name, Biology.WorldMap world, int seed)
        {
            m_name = name;
            m_world = world;
            m_seed = seed;
            m_species = new List<Species>();
            m_index = new Dictionary<string, Species>();
        }

        // ------------------------------------------------------------------
        // Species registry
        // ------------------------------------------------------------------

        public Species Register(Species species)
        {
            if (species == null)
                return null;

            if (!m_species.Contains(species))
                m_species.Add(species);

            if (!string.IsNullOrEmpty(species.Name))
                m_index[species.Name] = species;

            return species;
        }

        public Species Find(string name)
        {
            Species found;

            if (name != null && m_index.TryGetValue(name, out found))
                return found;

            return null;
        }

        /// <summary>Every species still present somewhere on the map.</summary>
        public List<Species> GetLivingSpecies()
        {
            List<Species> living = new List<Species>();
            HashSet<Species> present = new HashSet<Species>();

            foreach (Biology.Tile tile in m_world.Tiles)
            {
                for (int i = 0; i < tile.Organisms.Count; i++)
                {
                    Species species = tile.Organisms[i] as Species;

                    if (species != null)
                        present.Add(species);
                }
            }

            for (int i = 0; i < m_species.Count; i++)
            {
                if (present.Contains(m_species[i]))
                    living.Add(m_species[i]);
            }

            return living;
        }

        public int GetPopulation(Species species)
        {
            if (species == null)
                return 0;

            int total = 0;

            foreach (Biology.Tile tile in m_world.Tiles)
                total += tile.GetPopulation(species);

            return total;
        }

        /// <summary>Total headcount of everything on the map.</summary>
        public int TotalPopulation()
        {
            return m_world.TotalPopulation();
        }

        // ------------------------------------------------------------------
        // Seeding
        // ------------------------------------------------------------------

        /// <summary>
        /// Drops each species onto every tile it can live on, at its seed
        /// headcount. Call it once, after the species are registered.
        /// </summary>
        public void SeedPopulations()
        {
            for (int s = 0; s < m_species.Count; s++)
            {
                Species species = m_species[s];
                Random random = new Random(m_seed + s * 101);

                foreach (Biology.Tile tile in m_world.Tiles)
                {
                    if (!species.CanLiveOn(tile))
                        continue;

                    // Not every suitable tile starts occupied: leave gaps so the
                    // world can spread into them.
                    if (random.NextDouble() > 0.55)
                        continue;

                    int population = (int)Math.Round(species.SeedPopulation * (0.5 + random.NextDouble()));

                    if (population < 1)
                        population = 1;

                    tile.AddOrganism(species, population);
                }
            }

            m_tick = 0;
            m_births = 0;
            m_deaths = 0;
        }

        /// <summary>Wipes every population off the map but keeps the species.</summary>
        public void Clear()
        {
            foreach (Biology.Tile tile in m_world.Tiles)
                tile.ClearPopulation();

            m_tick = 0;
            m_births = 0;
            m_deaths = 0;
            m_extinctions = 0;
        }

        // ------------------------------------------------------------------
        // The tick
        // ------------------------------------------------------------------

        /// <summary>
        /// Advances the ecosystem by one tick. Producers are grown first, then
        /// every consumer in trophic order, so a herbivore always eats this
        /// tick's grass rather than last tick's.
        /// </summary>
        public void Step()
        {
            m_tick++;

            for (int s = 0; s < m_species.Count; s++)
            {
                Species species = m_species[s];

                if (species.TrophicLevel == Biology.TrophicLevel.Producer)
                    Grow(species);
            }

            for (int s = 0; s < m_species.Count; s++)
            {
                Species species = m_species[s];

                if (species.TrophicLevel == Biology.TrophicLevel.Producer)
                    continue;

                Grow(species);
                GrazeConsumers(species);
            }
        }

        /// <summary>
        /// Moves every population one step toward what its tile can carry.
        /// Uses (target - current) so a healthy tile fills up and an overgrazed
        /// one recovers at a rate set by how far short it is.
        /// </summary>
        private void Grow(Species species)
        {
            foreach (Biology.Tile tile in m_world.Tiles)
            {
                int current = tile.GetPopulation(species);
                int target = species.TargetPopulation(tile);

                if (target <= 0)
                {
                    if (current <= 0)
                        continue;

                    // Conditions have turned against it: die back.
                    int lost = LossOf(current, 0, species.DeathRate * 3.0);
                    tile.AddOrganism(species, -lost);
                    m_deaths += lost;
                    continue;
                }

                if (current < target)
                {
                    int gained = GrowthOf(current, target, species.GrowthRate);
                    tile.AddOrganism(species, gained);
                    m_births += gained;
                }
                else if (current > target)
                {
                    int lost = LossOf(current, target, species.DeathRate);
                    tile.AddOrganism(species, -lost);
                    m_deaths += lost;
                }
            }
        }

        /// <summary>
        /// A consumer eats the plants, and the other consumers, on its tile. The
        /// eaten population is reduced; if there is nothing to eat, it starves.
        /// </summary>
        private void GrazeConsumers(Species species)
        {
            foreach (Biology.Tile tile in m_world.Tiles)
            {
                int eaters = tile.GetPopulation(species);

                if (eaters <= 0)
                    continue;

                double appetite = 0.15;

                if (species.Diet == Biology.DietType.Carnivore)
                    appetite = 0.25;
                else if (species.TrophicLevel >= Biology.TrophicLevel.TertiaryConsumer)
                    appetite = 0.3;

                bool foundFood = false;

                for (int i = 0; i < tile.Organisms.Count; i++)
                {
                    Species other = tile.Organisms[i] as Species;

                    if (other == null || other == species)
                        continue;

                    if (!CanEat(species, other))
                        continue;

                    int available = tile.GetPopulation(other);

                    if (available <= 0)
                        continue;

                    int eaten = (int)Math.Round(eaters * appetite);

                    // One consumer removes at most a fifth of the food there is.
                    int cap = available / 5 + 1;

                    if (eaten > cap)
                        eaten = cap;

                    if (eaten > available)
                        eaten = available;

                    if (eaten <= 0)
                        continue;

                    tile.AddOrganism(other, -eaten);
                    m_deaths += eaten;
                    foundFood = true;
                }

                if (!foundFood)
                {
                    // Nothing to eat here: starve.
                    int starved = LossOf(eaters, 0, species.DeathRate * 2.0);
                    tile.AddOrganism(species, -starved);
                    m_deaths += starved;
                }
            }
        }

        private static bool CanEat(Species eater, Species prey)
        {
            if (eater.TrophicLevel == Biology.TrophicLevel.Producer)
                return false;

            if (eater.TrophicLevel <= prey.TrophicLevel)
                return false;

            if (eater.Diet == Biology.DietType.Carnivore)
                return prey.TrophicLevel >= Biology.TrophicLevel.PrimaryConsumer;

            // An omnivore will take a herbivore but not another predator.
            if (eater.Diet == Biology.DietType.Omnivore)
                return true;

            return prey.TrophicLevel == Biology.TrophicLevel.Producer;
        }

        private static int GrowthOf(int current, int target, double rate)
        {
            double grown = (target - current) * rate;

            // Always grow by at least one while there is room, or a small
            // population can stall below the rounding threshold forever.
            int gained = (int)Math.Round(grown);

            if (gained < 1 && current < target)
                gained = 1;

            if (current + gained > target)
                gained = target - current;

            return gained;
        }

        private static int LossOf(int current, int target, double rate)
        {
            int lost = (int)Math.Round((current - target) * rate);

            if (lost < 1 && current > target)
                lost = 1;

            if (current - lost < 0)
                lost = current;

            return lost;
        }

        // ------------------------------------------------------------------
        // Reporting
        // ------------------------------------------------------------------

        /// <summary>Ticks the ecosystem until nothing changes much, or the limit is hit.</summary>
        public void Run(int ticks)
        {
            for (int i = 0; i < ticks; i++)
                Step();
        }

        /// <summary>Species with nobody left anywhere on the map.</summary>
        public List<Species> GetExtinctSpecies()
        {
            List<Species> extinct = new List<Species>();

            for (int i = 0; i < m_species.Count; i++)
            {
                if (GetPopulation(m_species[i]) <= 0)
                    extinct.Add(m_species[i]);
            }

            return extinct;
        }

        /// <summary>
        /// A plain-text report of the world and everything living on it: the
        /// kind of summary a sidebar or a console would print.
        /// </summary>
        public string Summarise()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.AppendLine("Ecosystem: " + m_name);
            builder.AppendLine("World:     " + m_world);
            builder.AppendLine("Tick:      " + m_tick);
            builder.AppendLine("Sea level: " + m_world.SeaLevelMetres + " m");
            builder.AppendLine("Land:      " + Math.Round(m_world.LandFraction() * 100.0, 1) + "% of tiles");
            builder.AppendLine("Mean temp: " + Math.Round(m_world.AverageTemperatureCelsius(), 1) + " C");
            builder.AppendLine("Mean elev: " + Math.Round(m_world.AverageElevationMetres()) + " m");
            builder.AppendLine("Rivers:    " + m_world.RiverCourses.Count);
            builder.AppendLine();
            builder.AppendLine("Biomes");

            Dictionary<Biology.BiomeType, int> census = m_world.BiomeCensus();

            List<Biology.BiomeType> keys = new List<Biology.BiomeType>(census.Keys);
            keys.Sort();

            for (int i = 0; i < keys.Count; i++)
            {
                builder.AppendLine("  " + Pad(keys[i].ToString(), 26) + census[keys[i]] + " tiles");
            }

            builder.AppendLine();
            builder.AppendLine("Species");

            List<Species> living = GetLivingSpecies();

            for (int i = 0; i < m_species.Count; i++)
            {
                Species species = m_species[i];
                int population = GetPopulation(species);
                string state = population > 0 ? population.ToString() : "EXTINCT";

                builder.AppendLine("  " + Pad(species.Name, 26) +
                                   Pad(species.TrophicLevel.ToString(), 20) + state);
            }

            builder.AppendLine();
            builder.AppendLine("Living species: " + living.Count + " of " + m_species.Count);
            builder.AppendLine("Births:         " + m_births);
            builder.AppendLine("Deaths:         " + m_deaths);

            return builder.ToString();
        }

        private static string Pad(string text, int width)
        {
            if (text == null)
                text = string.Empty;

            if (text.Length >= width)
                return text + " ";

            return text + new string(' ', width - text.Length);
        }

        public override string ToString()
        {
            return m_name + " (" + m_species.Count + " species, tick " + m_tick + ")";
        }
    }
}
