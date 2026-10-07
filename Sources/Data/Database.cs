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
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;

namespace LifeEngine.Data
{
    /// <summary>
    /// Where a <see cref="Database"/> gets its connection.
    ///
    /// The store talks to whatever this hands it, so the same code can drive a
    /// file-backed database, an in-memory one for a test, or a server. Nothing
    /// above this interface knows which.
    /// </summary>
    public interface IDatabaseProvider
    {
        /// <summary>A name for the log, e.g. the file path.</summary>
        string Description { get; }

        /// <summary>Opens a connection. The caller disposes it.</summary>
        DbConnection Open();

        /// <summary>True if the store can be used at all.</summary>
        bool IsAvailable { get; }
    }

    /// <summary>
    /// A provider backed by an Access database file, through Jet. This is the
    /// one that works on a stock Windows machine with no extra downloads: Jet is
    /// part of the operating system, or of the Access runtime if Office is
    /// installed.
    ///
    /// The .accdb format and the 64-bit build need the ACE provider instead,
    /// which is a separate download; the file extension chooses which provider
    /// string is used.
    /// </summary>
    public sealed class AccessDatabaseProvider : IDatabaseProvider
    {
        private string m_path;
        private string m_connectionString;
        private bool m_available;
        private string m_problem;

        public string Description
        {
            get { return m_path; }
        }

        /// <summary>Why the provider is unusable, if it is.</summary>
        public string Problem
        {
            get { return m_problem; }
        }

        public bool IsAvailable
        {
            get { return m_available; }
        }

        public AccessDatabaseProvider(string path)
        {
            m_path = path;
            m_available = false;
            m_problem = string.Empty;

            if (string.IsNullOrEmpty(path))
            {
                m_problem = "No database path was given.";
                return;
            }

            string directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception error)
                {
                    m_problem = "Could not create " + directory + ": " + error.Message;
                    return;
                }
            }

            bool accdb = string.Equals(Path.GetExtension(path), ".accdb",
                                       StringComparison.OrdinalIgnoreCase);

            string provider = accdb ? "Microsoft.ACE.OLEDB.12.0" : "Microsoft.Jet.OLEDB.4.0";
            string properties = accdb ? "Excel 12.0;Database=" : string.Empty;

            if (accdb)
            {
                m_connectionString =
                    "Provider=" + provider + ";" +
                    "Data Source=" + path + ";";
            }
            else
            {
                m_connectionString =
                    "Provider=" + provider + ";" +
                    "Data Source=" + path + ";";
            }

            // Creating an Access file from nothing needs the ADOX catalogue,
            // which the .NET Framework exposes through COM. Rather than take a
            // dependency on that, the file is expected to exist; if it does not,
            // it is created empty and the schema is applied on top.
            if (!File.Exists(path))
            {
                try
                {
                    File.WriteAllBytes(path, new byte[0]);
                }
                catch (Exception error)
                {
                    m_problem = "Could not create " + path + ": " + error.Message;
                    return;
                }
            }

            try
            {
                using (DbConnection test = Open())
                    m_available = true;
            }
            catch (Exception error)
            {
                m_problem = "Could not open " + path + ": " + error.Message;
            }
        }

        public DbConnection Open()
        {
            OleDbConnection connection = new OleDbConnection(m_connectionString);

            connection.Open();

            return connection;
        }
    }

    /// <summary>
    /// A store that keeps everything in memory. It exists so the export code can
    /// be tested and demonstrated without a database file on disk, and so a
    /// machine with no Jet provider still gets a working Excel export.
    /// </summary>
    public sealed class MemoryDatabaseProvider : IDatabaseProvider
    {
        private DataSet m_data;

        public string Description
        {
            get { return "in memory"; }
        }

        public bool IsAvailable
        {
            get { return true; }
        }

        /// <summary>The tables, as a plain data set.</summary>
        public DataSet Data
        {
            get { return m_data; }
        }

        public MemoryDatabaseProvider()
        {
            m_data = new DataSet("LifeEngine");
        }

        public DbConnection Open()
        {
            // There is no connection to open: the store reads and writes the
            // data set directly. A provider that cannot be used is worse than
            // one that says so, so this throws rather than returning null.
            throw new NotSupportedException(
                "The in-memory provider has no connection. Use the Data set directly.");
        }
    }

    /// <summary>
    /// The store: it knows the shape of every table and turns the program's
    /// objects into rows.
    ///
    /// It deliberately holds no connection open. Each write opens one, does its
    /// work and closes it, because the interface drives this from the UI thread
    /// and a long-lived connection would block the file from being copied or
    /// opened in another program.
    /// </summary>
    public sealed class Database
    {
        private IDatabaseProvider m_provider;
        private List<string> m_problems;
        private int m_runId;

        /// <summary>Anything that went wrong, newest last.</summary>
        public IList<string> Problems
        {
            get { return m_problems; }
        }

        /// <summary>Whether the store can be written to.</summary>
        public bool IsAvailable
        {
            get { return m_provider != null && m_provider.IsAvailable; }
        }

        /// <summary>Where the store is.</summary>
        public string Description
        {
            get { return m_provider == null ? "none" : m_provider.Description; }
        }

        /// <summary>The id of the run currently being written, or 0.</summary>
        public int RunId
        {
            get { return m_runId; }
        }

        public Database(IDatabaseProvider provider)
        {
            m_provider = provider;
            m_problems = new List<string>();
            m_runId = 0;
        }

        /// <summary>
        /// Builds the tables if they are not there. Every statement is guarded,
        /// so calling it on an existing database is harmless.
        /// </summary>
        public bool CreateSchema()
        {
            if (!IsAvailable)
            {
                m_problems.Add("The database is not available.");
                return false;
            }

            string[] statements = SchemaStatements();

            int made = 0;

            for (int i = 0; i < statements.Length; i++)
            {
                try
                {
                    using (DbConnection connection = m_provider.Open())
                    using (DbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = statements[i];
                        command.ExecuteNonQuery();

                        made++;
                    }
                }
                catch (Exception error)
                {
                    // A table that already exists is not a failure worth
                    // reporting; anything else is.
                    if (error.Message.IndexOf("already exists",
                        StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        m_problems.Add("Schema: " + error.Message);
                    }
                }
            }

            return made > 0;
        }

        private static string[] SchemaStatements()
        {
            return new string[]
            {
                "CREATE TABLE [run] (" +
                "[run_id] COUNTER PRIMARY KEY, [started_at] TEXT(32), [finished_at] TEXT(32), " +
                "[build] TEXT(32), [version] TEXT(32), [product] TEXT(64), [developer] TEXT(64), " +
                "[seed] LONG, [rules] TEXT(32), [grid_size] LONG, [speed] LONG, [note] MEMO)",

                "CREATE TABLE [generation] (" +
                "[generation_id] COUNTER PRIMARY KEY, [run_id] LONG, [number] LONG, " +
                "[population] LONG, [density] DOUBLE, [born] LONG, [died] LONG, " +
                "[elapsed_ms] DOUBLE, [recorded_at] TEXT(32))",

                "CREATE TABLE [cell] (" +
                "[generation_id] LONG, [x] LONG, [y] LONG, [alive] BIT, [ever_alive] BIT)",

                "CREATE TABLE [world] (" +
                "[world_id] COUNTER PRIMARY KEY, [run_id] LONG, [name] TEXT(128), " +
                "[seed] LONG, [columns] LONG, [rows] LONG, [sea_level] DOUBLE, " +
                "[axial_tilt] DOUBLE, [day_length] DOUBLE, [gravity] DOUBLE, " +
                "[land_fraction] DOUBLE, [mean_temp] DOUBLE, [mean_elevation] DOUBLE, " +
                "[river_count] LONG, [created_at] TEXT(32))",

                "CREATE TABLE [tile] (" +
                "[world_id] LONG, [x] LONG, [y] LONG, [latitude] DOUBLE, [longitude] DOUBLE, " +
                "[biome] TEXT(48), [climate_zone] TEXT(32), [terrain] TEXT(32), " +
                "[zone] TEXT(32), [temperature] DOUBLE, [rainfall] DOUBLE, " +
                "[elevation] DOUBLE, [has_river] BIT, [is_coastal] BIT, [is_water] BIT)",

                "CREATE TABLE [species] (" +
                "[species_id] COUNTER PRIMARY KEY, [world_id] LONG, [name] TEXT(128), " +
                "[kingdom] TEXT(32), [diet] TEXT(32), [locomotion] TEXT(32), " +
                "[trophic_level] TEXT(32), [symbol] TEXT(4), [colour_key] TEXT(32), " +
                "[min_temp] DOUBLE, [max_temp] DOUBLE, [min_rainfall] DOUBLE, " +
                "[max_rainfall] DOUBLE, [max_elevation] DOUBLE, [needs_water] BIT, " +
                "[needs_land] BIT, [needs_coast] BIT, [avoids_desert] BIT, [avoids_ice] BIT, " +
                "[growth_rate] DOUBLE, [death_rate] DOUBLE, [max_population] LONG, " +
                "[seed_population] LONG)",

                "CREATE TABLE [population] (" +
                "[world_id] LONG, [tick] LONG, [species_id] LONG, [x] LONG, [y] LONG, [head] LONG)",

                "CREATE TABLE [wolf_pack] (" +
                "[pack_id] COUNTER PRIMARY KEY, [run_id] LONG, [name] TEXT(128), " +
                "[territory] TEXT(128), [founded_at] TEXT(32), [alpha_name] TEXT(64), " +
                "[cohesion] DOUBLE, [alpha_changes] LONG)",

                "CREATE TABLE [wolf] (" +
                "[wolf_id] COUNTER PRIMARY KEY, [pack_id] LONG, [name] TEXT(64), " +
                "[rank] TEXT(16), [age_years] DOUBLE, [mass_kg] DOUBLE, [health] DOUBLE, " +
                "[energy] DOUBLE, [is_alive] BIT, [is_cub] BIT, [is_elder] BIT, " +
                "[strength] DOUBLE, [speed] DOUBLE, [aggression] DOUBLE, [hearing] DOUBLE, " +
                "[scent] DOUBLE, [night_sight] DOUBLE, [healing] DOUBLE, " +
                "[human_retention] DOUBLE, [is_cursed] BIT, [is_shapeshifter] BIT, " +
                "[scar_count] LONG, [kills] LONG, [dominance_wins] LONG, " +
                "[dominance_losses] LONG, [mother_id] LONG, [father_id] LONG)",

                "CREATE TABLE [hunt] (" +
                "[hunt_id] COUNTER PRIMARY KEY, [pack_id] LONG, [occurred_at] TEXT(32), " +
                "[is_night] BIT, [is_full_moon] BIT, [prey_name] TEXT(128), [succeeded] BIT, " +
                "[taken] LONG, [meat_kg] DOUBLE, [hunter_count] LONG, [wounded_count] LONG, " +
                "[narrative] MEMO)",

                "CREATE TABLE [setting] (" +
                "[setting_id] COUNTER PRIMARY KEY, [run_id] LONG, [key] TEXT(128), " +
                "[value] TEXT(255), [default_value] TEXT(255), [type_name] TEXT(32), " +
                "[category] TEXT(64), [description] MEMO, [is_modified] BIT)",

                "CREATE TABLE [schedule] (" +
                "[schedule_id] COUNTER PRIMARY KEY, [name] TEXT(128), [action] TEXT(32), " +
                "[repeat_mode] TEXT(32), [interval_text] TEXT(32), [enabled] BIT, " +
                "[runs] LONG, [max_runs] LONG, [last_run] TEXT(32), [next_run] TEXT(32), " +
                "[description] MEMO)",

                "CREATE TABLE [release] (" +
                "[release_id] COUNTER PRIMARY KEY, [build] TEXT(32), [version] TEXT(32), " +
                "[name] TEXT(128), [developer] TEXT(64), [released_on] TEXT(32), " +
                "[kind] TEXT(16), [minimum_version] TEXT(32), [path] TEXT(255), " +
                "[url] TEXT(255), [sha256] TEXT(80), [size_bytes] DOUBLE, [notes] MEMO)",

                "CREATE TABLE [script_run] (" +
                "[script_run_id] COUNTER PRIMARY KEY, [run_id] LONG, [name] TEXT(128), " +
                "[succeeded] BIT, [error] MEMO, [output] MEMO, [steps] LONG, " +
                "[elapsed_ms] DOUBLE, [ran_at] TEXT(32))",

                "CREATE TABLE [event] (" +
                "[event_id] COUNTER PRIMARY KEY, [run_id] LONG, [logged_at] TEXT(32), " +
                "[message] MEMO)"
            };
        }

        // ------------------------------------------------------------------
        // Writing
        // ------------------------------------------------------------------

        /// <summary>
        /// Records the start of a session and returns its id. Everything else is
        /// keyed on it.
        /// </summary>
        public int BeginRun(string build, string version, string product,
                            string developer, int seed, string rules,
                            int gridSize, int speed)
        {
            if (!IsAvailable)
                return 0;

            try
            {
                using (DbConnection connection = m_provider.Open())
                {
                    using (DbCommand command = connection.CreateCommand())
                    {
                        command.CommandText =
                            "INSERT INTO [run] (started_at, build, version, product, developer, " +
                            "seed, rules, grid_size, speed) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";

                        AddParameter(command, Stamp());
                        AddParameter(command, build);
                        AddParameter(command, version);
                        AddParameter(command, product);
                        AddParameter(command, developer);
                        AddParameter(command, seed);
                        AddParameter(command, rules);
                        AddParameter(command, gridSize);
                        AddParameter(command, speed);

                        command.ExecuteNonQuery();
                    }

                    m_runId = LastIdentity(connection, "SELECT MAX(run_id) FROM [run]");
                }

                return m_runId;
            }
            catch (Exception error)
            {
                m_problems.Add("BeginRun: " + error.Message);
                return 0;
            }
        }

        /// <summary>Closes off a run.</summary>
        public void EndRun(int runId)
        {
            if (!IsAvailable || runId <= 0)
                return;

            try
            {
                using (DbConnection connection = m_provider.Open())
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE [run] SET finished_at = ? WHERE run_id = ?";

                    AddParameter(command, Stamp());
                    AddParameter(command, runId);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception error)
            {
                m_problems.Add("EndRun: " + error.Message);
            }
        }

        /// <summary>Writes one generation, with its live cells.</summary>
        public void WriteGeneration(int runId, int number, bool[] cells,
                                    int columns, int rows, int population,
                                    int born, int died, double elapsedMs)
        {
            if (!IsAvailable || runId <= 0 || cells == null)
                return;

            try
            {
                using (DbConnection connection = m_provider.Open())
                {
                    int generationId;

                    using (DbCommand command = connection.CreateCommand())
                    {
                        double density = cells.Length == 0 ? 0.0 : (double)population / cells.Length;

                        command.CommandText =
                            "INSERT INTO [generation] (run_id, number, population, density, " +
                            "born, died, elapsed_ms, recorded_at) VALUES (?, ?, ?, ?, ?, ?, ?, ?)";

                        AddParameter(command, runId);
                        AddParameter(command, number);
                        AddParameter(command, population);
                        AddParameter(command, density);
                        AddParameter(command, born);
                        AddParameter(command, died);
                        AddParameter(command, elapsedMs);
                        AddParameter(command, Stamp());

                        command.ExecuteNonQuery();
                    }

                    generationId = LastIdentity(connection, "SELECT MAX(generation_id) FROM [generation]");

                    // Only live cells are written: an empty board is no rows,
                    // not 62,500 rows of dead ones.
                    using (DbTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbCommand command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;

                            command.CommandText =
                                "INSERT INTO [cell] (generation_id, x, y, alive, ever_alive) " +
                                "VALUES (?, ?, ?, ?, ?)";

                            for (int y = 0; y < rows; y++)
                            {
                                for (int x = 0; x < columns; x++)
                                {
                                    int index = x + y * columns;

                                    if (index >= cells.Length || !cells[index])
                                        continue;

                                    command.Parameters.Clear();

                                    AddParameter(command, generationId);
                                    AddParameter(command, x);
                                    AddParameter(command, y);
                                    AddParameter(command, 1);
                                    AddParameter(command, 1);

                                    command.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (Exception error)
            {
                m_problems.Add("WriteGeneration: " + error.Message);
            }
        }

        /// <summary>Writes a world, its tiles and its species. Returns the world id.</summary>
        public int WriteWorld(int runId, Biology.WorldMap world, Ecosystem.Ecosystem ecosystem)
        {
            if (!IsAvailable || world == null)
                return 0;

            int worldId = 0;

            try
            {
                using (DbConnection connection = m_provider.Open())
                {
                    using (DbCommand command = connection.CreateCommand())
                    {
                        command.CommandText =
                            "INSERT INTO [world] (run_id, name, seed, columns, rows, sea_level, " +
                            "axial_tilt, day_length, gravity, land_fraction, mean_temp, " +
                            "mean_elevation, river_count, created_at) " +
                            "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                        AddParameter(command, runId);
                        AddParameter(command, world.Name);
                        AddParameter(command, world.Seed);
                        AddParameter(command, world.Columns);
                        AddParameter(command, world.Rows);
                        AddParameter(command, world.SeaLevelMetres);
                        AddParameter(command, world.AxialTiltDegrees);
                        AddParameter(command, world.DayLengthHours);
                        AddParameter(command, world.GravityMs2);
                        AddParameter(command, world.LandFraction());
                        AddParameter(command, world.AverageTemperatureCelsius());
                        AddParameter(command, world.AverageElevationMetres());
                        AddParameter(command, world.RiverCourses.Count);
                        AddParameter(command, Stamp());

                        command.ExecuteNonQuery();
                    }

                    worldId = LastIdentity(connection, "SELECT MAX(world_id) FROM [world]");

                    WriteTiles(connection, worldId, world);
                }
            }
            catch (Exception error)
            {
                m_problems.Add("WriteWorld: " + error.Message);
            }

            return worldId;
        }

        private static void WriteTiles(DbConnection connection, int worldId, Biology.WorldMap world)
        {
            using (DbTransaction transaction = connection.BeginTransaction())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;

                    command.CommandText =
                        "INSERT INTO [tile] (world_id, x, y, latitude, longitude, biome, " +
                        "climate_zone, terrain, zone, temperature, rainfall, elevation, " +
                        "has_river, is_coastal, is_water) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                    for (int y = 0; y < world.Rows; y++)
                    {
                        for (int x = 0; x < world.Columns; x++)
                        {
                            Biology.Tile tile = world.GetTile(x, y);

                            if (tile == null)
                                continue;

                            command.Parameters.Clear();

                            AddParameter(command, worldId);
                            AddParameter(command, x);
                            AddParameter(command, y);
                            AddParameter(command, tile.Latitude);
                            AddParameter(command, tile.Longitude);
                            AddParameter(command, tile.Biome.ToString());
                            AddParameter(command, tile.ClimateZone.ToString());
                            AddParameter(command, tile.Terrain.ToString());
                            AddParameter(command, tile.Zone.ToString());
                            AddParameter(command, tile.TemperatureCelsius);
                            AddParameter(command, tile.RainfallMm);
                            AddParameter(command, tile.ElevationMetres);
                            AddParameter(command, tile.HasRiver ? 1 : 0);
                            AddParameter(command, tile.IsCoastal ? 1 : 0);
                            AddParameter(command, tile.IsWater ? 1 : 0);

                            command.ExecuteNonQuery();
                        }
                    }
                }

                transaction.Commit();
            }
        }

        /// <summary>Writes the species registry for a world.</summary>
        public void WriteSpecies(int worldId, Ecosystem.Ecosystem ecosystem)
        {
            if (!IsAvailable || worldId <= 0 || ecosystem == null)
                return;

            try
            {
                using (DbConnection connection = m_provider.Open())
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "INSERT INTO [species] (world_id, name, kingdom, diet, locomotion, " +
                        "trophic_level, symbol, colour_key, min_temp, max_temp, min_rainfall, " +
                        "max_rainfall, max_elevation, needs_water, needs_land, needs_coast, " +
                        "avoids_desert, avoids_ice, growth_rate, death_rate, max_population, " +
                        "seed_population) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                    for (int i = 0; i < ecosystem.Species.Count; i++)
                    {
                        Ecosystem.Species species = ecosystem.Species[i];

                        command.Parameters.Clear();

                        AddParameter(command, worldId);
                        AddParameter(command, species.Name);
                        AddParameter(command, species.Kingdom.ToString());
                        AddParameter(command, species.Diet.ToString());
                        AddParameter(command, species.Locomotion.ToString());
                        AddParameter(command, species.TrophicLevel.ToString());
                        AddParameter(command, species.Symbol.ToString());
                        AddParameter(command, species.ColourKey);
                        AddParameter(command, species.MinTemperatureCelsius);
                        AddParameter(command, species.MaxTemperatureCelsius);
                        AddParameter(command, species.MinRainfallMm);
                        AddParameter(command, species.MaxRainfallMm);
                        AddParameter(command, species.MaxElevationMetres);
                        AddParameter(command, species.NeedsWaterTile ? 1 : 0);
                        AddParameter(command, species.NeedsLandTile ? 1 : 0);
                        AddParameter(command, species.NeedsCoast ? 1 : 0);
                        AddParameter(command, species.AvoidsDesert ? 1 : 0);
                        AddParameter(command, species.AvoidsIce ? 1 : 0);
                        AddParameter(command, species.GrowthRate);
                        AddParameter(command, species.DeathRate);
                        AddParameter(command, species.MaxPopulation);
                        AddParameter(command, species.SeedPopulation);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception error)
            {
                m_problems.Add("WriteSpecies: " + error.Message);
            }
        }

        /// <summary>Writes a pack and every wolf in it.</summary>
        public int WritePack(int runId, Biology.WolfPack pack)
        {
            if (!IsAvailable || pack == null)
                return 0;

            int packId = 0;

            try
            {
                using (DbConnection connection = m_provider.Open())
                {
                    using (DbCommand command = connection.CreateCommand())
                    {
                        command.CommandText =
                            "INSERT INTO [wolf_pack] (run_id, name, territory, founded_at, " +
                            "alpha_name, cohesion, alpha_changes) VALUES (?, ?, ?, ?, ?, ?, ?)";

                        AddParameter(command, runId);
                        AddParameter(command, pack.Name);
                        AddParameter(command, pack.Territory);
                        AddParameter(command, Stamp());
                        AddParameter(command, pack.Alpha == null ? string.Empty : pack.Alpha.Name);
                        AddParameter(command, pack.Cohesion);
                        AddParameter(command, pack.GenerationNumber);

                        command.ExecuteNonQuery();
                    }

                    packId = LastIdentity(connection, "SELECT MAX(pack_id) FROM [wolf_pack]");

                    WriteWolves(connection, packId, pack);
                }
            }
            catch (Exception error)
            {
                m_problems.Add("WritePack: " + error.Message);
            }

            return packId;
        }

        private static void WriteWolves(DbConnection connection, int packId, Biology.WolfPack pack)
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "INSERT INTO [wolf] (pack_id, name, rank, age_years, mass_kg, health, energy, " +
                    "is_alive, is_cub, is_elder, strength, speed, aggression, hearing, scent, " +
                    "night_sight, healing, human_retention, is_cursed, is_shapeshifter, " +
                    "scar_count, kills, dominance_wins, dominance_losses) " +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                for (int i = 0; i < pack.Members.Count; i++)
                    InsertWolf(command, packId, pack.Members[i]);

                // The dead are written too: a pack's history is the point.
                for (int i = 0; i < pack.Dead.Count; i++)
                    InsertWolf(command, packId, pack.Dead[i]);
            }
        }

        private static void InsertWolf(DbCommand command, int packId, Biology.Wolf wolf)
        {
            command.Parameters.Clear();

            AddParameter(command, packId);
            AddParameter(command, wolf.Name);
            AddParameter(command, wolf.Rank.ToString());
            AddParameter(command, wolf.AgeYears);
            AddParameter(command, wolf.MassKilograms);
            AddParameter(command, wolf.Health);
            AddParameter(command, wolf.Energy);
            AddParameter(command, wolf.IsAlive ? 1 : 0);
            AddParameter(command, wolf.IsCub() ? 1 : 0);
            AddParameter(command, wolf.IsElder() ? 1 : 0);
            AddParameter(command, wolf.Traits.Strength);
            AddParameter(command, wolf.Traits.Speed);
            AddParameter(command, wolf.Traits.Aggression);
            AddParameter(command, wolf.Traits.Hearing);
            AddParameter(command, wolf.Traits.Scent);
            AddParameter(command, wolf.Traits.NightSight);
            AddParameter(command, wolf.Traits.Healing);
            AddParameter(command, wolf.Traits.HumanRetention);
            AddParameter(command, wolf.Traits.IsCursed ? 1 : 0);
            AddParameter(command, wolf.Traits.IsShapeshifter ? 1 : 0);
            AddParameter(command, wolf.Traits.ScarCount);
            AddParameter(command, wolf.Kills);
            AddParameter(command, wolf.DominanceWins);
            AddParameter(command, wolf.DominanceLosses);

            command.ExecuteNonQuery();
        }

        /// <summary>Writes one hunt.</summary>
        public void WriteHunt(int packId, WolfLogic.HuntResult result, bool fullMoon)
        {
            if (!IsAvailable || packId <= 0 || result == null)
                return;

            try
            {
                using (DbConnection connection = m_provider.Open())
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "INSERT INTO [hunt] (pack_id, occurred_at, is_night, is_full_moon, " +
                        "prey_name, succeeded, taken, meat_kg, hunter_count, wounded_count, " +
                        "narrative) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                    AddParameter(command, packId);
                    AddParameter(command, Stamp());
                    AddParameter(command, 1);
                    AddParameter(command, fullMoon ? 1 : 0);
                    AddParameter(command, result.PreyName);
                    AddParameter(command, result.Success ? 1 : 0);
                    AddParameter(command, result.Taken);
                    AddParameter(command, result.MeatKilograms);
                    AddParameter(command, result.Hunters.Count);
                    AddParameter(command, result.Wounded.Count);
                    AddParameter(command, result.Narrative);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception error)
            {
                m_problems.Add("WriteHunt: " + error.Message);
            }
        }

        /// <summary>Writes every setting, so a run can be reproduced exactly.</summary>
        public void WriteSettings(int runId, Configuration.Config config)
        {
            if (!IsAvailable || config == null)
                return;

            try
            {
                using (DbConnection connection = m_provider.Open())
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "INSERT INTO [setting] (run_id, key, value, default_value, type_name, " +
                        "category, description, is_modified) VALUES (?, ?, ?, ?, ?, ?, ?, ?)";

                    for (int i = 0; i < config.Entries.Count; i++)
                    {
                        Configuration.ConfigEntry entry = config.Entries[i];

                        command.Parameters.Clear();

                        AddParameter(command, runId);
                        AddParameter(command, entry.Key);
                        AddParameter(command, entry.Value);
                        AddParameter(command, entry.DefaultValue);
                        AddParameter(command, entry.TypeName);
                        AddParameter(command, entry.Category);
                        AddParameter(command, entry.Description);
                        AddParameter(command, entry.IsModified() ? 1 : 0);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception error)
            {
                m_problems.Add("WriteSettings: " + error.Message);
            }
        }

        /// <summary>Writes one line of the event log.</summary>
        public void WriteEvent(int runId, string message)
        {
            if (!IsAvailable || string.IsNullOrEmpty(message))
                return;

            try
            {
                using (DbConnection connection = m_provider.Open())
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "INSERT INTO [event] (run_id, logged_at, message) VALUES (?, ?, ?)";

                    AddParameter(command, runId);
                    AddParameter(command, Stamp());
                    AddParameter(command, message);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception error)
            {
                m_problems.Add("WriteEvent: " + error.Message);
            }
        }

        // ------------------------------------------------------------------
        // Reading
        // ------------------------------------------------------------------

        /// <summary>Runs a query and returns the rows as a table.</summary>
        public DataTable Query(string sql, string tableName)
        {
            DataTable table = new DataTable(tableName ?? "query");

            if (!IsAvailable)
            {
                m_problems.Add("The database is not available.");
                return table;
            }

            try
            {
                using (DbConnection connection = m_provider.Open())
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = sql;

                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                            table.Columns.Add(reader.GetName(i));

                        while (reader.Read())
                        {
                            string[] row = new string[reader.FieldCount];

                            for (int i = 0; i < reader.FieldCount; i++)
                                row[i] = reader.IsDBNull(i) ? string.Empty : reader.GetValue(i).ToString();

                            table.Rows.Add(row);
                        }
                    }
                }
            }
            catch (Exception error)
            {
                m_problems.Add("Query: " + error.Message);
            }

            return table;
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static string Stamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private static void AddParameter(DbCommand command, object value)
        {
            DbParameter parameter = command.CreateParameter();

            parameter.Value = value == null ? DBNull.Value : value;

            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// The id of the row just inserted. Access has no RETURNING clause and
        /// Jet's @@IDENTITY is per-connection, so the maximum is read back on
        /// the same connection that did the insert.
        /// </summary>
        private static int LastIdentity(DbConnection connection, string sql)
        {
            try
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = sql;

                    object value = command.ExecuteScalar();

                    if (value == null || value == DBNull.Value)
                        return 0;

                    return Convert.ToInt32(value);
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public override string ToString()
        {
            return "Database at " + Description + (IsAvailable ? " (available)" : " (unavailable)");
        }
    }
}
