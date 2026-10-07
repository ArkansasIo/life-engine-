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
    /// The largest-scale classification of a climate region.
    /// </summary>
    public enum BiomeType
    {
        TropicalRainforest,
        TropicalSeasonalForest,
        TropicalSavanna,
        HotDesert,
        ColdDesert,
        Mediterranean,
        TemperateGrassland,
        TemperateDeciduousForest,
        TemperateRainforest,
        BorealForest,
        Tundra,
        IceSheet,
        Mountain,
        Wetland,
        Freshwater,
        Ocean
    }

    /// <summary>
    /// Broad physical environment in which a biome sits.
    /// </summary>
    public enum ClimateZone
    {
        Polar,
        Subpolar,
        Temperate,
        Subtropical,
        Tropical,
        Arid,
        Alpine,
        Aquatic,
        DeepSea,
        Hydrothermal
    }

    /// <summary>
    /// Vertical or horizontal banding within a biome, e.g. the bathypelagic zone
    /// of the ocean or the canopy of a forest.
    /// </summary>
    public enum ZoneType
    {
        Epipelagic,
        Mesopelagic,
        Bathypelagic,
        Abyssopelagic,
        Hadalpelagic,
        Littoral,
        Limnetic,
        Profundal,
        Canopy,
        Understory,
        ForestFloor,
        Riparian,
        Intertidal,
        Neritic,
        Oceanic,
        Soil,
        Subterranean,
        Aeriel
    }

    /// <summary>
    /// Terrain an organism can live on or move across.
    /// </summary>
    public enum HabitatTerrain
    {
        Ocean,
        Coast,
        River,
        Lake,
        Wetland,
        Grassland,
        Forest,
        Jungle,
        Desert,
        Mountain,
        Tundra,
        Ice,
        Cave,
        Urban,
        Farmland,
        Soil,
        Host
    }

    /// <summary>
    /// The trophic level an organism occupies in a food web.
    /// </summary>
    public enum TrophicLevel
    {
        Producer,
        PrimaryConsumer,
        SecondaryConsumer,
        TertiaryConsumer,
        ApexPredator,
        Decomposer,
        Detritivore
    }

    /// <summary>
    /// A zone inside a biome. Zones may nest, e.g. a forest biome holds a canopy
    /// zone, which itself holds a nesting zone.
    /// </summary>
    public sealed class Zone
    {
        private string m_name;
        private ZoneType m_type;
        private Zone m_parent;
        private List<Zone> m_subZones;
        private double m_minAltitudeMetres;
        private double m_maxAltitudeMetres;
        private double m_minTemperatureCelsius;
        private double m_maxTemperatureCelsius;
        private double m_salinityPpt;

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public ZoneType Type
        {
            get { return m_type; }
            set { m_type = value; }
        }

        public Zone Parent
        {
            get { return m_parent; }
        }

        public IList<Zone> SubZones
        {
            get { return m_subZones; }
        }

        /// <summary>Depth or height band, in metres above/below the reference surface.</summary>
        public double MinAltitudeMetres
        {
            get { return m_minAltitudeMetres; }
            set { m_minAltitudeMetres = value; }
        }

        public double MaxAltitudeMetres
        {
            get { return m_maxAltitudeMetres; }
            set { m_maxAltitudeMetres = value; }
        }

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

        /// <summary>Salt content in parts per thousand.</summary>
        public double SalinityPpt
        {
            get { return m_salinityPpt; }
            set { m_salinityPpt = value; }
        }

        public Zone(string name, ZoneType type)
        {
            m_name = name;
            m_type = type;
            m_subZones = new List<Zone>();
            m_maxTemperatureCelsius = 30.0;
            m_minTemperatureCelsius = -10.0;
        }

        public Zone AddSubZone(Zone subZone)
        {
            if (subZone == null || subZone == this)
                return null;

            subZone.m_parent = this;

            if (!m_subZones.Contains(subZone))
                m_subZones.Add(subZone);

            return subZone;
        }

        /// <summary>True if a temperature falls inside this zone's band.</summary>
        public bool IsTemperatureSuitable(double celsius)
        {
            return celsius >= m_minTemperatureCelsius && celsius <= m_maxTemperatureCelsius;
        }

        public override string ToString()
        {
            return m_name + " (" + m_type + ")";
        }
    }

    /// <summary>
    /// A biome: a large community of plants and animals shaped by climate.
    /// </summary>
    public class Biome
    {
        private string m_name;
        private BiomeType m_type;
        private ClimateZone m_climateZone;
        private double m_averageTemperatureCelsius;
        private double m_averageRainfallMm;
        private double m_latitude;
        private bool m_isAquatic;
        private bool m_isMarine;
        private List<Zone> m_zones;
        private List<HabitatTerrain> m_terrains;
        private List<Organism> m_organisms;
        private List<Species> m_species;

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public BiomeType Type
        {
            get { return m_type; }
            set { m_type = value; }
        }

        public ClimateZone ClimateZone
        {
            get { return m_climateZone; }
            set { m_climateZone = value; }
        }

        public double AverageTemperatureCelsius
        {
            get { return m_averageTemperatureCelsius; }
            set { m_averageTemperatureCelsius = value; }
        }

        /// <summary>Average yearly rainfall in millimetres.</summary>
        public double AverageRainfallMm
        {
            get { return m_averageRainfallMm; }
            set { m_averageRainfallMm = value; }
        }

        public double Latitude
        {
            get { return m_latitude; }
            set { m_latitude = value; }
        }

        public bool IsAquatic
        {
            get { return m_isAquatic; }
            set { m_isAquatic = value; }
        }

        public bool IsMarine
        {
            get { return m_isMarine; }
            set { m_isMarine = value; }
        }

        public IList<Zone> Zones
        {
            get { return m_zones; }
        }

        public IList<HabitatTerrain> Terrains
        {
            get { return m_terrains; }
        }

        public IList<Organism> Organisms
        {
            get { return m_organisms; }
        }

        public IList<Species> Species
        {
            get { return m_species; }
        }

        public Biome(string name, BiomeType type, ClimateZone climateZone)
        {
            m_name = name;
            m_type = type;
            m_climateZone = climateZone;
            m_zones = new List<Zone>();
            m_terrains = new List<HabitatTerrain>();
            m_organisms = new List<Organism>();
            m_species = new List<Species>();
        }

        public Zone AddZone(Zone zone)
        {
            if (zone != null && !m_zones.Contains(zone))
                m_zones.Add(zone);

            return zone;
        }

        public Zone AddZone(string name, ZoneType type)
        {
            return AddZone(new Zone(name, type));
        }

        public void AddTerrain(HabitatTerrain terrain)
        {
            if (!m_terrains.Contains(terrain))
                m_terrains.Add(terrain);
        }

        public void AddOrganism(Organism organism)
        {
            if (organism != null && !m_organisms.Contains(organism))
                m_organisms.Add(organism);
        }

        public void AddSpecies(Species species)
        {
            if (species != null && !m_species.Contains(species))
                m_species.Add(species);
        }

        /// <summary>Number of organisms currently recorded in the biome.</summary>
        public int CountOrganisms()
        {
            return m_organisms.Count;
        }

        public override string ToString()
        {
            return m_name + " [" + m_type + ", " + m_climateZone + "]";
        }
    }

    /// <summary>
    /// A collection of biomes, together with the world-level parameters that
    /// drive them.
    /// </summary>
    public sealed class BiomeMap
    {
        private string m_worldName;
        private List<Biome> m_biomes;
        private double m_axialTiltDegrees;
        private double m_dayLengthHours;
        private double m_gravityMs2;

        public string WorldName
        {
            get { return m_worldName; }
            set { m_worldName = value; }
        }

        public IList<Biome> Biomes
        {
            get { return m_biomes; }
        }

        /// <summary>Planetary axial tilt, which sets the strength of seasons.</summary>
        public double AxialTiltDegrees
        {
            get { return m_axialTiltDegrees; }
            set { m_axialTiltDegrees = value; }
        }

        public double DayLengthHours
        {
            get { return m_dayLengthHours; }
            set { m_dayLengthHours = value; }
        }

        /// <summary>Surface gravity in metres per second squared.</summary>
        public double GravityMs2
        {
            get { return m_gravityMs2; }
            set { m_gravityMs2 = value; }
        }

        public BiomeMap(string worldName)
        {
            m_worldName = worldName;
            m_biomes = new List<Biome>();
            m_axialTiltDegrees = 23.44;
            m_dayLengthHours = 24.0;
            m_gravityMs2 = 9.81;
        }

        public Biome AddBiome(Biome biome)
        {
            if (biome != null && !m_biomes.Contains(biome))
                m_biomes.Add(biome);

            return biome;
        }

        public Biome AddBiome(string name, BiomeType type, ClimateZone climateZone)
        {
            return AddBiome(new Biome(name, type, climateZone));
        }

        /// <summary>Returns every biome of a given type.</summary>
        public List<Biome> GetBiomesOfType(BiomeType type)
        {
            List<Biome> result = new List<Biome>();

            for (int i = 0; i < m_biomes.Count; i++)
            {
                if (m_biomes[i].Type == type)
                    result.Add(m_biomes[i]);
            }

            return result;
        }

        /// <summary>Total number of organisms across every biome.</summary>
        public int CountOrganisms()
        {
            int count = 0;

            for (int i = 0; i < m_biomes.Count; i++)
                count += m_biomes[i].CountOrganisms();

            return count;
        }

        public override string ToString()
        {
            return m_worldName + " (" + m_biomes.Count + " biomes)";
        }
    }
}
