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

namespace LifeEngine.Data
{
    /// <summary>
    /// Builds the tables that go into a spreadsheet, straight from the live
    /// objects.
    ///
    /// It reads the program's own models rather than querying a database, so an
    /// export works whether or not a database is configured, and shows the state
    /// of the world as it is right now rather than as it was last written.
    /// </summary>
    public static class ExportBuilder
    {
        /// <summary>
        /// Every table that can be exported, in the order a workbook should
        /// present them: what this build is, then the grid, then the world, then
        /// the wolves.
        /// </summary>
        public static List<DataTable> BuildAll(Gui.AppContext context)
        {
            List<DataTable> tables = new List<DataTable>();

            if (context == null)
                return tables;

            tables.Add(BuildVersion(context));
            tables.Add(BuildGridSummary(context));
            tables.Add(BuildGeneration(context));

            if (context.Ecosystem != null)
            {
                tables.Add(BuildWorld(context));
                tables.Add(BuildBiomeCensus(context));
                tables.Add(BuildSpecies(context));
                tables.Add(BuildTileSample(context));
            }

            if (context.Pack != null)
            {
                tables.Add(BuildPack(context));
                tables.Add(BuildWolves(context));
            }

            tables.Add(BuildSettings(context));
            tables.Add(BuildSchedules(context));
            tables.Add(BuildReleases(context));
            tables.Add(BuildEvents(context));

            return tables;
        }

        // ------------------------------------------------------------------
        // The build itself
        // ------------------------------------------------------------------

        public static DataTable BuildVersion(Gui.AppContext context)
        {
            DataTable table = new DataTable("Version");

            table.WithColumns("Item", "Value");

            table.Add("Product", context.Version.Product);
            table.Add("Version", context.Version.VersionText);
            table.Add("Build", context.Version.Build);
            table.Add("Built", context.Version.BuildDate);
            table.Add("Company", context.Version.Company);
            table.Add("Copyright", context.Version.Copyright);
            table.Add("Exported", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            table.Add("World seed", context.Seed);

            return table;
        }

        // ------------------------------------------------------------------
        // The grid
        // ------------------------------------------------------------------

        public static DataTable BuildGridSummary(Gui.AppContext context)
        {
            DataTable table = new DataTable("Grid");

            table.WithColumns("Measure", "Value");

            LifeGame game = context.Game;

            table.Add("Columns", game.Columns);
            table.Add("Rows", game.Rows);
            table.Add("Cells", game.Cells);
            table.Add("Population", game.LiveCellCount);
            table.Add("Generation", context.Generation);
            table.Add("Rules", game.Rules);
            table.Add("Speed", context.Config.GetInt(Configuration.ConfigDefaults.Speed));
            table.Add("Running", context.IsRunning);
            table.Add("Density", game.Cells == 0 ? 0.0 : (double)game.LiveCellCount / game.Cells);

            return table;
        }

        /// <summary>Every live cell on the board, with its coordinates.</summary>
        public static DataTable BuildGeneration(Gui.AppContext context)
        {
            DataTable table = new DataTable("Live cells");

            table.WithColumns("x", "y", "alive", "ever_alive");

            LifeGame game = context.Game;

            bool[] cells = game.GameGrid;
            bool[] history = game.HistoryGrid;

            for (int y = 0; y < game.Rows; y++)
            {
                for (int x = 0; x < game.Columns; x++)
                {
                    int index = x + y * game.Columns;

                    if (index >= cells.Length)
                        continue;

                    bool alive = cells[index];

                    if (!alive)
                        continue;

                    table.Add(x, y, alive,
                        index < history.Length ? history[index] : false);
                }
            }

            return table;
        }

        // ------------------------------------------------------------------
        // The world
        // ------------------------------------------------------------------

        public static DataTable BuildWorld(Gui.AppContext context)
        {
            DataTable table = new DataTable("World");

            table.WithColumns("Measure", "Value");

            Biology.WorldMap world = context.Ecosystem.World;

            table.Add("Name", world.Name);
            table.Add("Seed", world.Seed);
            table.Add("Columns", world.Columns);
            table.Add("Rows", world.Rows);
            table.Add("Tiles", world.TileCount);
            table.Add("Sea level (m)", world.SeaLevelMetres);
            table.Add("Axial tilt (deg)", world.AxialTiltDegrees);
            table.Add("Day length (h)", world.DayLengthHours);
            table.Add("Gravity (m/s2)", world.GravityMs2);
            table.Add("Land fraction", world.LandFraction());
            table.Add("Mean temperature (C)", world.AverageTemperatureCelsius());
            table.Add("Mean elevation (m)", world.AverageElevationMetres());
            table.Add("Rivers", world.RiverCourses.Count);
            table.Add("Ecosystem ticks", context.Ecosystem.Tick);
            table.Add("Total head", context.Ecosystem.TotalPopulation());
            table.Add("Species", context.Ecosystem.Species.Count);

            return table;
        }

        /// <summary>How many tiles of each biome, with the mean climate of each.</summary>
        public static DataTable BuildBiomeCensus(Gui.AppContext context)
        {
            DataTable table = new DataTable("Biomes");

            table.WithColumns("Biome", "Tiles", "Percent", "Mean temp (C)",
                              "Mean rainfall (mm)", "Mean elevation (m)");

            Biology.WorldMap world = context.Ecosystem.World;

            Dictionary<Biology.BiomeType, int> census = world.BiomeCensus();

            List<Biology.BiomeType> keys = new List<Biology.BiomeType>(census.Keys);
            keys.Sort();

            double total = world.TileCount;

            for (int i = 0; i < keys.Count; i++)
            {
                Biology.BiomeType biome = keys[i];
                int tiles = census[biome];

                double temperature = 0.0;
                double rainfall = 0.0;
                double elevation = 0.0;

                foreach (Biology.Tile tile in world.Tiles)
                {
                    if (tile.Biome != biome)
                        continue;

                    temperature += tile.TemperatureCelsius;
                    rainfall += tile.RainfallMm;
                    elevation += tile.ElevationMetres;
                }

                if (tiles > 0)
                {
                    temperature /= tiles;
                    rainfall /= tiles;
                    elevation /= tiles;
                }

                table.Add(biome.ToString(), tiles,
                    total == 0.0 ? 0.0 : tiles / total * 100.0,
                    temperature, rainfall, elevation);
            }

            return table;
        }

        /// <summary>Every species, its habitat envelope, and how it is doing.</summary>
        public static DataTable BuildSpecies(Gui.AppContext context)
        {
            DataTable table = new DataTable("Species");

            table.WithColumns("Name", "Kingdom", "Diet", "Locomotion", "Level", "Symbol",
                              "Head", "Tiles", "Extinct",
                              "Min temp", "Max temp", "Min rainfall", "Max rainfall",
                              "Max elevation", "Growth", "Death", "Carrying capacity");

            Ecosystem.Ecosystem ecosystem = context.Ecosystem;
            Biology.WorldMap world = ecosystem.World;

            for (int i = 0; i < ecosystem.Species.Count; i++)
            {
                Ecosystem.Species species = ecosystem.Species[i];

                int head = ecosystem.GetPopulation(species);
                int tiles = 0;

                foreach (Biology.Tile tile in world.Tiles)
                {
                    if (tile.GetPopulation(species) > 0)
                        tiles++;
                }

                table.Add(species.Name,
                    species.Kingdom.ToString(),
                    species.Diet.ToString(),
                    species.Locomotion.ToString(),
                    species.TrophicLevel.ToString(),
                    species.Symbol.ToString(),
                    head,
                    tiles,
                    head <= 0,
                    species.MinTemperatureCelsius,
                    species.MaxTemperatureCelsius,
                    species.MinRainfallMm,
                    species.MaxRainfallMm,
                    species.MaxElevationMetres,
                    species.GrowthRate,
                    species.DeathRate,
                    species.MaxPopulation);
            }

            return table;
        }

        /// <summary>
        /// The tiles, with their climate and whatever is living on them. A
        /// 250x250 world is 62,500 rows, which is more than Excel enjoys, so the
        /// caller gets a sample when the world is large.
        /// </summary>
        public static DataTable BuildTileSample(Gui.AppContext context)
        {
            DataTable table = new DataTable("Tiles");

            table.WithColumns("x", "y", "Biome", "Climate zone", "Terrain", "Zone",
                              "Temp (C)", "Rainfall (mm)", "Elevation (m)",
                              "River", "Coastal", "Water", "Species", "Head");

            Biology.WorldMap world = context.Ecosystem.World;

            // One row in four for a large world, so the sheet stays openable.
            int step = world.TileCount > 20000 ? 2 : 1;

            for (int y = 0; y < world.Rows; y += step)
            {
                for (int x = 0; x < world.Columns; x += step)
                {
                    Biology.Tile tile = world.GetTile(x, y);

                    if (tile == null)
                        continue;

                    string speciesNames = string.Empty;

                    for (int i = 0; i < tile.Organisms.Count; i++)
                    {
                        Ecosystem.Species species = tile.Organisms[i] as Ecosystem.Species;

                        if (species == null)
                            continue;

                        if (speciesNames.Length > 0)
                            speciesNames += ", ";

                        speciesNames += species.Name;
                    }

                    table.Add(x, y,
                        tile.Biome.ToString(),
                        tile.ClimateZone.ToString(),
                        tile.Terrain.ToString(),
                        tile.Zone.ToString(),
                        tile.TemperatureCelsius,
                        tile.RainfallMm,
                        tile.ElevationMetres,
                        tile.HasRiver,
                        tile.IsCoastal,
                        tile.IsWater,
                        speciesNames,
                        tile.TotalPopulation());
                }
            }

            return table;
        }

        // ------------------------------------------------------------------
        // The wolves
        // ------------------------------------------------------------------

        public static DataTable BuildPack(Gui.AppContext context)
        {
            DataTable table = new DataTable("Pack");

            table.WithColumns("Measure", "Value");

            Biology.WolfPack pack = context.Pack;

            table.Add("Name", pack.Name);
            table.Add("Territory", pack.Territory);
            table.Add("Members", pack.Count);
            table.Add("Alpha", pack.Alpha == null ? "none" : pack.Alpha.Name);
            table.Add("Alpha mate", pack.AlphaMate == null ? "none" : pack.AlphaMate.Name);
            table.Add("Cohesion", pack.Cohesion);
            table.Add("Alpha changes", pack.GenerationNumber);
            table.Add("Dead", pack.Dead.Count);

            int kills = 0;

            for (int i = 0; i < pack.Members.Count; i++)
                kills += pack.Members[i].Kills;

            table.Add("Total kills", kills);

            if (context.Prey != null)
            {
                table.Add("Prey", context.Prey.Name);
                table.Add("Prey herd", context.Prey.Population);
                table.Add("Prey mass (kg)", context.Prey.MassKilograms);
                table.Add("Prey speed (kph)", context.Prey.SpeedKph);
                table.Add("Prey defence", context.Prey.Defence);
            }

            return table;
        }

        /// <summary>Every wolf, with its traits and its record. Strongest first.</summary>
        public static DataTable BuildWolves(Gui.AppContext context)
        {
            DataTable table = new DataTable("Wolves");

            table.WithColumns("Rank", "Name", "Age (y)", "Mass (kg)", "Health", "Energy",
                              "Alive", "Cub", "Elder", "Strength", "Speed", "Aggression",
                              "Hearing", "Scent", "Night sight", "Healing",
                              "Human retention", "Cursed", "Shapeshifter",
                              "Scars", "Kills", "Wins", "Losses", "Dominance");

            Biology.WolfPack pack = context.Pack;

            List<Biology.Wolf> all = new List<Biology.Wolf>(pack.Members);
            all.AddRange(pack.Dead);

            all.Sort(delegate (Biology.Wolf left, Biology.Wolf right)
            {
                return right.FightingPower().CompareTo(left.FightingPower());
            });

            for (int i = 0; i < all.Count; i++)
            {
                Biology.Wolf wolf = all[i];

                table.Add(wolf.Rank.ToString(),
                    wolf.Name,
                    wolf.AgeYears,
                    wolf.MassKilograms,
                    wolf.Health,
                    wolf.Energy,
                    wolf.IsAlive,
                    wolf.IsCub(),
                    wolf.IsElder(),
                    wolf.Traits.Strength,
                    wolf.Traits.Speed,
                    wolf.Traits.Aggression,
                    wolf.Traits.Hearing,
                    wolf.Traits.Scent,
                    wolf.Traits.NightSight,
                    wolf.Traits.Healing,
                    wolf.Traits.HumanRetention,
                    wolf.Traits.IsCursed,
                    wolf.Traits.IsShapeshifter,
                    wolf.Traits.ScarCount,
                    wolf.Kills,
                    wolf.DominanceWins,
                    wolf.DominanceLosses,
                    wolf.FightingPower());
            }

            return table;
        }

        // ------------------------------------------------------------------
        // Configuration and machinery
        // ------------------------------------------------------------------

        public static DataTable BuildSettings(Gui.AppContext context)
        {
            DataTable table = new DataTable("Settings");

            table.WithColumns("Key", "Value", "Default", "Type", "Category",
                              "Modified", "Description");

            IList<Configuration.ConfigEntry> entries = context.Config.Entries;

            for (int i = 0; i < entries.Count; i++)
            {
                Configuration.ConfigEntry entry = entries[i];

                table.Add(entry.Key, entry.Value, entry.DefaultValue, entry.TypeName,
                          entry.Category, entry.IsModified(), entry.Description);
            }

            return table;
        }

        public static DataTable BuildSchedules(Gui.AppContext context)
        {
            DataTable table = new DataTable("Schedules");

            table.WithColumns("Name", "Action", "Repeat", "Interval", "Enabled",
                              "Runs", "Max runs", "Last run", "Next run", "Description");

            IList<Configuration.Schedule> schedules = context.Scheduler.Schedules;

            for (int i = 0; i < schedules.Count; i++)
            {
                Configuration.Schedule schedule = schedules[i];

                table.Add(schedule.Name,
                    schedule.Action.ToString(),
                    schedule.Repeat.ToString(),
                    schedule.IntervalText,
                    schedule.Enabled,
                    schedule.Runs,
                    schedule.MaxRuns,
                    schedule.LastRun == DateTime.MinValue ? "never" : schedule.LastRun.ToString("yyyy-MM-dd HH:mm:ss"),
                    schedule.NextRun == DateTime.MaxValue ? "never" : schedule.NextRun.ToString("yyyy-MM-dd HH:mm:ss"),
                    schedule.Description);
            }

            return table;
        }

        public static DataTable BuildReleases(Gui.AppContext context)
        {
            DataTable table = new DataTable("Releases");

            table.WithColumns("Build", "Version", "Name", "Kind", "Released",
                              "Size (bytes)", "Minimum version", "Changes", "Notes");

            IList<Configuration.Release> releases = context.Channel.Releases;

            for (int i = 0; i < releases.Count; i++)
            {
                Configuration.Release release = releases[i];

                System.Text.StringBuilder changes = new System.Text.StringBuilder();

                for (int c = 0; c < release.Changes.Count; c++)
                {
                    if (c > 0)
                        changes.Append("; ");

                    changes.Append(release.Changes[c]);
                }

                table.Add(release.Build,
                    release.Version,
                    release.Name,
                    release.Kind.ToString(),
                    release.ReleasedOn,
                    release.SizeBytes,
                    release.MinimumVersion == null ? string.Empty : release.MinimumVersion.ToString(),
                    changes.ToString(),
                    release.Notes);
            }

            return table;
        }

        public static DataTable BuildEvents(Gui.AppContext context)
        {
            DataTable table = new DataTable("Events");

            table.WithColumns("Line");

            IList<string> events = context.EventLog;

            for (int i = 0; i < events.Count; i++)
                table.Add(events[i]);

            return table;
        }

        /// <summary>
        /// Every problem found by every write to a database, for a report.
        /// </summary>
        public static DataTable BuildProblems(Database database)
        {
            DataTable table = new DataTable("Problems");

            table.WithColumns("Problem");

            if (database == null)
                return table;

            for (int i = 0; i < database.Problems.Count; i++)
                table.Add(database.Problems[i]);

            return table;
        }

        /// <summary>A description of the builder, for a log line.</summary>
        public static string Describe()
        {
            return "ExportBuilder";
        }
    }
}
