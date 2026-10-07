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
    /// One square of the world: where it is, what climate it has, what terrain is
    /// on it and what lives there.
    /// </summary>
    public sealed class Tile
    {
        private int m_x;
        private int m_y;
        private double m_latitude;
        private double m_longitude;
        private double m_temperatureCelsius;
        private double m_elevationMetres;
        private double m_rainfallMm;
        private bool m_hasRiver;
        private bool m_isCoastal;
        private BiomeType m_biome;
        private ClimateZone m_climateZone;
        private HabitatTerrain m_terrain;
        private ZoneType m_zone;
        private List<object> m_organisms;
        private List<int> m_populations;

        public int X
        {
            get { return m_x; }
        }

        public int Y
        {
            get { return m_y; }
        }

        /// <summary>Degrees north or south. Negative is the southern hemisphere.</summary>
        public double Latitude
        {
            get { return m_latitude; }
            set { m_latitude = value; }
        }

        /// <summary>Degrees east or west, from -180 to 180.</summary>
        public double Longitude
        {
            get { return m_longitude; }
            set { m_longitude = value; }
        }

        public double TemperatureCelsius
        {
            get { return m_temperatureCelsius; }
            set { m_temperatureCelsius = value; }
        }

        /// <summary>Height above sea level. Negative is below it.</summary>
        public double ElevationMetres
        {
            get { return m_elevationMetres; }
            set { m_elevationMetres = value; }
        }

        public double RainfallMm
        {
            get { return m_rainfallMm; }
            set { m_rainfallMm = value; }
        }

        public bool HasRiver
        {
            get { return m_hasRiver; }
            set { m_hasRiver = value; }
        }

        /// <summary>True if this tile touches a water tile.</summary>
        public bool IsCoastal
        {
            get { return m_isCoastal; }
            set { m_isCoastal = value; }
        }

        public BiomeType Biome
        {
            get { return m_biome; }
            set { m_biome = value; }
        }

        public ClimateZone ClimateZone
        {
            get { return m_climateZone; }
            set { m_climateZone = value; }
        }

        public HabitatTerrain Terrain
        {
            get { return m_terrain; }
            set { m_terrain = value; }
        }

        public ZoneType Zone
        {
            get { return m_zone; }
            set { m_zone = value; }
        }

        /// <summary>Species recorded on this tile.</summary>
        public IList<object> Organisms
        {
            get { return m_organisms; }
        }

        /// <summary>
        /// How many of each species live here, parallel to <see cref="Organisms"/>.
        /// This is what the ecosystem ticks.
        /// </summary>
        public IList<int> Populations
        {
            get { return m_populations; }
        }

        public Tile(int x, int y)
        {
            m_x = x;
            m_y = y;
            m_organisms = new List<object>();
            m_populations = new List<int>();
        }

        public bool IsWater
        {
            get
            {
                return m_terrain == HabitatTerrain.Ocean ||
                       m_terrain == HabitatTerrain.Coast ||
                       m_terrain == HabitatTerrain.Lake ||
                       m_terrain == HabitatTerrain.River ||
                       m_terrain == HabitatTerrain.Wetland;
            }
        }

        public bool IsLand
        {
            get { return !IsWater; }
        }

        /// <summary>Adds a species and gives it a starting headcount.</summary>
        public void AddOrganism(object organism, int population)
        {
            if (organism == null)
                return;

            int index = m_organisms.IndexOf(organism);

            if (index < 0)
            {
                m_organisms.Add(organism);
                m_populations.Add(population < 0 ? 0 : population);
            }
            else
            {
                m_populations[index] += population;
            }
        }

        /// <summary>Headcount of one species, 0 if it does not live here.</summary>
        public int GetPopulation(object organism)
        {
            int index = m_organisms.IndexOf(organism);

            if (index < 0)
                return 0;

            return m_populations[index];
        }

        /// <summary>All headcounts on this tile added together.</summary>
        public int TotalPopulation()
        {
            int total = 0;

            for (int i = 0; i < m_populations.Count; i++)
                total += m_populations[i];

            return total;
        }

        public void ClearPopulation()
        {
            m_organisms.Clear();
            m_populations.Clear();
        }

        public override string ToString()
        {
            return "(" + m_x + "," + m_y + ") " + m_biome + " " +
                   Math.Round(m_temperatureCelsius) + "C";
        }
    }

    /// <summary>
    /// A horizontal band of the world: everything at the same latitude shares a
    /// base climate.
    /// </summary>
    public sealed class LatitudeBand
    {
        private string m_name;
        private double m_minLatitude;
        private double m_maxLatitude;
        private double m_baseTemperatureCelsius;
        private double m_baseRainfallMm;
        private ClimateZone m_climateZone;

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public double MinLatitude
        {
            get { return m_minLatitude; }
            set { m_minLatitude = value; }
        }

        public double MaxLatitude
        {
            get { return m_maxLatitude; }
            set { m_maxLatitude = value; }
        }

        public double BaseTemperatureCelsius
        {
            get { return m_baseTemperatureCelsius; }
            set { m_baseTemperatureCelsius = value; }
        }

        public double BaseRainfallMm
        {
            get { return m_baseRainfallMm; }
            set { m_baseRainfallMm = value; }
        }

        public ClimateZone ClimateZone
        {
            get { return m_climateZone; }
            set { m_climateZone = value; }
        }

        public LatitudeBand(string name, double minLatitude, double maxLatitude,
                            double baseTemperatureCelsius, double baseRainfallMm,
                            ClimateZone climateZone)
        {
            m_name = name;
            m_minLatitude = minLatitude;
            m_maxLatitude = maxLatitude;
            m_baseTemperatureCelsius = baseTemperatureCelsius;
            m_baseRainfallMm = baseRainfallMm;
            m_climateZone = climateZone;
        }

        public override string ToString()
        {
            return m_name + " (" + m_minLatitude + " to " + m_maxLatitude + " deg)";
        }
    }

    /// <summary>
    /// The world map. A grid of <see cref="Tile"/>s with a latitude gradient, an
    /// ocean, a noise seed, rivers and watersheds, and a biome decided per tile
    /// from its temperature and rainfall.
    ///
    /// The generation is deterministic: the same seed and the same size always
    /// produce the same world.
    /// </summary>
    public sealed class WorldMap
    {
        private string m_name;
        private int m_columns;
        private int m_rows;
        private int m_seed;
        private double m_seaLevelMetres;
        private double m_axialTiltDegrees;
        private double m_dayLengthHours;
        private double m_gravityMs2;
        private Tile[] m_tiles;
        private List<LatitudeBand> m_bands;
        private List<string> m_riverCourses;

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public int Columns
        {
            get { return m_columns; }
        }

        public int Rows
        {
            get { return m_rows; }
        }

        public int Seed
        {
            get { return m_seed; }
        }

        /// <summary>Everything at or below this height is water.</summary>
        public double SeaLevelMetres
        {
            get { return m_seaLevelMetres; }
            set { m_seaLevelMetres = value; }
        }

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

        public double GravityMs2
        {
            get { return m_gravityMs2; }
            set { m_gravityMs2 = value; }
        }

        /// <summary>The climate bands, from pole to pole.</summary>
        public IList<LatitudeBand> Bands
        {
            get { return m_bands; }
        }

        /// <summary>Names of the rivers that were cut, for the world summary.</summary>
        public IList<string> RiverCourses
        {
            get { return m_riverCourses; }
        }

        public int TileCount
        {
            get { return m_columns * m_rows; }
        }

        public WorldMap(string name, int columns, int rows, int seed)
        {
            if (columns < 2)
                columns = 2;

            if (rows < 2)
                rows = 2;

            m_name = name;
            m_columns = columns;
            m_rows = rows;
            m_seed = seed;
            m_seaLevelMetres = 0.0;
            m_axialTiltDegrees = 23.44;
            m_dayLengthHours = 24.0;
            m_gravityMs2 = 9.81;
            m_tiles = new Tile[columns * rows];
            m_bands = new List<LatitudeBand>();
            m_riverCourses = new List<string>();

            BuildBands();
            BuildTiles();
            CutRivers();
            MarkCoastlines();
            ClassifyBiomes();
        }

        // ------------------------------------------------------------------
        // Access
        // ------------------------------------------------------------------

        public Tile GetTile(int x, int y)
        {
            if (x < 0 || x >= m_columns || y < 0 || y >= m_rows)
                return null;

            return m_tiles[x + y * m_columns];
        }

        public void SetTile(int x, int y, Tile tile)
        {
            if (x < 0 || x >= m_columns || y < 0 || y >= m_rows)
                return;

            m_tiles[x + y * m_columns] = tile;
        }

        public IEnumerable<Tile> Tiles
        {
            get
            {
                for (int i = 0; i < m_tiles.Length; i++)
                    yield return m_tiles[i];
            }
        }

        /// <summary>The climate band a latitude falls in.</summary>
        public LatitudeBand GetBand(double latitude)
        {
            for (int i = 0; i < m_bands.Count; i++)
            {
                if (latitude >= m_bands[i].MinLatitude && latitude < m_bands[i].MaxLatitude)
                    return m_bands[i];
            }

            return m_bands[m_bands.Count - 1];
        }

        /// <summary>How many tiles carry a given biome.</summary>
        public int CountBiome(BiomeType type)
        {
            int count = 0;

            for (int i = 0; i < m_tiles.Length; i++)
            {
                if (m_tiles[i].Biome == type)
                    count++;
            }

            return count;
        }

        /// <summary>Every biome present on the map, with its tile count.</summary>
        public Dictionary<BiomeType, int> BiomeCensus()
        {
            Dictionary<BiomeType, int> census = new Dictionary<BiomeType, int>();

            for (int i = 0; i < m_tiles.Length; i++)
            {
                BiomeType type = m_tiles[i].Biome;

                if (!census.ContainsKey(type))
                    census[type] = 0;

                census[type] = census[type] + 1;
            }

            return census;
        }

        public double AverageTemperatureCelsius()
        {
            if (m_tiles.Length == 0)
                return 0.0;

            double total = 0.0;

            for (int i = 0; i < m_tiles.Length; i++)
                total += m_tiles[i].TemperatureCelsius;

            return total / m_tiles.Length;
        }

        public double AverageElevationMetres()
        {
            if (m_tiles.Length == 0)
                return 0.0;

            double total = 0.0;

            for (int i = 0; i < m_tiles.Length; i++)
                total += m_tiles[i].ElevationMetres;

            return total / m_tiles.Length;
        }

        /// <summary>Rough land fraction, 0 to 1.</summary>
        public double LandFraction()
        {
            if (m_tiles.Length == 0)
                return 0.0;

            int land = 0;

            for (int i = 0; i < m_tiles.Length; i++)
            {
                if (m_tiles[i].IsLand)
                    land++;
            }

            return (double)land / m_tiles.Length;
        }

        /// <summary>Everything living on the map, tile by tile.</summary>
        public int TotalPopulation()
        {
            int total = 0;

            for (int i = 0; i < m_tiles.Length; i++)
                total += m_tiles[i].TotalPopulation();

            return total;
        }

        /// <summary>The tiles around a tile, including diagonals. Fewer at the edges.</summary>
        public List<Tile> GetNeighbours(Tile tile)
        {
            List<Tile> neighbours = new List<Tile>(8);

            if (tile == null)
                return neighbours;

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    Tile neighbour = GetTile(tile.X + dx, tile.Y + dy);

                    if (neighbour != null)
                        neighbours.Add(neighbour);
                }
            }

            return neighbours;
        }

        // ------------------------------------------------------------------
        // Generation
        // ------------------------------------------------------------------

        private void BuildBands()
        {
            m_bands.Clear();

            m_bands.Add(new LatitudeBand("North Pole", 75.0, 91.0, -25.0, 100.0, ClimateZone.Polar));
            m_bands.Add(new LatitudeBand("Arctic", 60.0, 75.0, -8.0, 250.0, ClimateZone.Subpolar));
            m_bands.Add(new LatitudeBand("Boreal", 45.0, 60.0, 4.0, 500.0, ClimateZone.Temperate));
            m_bands.Add(new LatitudeBand("Temperate", 30.0, 45.0, 13.0, 750.0, ClimateZone.Temperate));
            m_bands.Add(new LatitudeBand("Subtropical", 15.0, 30.0, 23.0, 450.0, ClimateZone.Subtropical));
            m_bands.Add(new LatitudeBand("Tropical North", 0.0, 15.0, 28.0, 1800.0, ClimateZone.Tropical));
            m_bands.Add(new LatitudeBand("Tropical South", -15.0, 0.0, 28.0, 1800.0, ClimateZone.Tropical));
            m_bands.Add(new LatitudeBand("Subtropical South", -30.0, -15.0, 23.0, 450.0, ClimateZone.Subtropical));
            m_bands.Add(new LatitudeBand("Temperate South", -45.0, -30.0, 13.0, 750.0, ClimateZone.Temperate));
            m_bands.Add(new LatitudeBand("Boreal South", -60.0, -45.0, 4.0, 500.0, ClimateZone.Temperate));
            m_bands.Add(new LatitudeBand("Antarctic", -75.0, -60.0, -8.0, 250.0, ClimateZone.Subpolar));
            m_bands.Add(new LatitudeBand("South Pole", -91.0, -75.0, -25.0, 100.0, ClimateZone.Polar));
        }

        private void BuildTiles()
        {
            Noise noise = new Noise(m_seed);

            // Two octaves: a coarse continent shape plus finer terrain detail.
            Noise detail = new Noise(m_seed + 7919);

            for (int y = 0; y < m_rows; y++)
            {
                // Row 0 is the north pole, the last row is the south pole.
                double latitude = 90.0 - ((double)y / (m_rows - 1)) * 180.0;
                LatitudeBand band = GetBand(latitude);

                for (int x = 0; x < m_columns; x++)
                {
                    double longitude = -180.0 + ((double)x / (m_columns - 1)) * 360.0;

                    Tile tile = new Tile(x, y);
                    tile.Latitude = latitude;
                    tile.Longitude = longitude;
                    tile.ClimateZone = band.ClimateZone;

                    // Continent shape, then detail on top.
                    double u = (double)x / m_columns * 6.0;
                    double v = (double)y / m_rows * 3.0;

                    double continents = noise.Fractal(u, v, 3, 0.5);
                    double terrain = detail.Smooth(u * 4.0, v * 4.0);

                    double elevation = (continents - 0.45) * 9000.0;
                    elevation += (terrain - 0.5) * 900.0;

                    tile.ElevationMetres = elevation;

                    if (tile.ElevationMetres <= m_seaLevelMetres)
                    {
                        tile.Terrain = HabitatTerrain.Ocean;
                        tile.Zone = ZoneType.Oceanic;
                    }
                    else
                    {
                        // Rain: the band's rainfall, modulated by noise, and the
                        // rainshadow of height.
                        double rainfall = band.BaseRainfallMm * (0.4 + terrain * 1.2);

                        if (tile.ElevationMetres > 2000.0)
                            rainfall *= 0.5;

                        tile.RainfallMm = rainfall;
                        tile.Terrain = HabitatTerrain.Grassland;
                        tile.Zone = ZoneType.Soil;
                    }

                    double temperature = band.BaseTemperatureCelsius;

                    // Height costs about 6.5 degrees per kilometre, and the sea
                    // moderates what it touches.
                    double elevationKm = Math.Max(0.0, tile.ElevationMetres) / 1000.0;
                    temperature -= elevationKm * 6.5;

                    if (tile.IsWater)
                        temperature += 2.0;

                    if (temperature > -12.0)
                        temperature += (terrain - 0.5) * 4.0;

                    tile.TemperatureCelsius = temperature;

                    m_tiles[x + y * m_columns] = tile;
                }
            }
        }

        /// <summary>
        /// Walks a few rivers downhill from high ground to the sea. A river tile
        /// is fresh water no matter what the climate says.
        /// </summary>
        private void CutRivers()
        {
            int riverCount = Math.Max(3, (m_columns * m_rows) / 900);
            int river = 0;

            for (int attempt = 0; attempt < riverCount * 40 && river < riverCount; attempt++)
            {
                Noise source = new Noise(m_seed + attempt * 31);
                int startX = (int)(source.Next() * m_columns);
                int startY = (int)(source.Next() * m_rows);

                Tile tile = GetTile(startX, startY);

                if (tile == null || tile.IsWater || tile.ElevationMetres < 200.0)
                    continue;

                int length = FlowDownhill(tile, 200);

                if (length >= 6)
                {
                    river++;
                    m_riverCourses.Add("River " + river + " (" + length + " tiles)");
                }
            }
        }

        private int FlowDownhill(Tile tile, int maximumSteps)
        {
            int steps = 0;
            Tile current = tile;
            List<Tile> course = new List<Tile>();

            while (current != null && steps < maximumSteps)
            {
                if (current.IsWater)
                    break;

                course.Add(current);
                Tile next = null;

                List<Tile> neighbours = GetNeighbours(current);

                for (int i = 0; i < neighbours.Count; i++)
                {
                    Tile candidate = neighbours[i];

                    if (candidate == null)
                        continue;

                    if (next == null || candidate.ElevationMetres < next.ElevationMetres)
                        next = candidate;
                }

                // Stop at a local minimum: nowhere lower to go.
                if (next == null || next.ElevationMetres >= current.ElevationMetres)
                    break;

                current = next;
                steps++;
            }

            if (steps < 3)
                return 0;

            for (int i = 0; i < course.Count; i++)
            {
                course[i].HasRiver = true;
                course[i].Terrain = HabitatTerrain.River;
                course[i].Zone = ZoneType.Riparian;
            }

            return steps;
        }

        private void MarkCoastlines()
        {
            for (int y = 0; y < m_rows; y++)
            {
                for (int x = 0; x < m_columns; x++)
                {
                    Tile tile = m_tiles[x + y * m_columns];

                    if (tile.IsWater)
                    {
                        tile.Terrain = HabitatTerrain.Ocean;
                        tile.Zone = tile.ElevationMetres < -1000.0
                            ? ZoneType.Bathypelagic
                            : ZoneType.Epipelagic;
                        continue;
                    }

                    if (tile.HasRiver)
                        continue;

                    List<Tile> neighbours = GetNeighbours(tile);

                    for (int i = 0; i < neighbours.Count; i++)
                    {
                        if (neighbours[i].IsWater)
                        {
                            tile.IsCoastal = true;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Picks the biome of every tile from its temperature, its rainfall and
        /// whether it is water or land.
        /// </summary>
        private void ClassifyBiomes()
        {
            for (int y = 0; y < m_rows; y++)
            {
                for (int x = 0; x < m_columns; x++)
                {
                    Tile tile = m_tiles[x + y * m_columns];
                    tile.Biome = ClassifyTile(tile);
                }
            }
        }

        private BiomeType ClassifyTile(Tile tile)
        {
            double t = tile.TemperatureCelsius;
            double rain = tile.RainfallMm;

            if (tile.HasRiver)
                return BiomeType.Freshwater;

            if (tile.IsWater)
            {
                if (Math.Abs(tile.Latitude) > 70.0 && t < -2.0)
                    return BiomeType.IceSheet;

                if (tile.ElevationMetres > -200.0 && tile.IsCoastal)
                    return BiomeType.Ocean;

                return BiomeType.Ocean;
            }

            if (t < -5.0)
                return BiomeType.Tundra;

            if (t < 0.0)
                return rain > 400.0 ? BiomeType.Tundra : BiomeType.IceSheet;

            if (tile.ElevationMetres > 2000.0)
                return BiomeType.Mountain;

            if (Math.Abs(tile.Latitude) > 60.0)
                return rain > 350.0 ? BiomeType.BorealForest : BiomeType.Tundra;

            if (t >= 20.0)
            {
                if (rain < 250.0)
                    return BiomeType.HotDesert;

                if (rain < 1000.0)
                    return BiomeType.TropicalSavanna;

                if (rain < 1600.0)
                    return BiomeType.TropicalSeasonalForest;

                return BiomeType.TropicalRainforest;
            }

            if (t >= 10.0)
            {
                if (rain < 200.0)
                    return BiomeType.ColdDesert;

                if (rain < 400.0 && Math.Abs(tile.Latitude) < 45.0)
                    return BiomeType.Mediterranean;

                if (rain < 700.0)
                    return BiomeType.TemperateGrassland;

                if (rain < 1400.0)
                    return BiomeType.TemperateDeciduousForest;

                return BiomeType.TemperateRainforest;
            }

            if (rain < 250.0)
                return BiomeType.ColdDesert;

            if (Math.Abs(tile.Latitude) > 50.0)
                return BiomeType.BorealForest;

            return BiomeType.TemperateGrassland;
        }

        // ------------------------------------------------------------------
        // Reporting
        // ------------------------------------------------------------------

        public override string ToString()
        {
            return m_name + " (" + m_columns + "x" + m_rows + " tiles, seed " + m_seed + ")";
        }
    }

    /// <summary>
    /// A small deterministic value noise. No dependencies, and the same seed
    /// always gives the same world.
    /// </summary>
    internal sealed class Noise
    {
        private int m_seed;
        private int m_state;

        public Noise(int seed)
        {
            m_seed = seed;
            m_state = seed == 0 ? 1 : seed;
        }

        /// <summary>The next value in the stream, 0 to 1.</summary>
        public double Next()
        {
            // A cheap linear congruential generator: enough for terrain.
            m_state = unchecked(m_state * 1103515245 + 12345);

            int bits = (m_state >> 16) & 0x7fff;

            return bits / 32767.0;
        }

        /// <summary>Value noise at a point, 0 to 1.</summary>
        public double Smooth(double x, double y)
        {
            int x0 = (int)Math.Floor(x);
            int y0 = (int)Math.Floor(y);
            double fx = x - x0;
            double fy = y - y0;

            double v00 = Cell(x0, y0);
            double v10 = Cell(x0 + 1, y0);
            double v01 = Cell(x0, y0 + 1);
            double v11 = Cell(x0 + 1, y0 + 1);

            double sx = fx * fx * (3.0 - 2.0 * fx);
            double sy = fy * fy * (3.0 - 2.0 * fy);

            double top = v00 + (v10 - v00) * sx;
            double bottom = v01 + (v11 - v01) * sx;

            return top + (bottom - top) * sy;
        }

        /// <summary>Layered noise: each extra octave adds finer detail.</summary>
        public double Fractal(double x, double y, int octaves, double persistence)
        {
            double total = 0.0;
            double amplitude = 1.0;
            double frequency = 1.0;
            double maximum = 0.0;

            for (int i = 0; i < octaves; i++)
            {
                total += Smooth(x * frequency, y * frequency) * amplitude;
                maximum += amplitude;
                amplitude *= persistence;
                frequency *= 2.0;
            }

            if (maximum == 0.0)
                return 0.0;

            return total / maximum;
        }

        private double Cell(int x, int y)
        {
            // Hash the coordinate with the seed so every cell is stable.
            int hash = m_seed;

            hash = unchecked(hash * 374761393 + x * 668265263);
            hash = unchecked(hash * 1274126177 + y * 2147483647);
            hash = unchecked(hash ^ (hash >> 13));
            hash = unchecked(hash * 1274126177);

            int bits = hash & 0x7fffffff;

            return bits / 2147483647.0;
        }
    }
}
