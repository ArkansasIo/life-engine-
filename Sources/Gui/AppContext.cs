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
using System.Drawing;

using LifeEngine.Configuration;
using LifeEngine.Ecosystem;
using LifeEngine.Scripting;

namespace LifeEngine.Gui
{
    /// <summary>
    /// Everything the interface drives, in one place.
    ///
    /// Before this existed the form held the simulation, and the world, the
    /// wolves, the scripts, the settings and the scheduler were unreachable
    /// because nothing owned them. This class owns them all, so any panel can
    /// ask for what it needs without the form having to pass six objects around.
    ///
    /// It also owns the one thing that has to be shared rather than duplicated:
    /// the random stream. Two panels that each made their own Random would give
    /// different results from the same seed, so everything that needs a die
    /// rolls it here.
    /// </summary>
    public sealed class AppContext : IScriptHost
    {
        private Config m_config;
        private VersionInfo m_version;
        private Scheduler m_scheduler;
        private UpdateChannel m_channel;
        private ScriptManager m_scripts;
        private Ecosystem.Ecosystem m_ecosystem;
        private Biology.WolfPack m_pack;
        private Wolves.Prey m_prey;
        private List<string> m_eventLog;
        private Random m_random;
        private int m_seed;
        private bool m_dirty;

        // ------------------------------------------------------------------
        // The subsystems
        // ------------------------------------------------------------------

        /// <summary>The settings, loaded from disk at start-up.</summary>
        public Config Config
        {
            get { return m_config; }
        }

        /// <summary>Which build this is.</summary>
        public VersionInfo Version
        {
            get { return m_version; }
        }

        /// <summary>The cron-style job set.</summary>
        public Scheduler Scheduler
        {
            get { return m_scheduler; }
        }

        /// <summary>The releases this build knows about.</summary>
        public UpdateChannel Channel
        {
            get { return m_channel; }
        }

        /// <summary>The script engine and the scripts it holds.</summary>
        public ScriptManager Scripts
        {
            get { return m_scripts; }
        }

        /// <summary>The world and everything living on it, or null before one is built.</summary>
        public Ecosystem.Ecosystem Ecosystem
        {
            get { return m_ecosystem; }
        }

        /// <summary>The wolf pack, or null before one is founded.</summary>
        public Biology.WolfPack Pack
        {
            get { return m_pack; }
        }

        /// <summary>What the pack hunts.</summary>
        public Wolves.Prey Prey
        {
            get { return m_prey; }
        }

        /// <summary>A running commentary, newest last. The interface shows the tail.</summary>
        public IList<string> EventLog
        {
            get { return m_eventLog; }
        }

        /// <summary>The shared random stream.</summary>
        public Random Random
        {
            get { return m_random; }
        }

        /// <summary>The seed everything was built from.</summary>
        public int Seed
        {
            get { return m_seed; }
        }

        /// <summary>True when there are settings changes not yet written to disk.</summary>
        public bool IsDirty
        {
            get { return m_dirty; }
        }

        /// <summary>Raised after anything changes that a panel might want to redraw.</summary>
        public event EventHandler Changed;

        // ------------------------------------------------------------------
        // The simulation, which this class also serves as the script host for
        // ------------------------------------------------------------------

        private LifeGame m_game;
        private int m_generation;

        public AppContext()
        {
            m_seed = 20261007;
            m_random = new Random(m_seed);
            m_eventLog = new List<string>();
            m_generation = 0;

            m_version = VersionInfo.FromAssembly();

            m_config = ConfigDefaults.Create();

            List<string> problems = ConfigFile.Load(m_config, ConfigFile.DefaultPath("LifeEngine.settings"));

            for (int i = 0; i < problems.Count; i++)
                Log("settings: " + problems[i]);

            m_scheduler = new Scheduler("main");
            m_channel = new UpdateChannel("stable");
            m_scripts = new ScriptManager(this);

            m_scripts.Directory = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Scripts");

            m_game = new LifeGame(m_config.GetInt(ConfigDefaults.GridSize));

            Log(m_version.TitleLine() + " ready");
        }

        // ------------------------------------------------------------------
        // Simulation
        // ------------------------------------------------------------------

        /// <summary>
        /// Changes the rule the simulator is running, and says so in the log.
        /// The rules can be changed while the simulation is running, which is
        /// the whole point of having them on a menu rather than a start-up
        /// option.
        /// </summary>
        public void ApplyRules(string rule)
        {
            if (string.IsNullOrEmpty(rule))
                return;

            m_game.ChangeRules(rule);
            m_config.SetString(ConfigDefaults.Rules, rule);

            Log("rules changed to " + rule);

            Notify();
        }

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------

        /// <summary>Records a line in the event log, and tells the interface.</summary>
        public void Log(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            string line = DateTime.Now.ToString("HH:mm:ss") + "  " + message;

            m_eventLog.Add(line);

            // The log is a running commentary, not an archive.
            while (m_eventLog.Count > 500)
                m_eventLog.RemoveAt(0);
        }

        /// <summary>Raises the change notification so panels refresh.</summary>
        public void Notify()
        {
            EventHandler handler = Changed;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        // ------------------------------------------------------------------
        // Settings
        // ------------------------------------------------------------------

        /// <summary>Writes the settings out, reporting anything that went wrong.</summary>
        public bool SaveSettings()
        {
            string path = ConfigFile.DefaultPath("LifeEngine.settings");

            List<string> problems = ConfigFile.Save(m_config, path);

            for (int i = 0; i < problems.Count; i++)
                Log("settings: " + problems[i]);

            m_dirty = problems.Count == 0;

            if (m_dirty == false && problems.Count == 0)
                Log("settings written to " + path);

            return problems.Count == 0;
        }

        /// <summary>Reads the settings back in, replacing anything changed at runtime.</summary>
        public void ReloadSettings()
        {
            List<string> problems = ConfigFile.Load(m_config, ConfigFile.DefaultPath("LifeEngine.settings"));

            for (int i = 0; i < problems.Count; i++)
                Log("settings: " + problems[i]);

            m_dirty = false;
            Notify();
        }

        /// <summary>Puts every setting back to its default.</summary>
        public void ResetSettings()
        {
            m_config.ResetAll();
            m_dirty = true;
            Log("settings reset to defaults");
            Notify();
        }

        // ------------------------------------------------------------------
        // World and ecosystem
        // ------------------------------------------------------------------

        /// <summary>
        /// Builds a world sized to the grid, with the default species on it and
        /// the populations already seeded. Returns false if the size is silly.
        /// </summary>
        public bool BuildWorld(bool seedLife)
        {
            int columns = m_game.Columns;
            int rows = m_game.Rows;

            if (columns < 2 || rows < 2)
            {
                Log("the grid is too small to build a world on");
                return false;
            }

            m_seed = m_config.GetInt(ConfigDefaults.WorldSeed);

            m_ecosystem = seedLife
                ? WorldFactory.CreateDefaultWorld(columns, rows, m_seed)
                : WorldFactory.CreateEmptyWorld(columns, rows, m_seed);

            if (seedLife)
                m_ecosystem.SeedPopulations();

            Log("world built: " + m_ecosystem.World.Columns + "x" + m_ecosystem.World.Rows +
                " tiles, seed " + m_seed + ", " + m_ecosystem.Species.Count + " species");

            Notify();

            return true;
        }

        /// <summary>Runs the ecosystem forward, reporting what it did.</summary>
        public void RunEcosystem(int ticks)
        {
            if (m_ecosystem == null)
            {
                Log("there is no world yet");
                return;
            }

            if (ticks < 1)
                ticks = 1;

            int before = m_ecosystem.TotalPopulation();

            m_ecosystem.Run(ticks);

            int after = m_ecosystem.TotalPopulation();

            Log("ecosystem ran " + ticks + " ticks: " + before + " -> " + after + " head");

            Notify();
        }

        /// <summary>Empties the world of life but keeps the species registered.</summary>
        public void ClearEcosystem()
        {
            if (m_ecosystem == null)
                return;

            m_ecosystem.Clear();
            Log("ecosystem cleared");
            Notify();
        }

        /// <summary>The world's own summary, for a panel or the log.</summary>
        public string DescribeWorld()
        {
            if (m_ecosystem == null)
                return "No world has been built yet. Use World, New World.";

            return m_ecosystem.Summarise();
        }

        // ------------------------------------------------------------------
        // Wolves
        // ------------------------------------------------------------------

        /// <summary>
        /// Founds a pack on the current world, with a herd to hunt. Refuses if
        /// there is no world, because a pack with no land has nothing to do.
        /// </summary>
        public bool FoundPack(string packName, string alphaName)
        {
            if (m_ecosystem == null)
            {
                Log("build a world before founding a pack");
                return false;
            }

            m_pack = new Biology.WolfPack(
                string.IsNullOrEmpty(packName) ? "The Grey Company" : packName,
                m_ecosystem.World.Name);

            m_pack.Found(string.IsNullOrEmpty(alphaName) ? "Fenrir" : alphaName);

            // Recruit a starting pack, so the first hunt has more than one wolf.
            string[] recruits = { "Skoll", "Hati", "Varg", "Lyra", "Ulf" };

            for (int i = 0; i < recruits.Length; i++)
                m_pack.Recruit(recruits[i]);

            m_pack.SettleRanks();

            m_prey = new Wolves.Prey("Red Deer", 120.0, 55.0, 0.35, 40);
            m_prey.MaxPopulation = 120;
            m_prey.GrowthRate = 0.08;

            Log("pack founded: " + m_pack + ", alpha " + m_pack.Alpha);

            Notify();

            return true;
        }

        /// <summary>Runs one night: the herd grows, the pack hunts, ranks shift.</summary>
        public Wolves.HuntResult RunNight(bool fullMoon)
        {
            if (m_pack == null)
            {
                Log("there is no pack to run a night for");
                return null;
            }

            Wolves.HuntResult result = Wolves.WolfBehaviour.Night(m_pack, m_prey, fullMoon, m_random);

            if (result != null)
                Log(result.Narrative);

            Notify();

            return result;
        }

        /// <summary>Runs a season: ageing, breeding and dispersal.</summary>
        public void RunSeason()
        {
            if (m_pack == null)
            {
                Log("there is no pack to run a season for");
                return;
            }

            List<Biology.Wolf> left = Wolves.WolfBehaviour.Season(m_pack, 0.25, 0.5, 12, m_random);

            Log("season passed: pack now " + m_pack.Count + " wolves" +
                (left.Count > 0 ? ", " + left.Count + " dispersed" : string.Empty));

            Notify();
        }

        /// <summary>The pack's own report, for a panel.</summary>
        public string DescribePack()
        {
            if (m_pack == null)
                return "No pack has been founded yet. Use Wolves, Found Pack.";

            return m_pack.Describe();
        }

        // ------------------------------------------------------------------
        // Scripts
        // ------------------------------------------------------------------

        /// <summary>Runs the named script and reports the outcome.</summary>
        public ScriptResult RunScript(string name)
        {
            ScriptResult result = m_scripts.Run(name);

            Log(result.Summary());

            Notify();

            return result;
        }

        /// <summary>Runs source text directly, for the script console.</summary>
        public ScriptResult RunScriptSource(string source, string name)
        {
            ScriptResult result = m_scripts.RunSource(source, name);

            Log(result.Summary());

            Notify();

            return result;
        }

        // ------------------------------------------------------------------
        // Scheduler
        // ------------------------------------------------------------------

        /// <summary>
        /// Builds the default job set: the things a long-running session would
        /// want done without being asked.
        /// </summary>
        public void InstallDefaultSchedules()
        {
            m_scheduler = new Scheduler("main");

            Schedule snapshot = m_scheduler.Add("hourly snapshot",
                ScheduleAction.Snapshot, ScheduleRepeat.Interval);
            snapshot.IntervalText = "1h";
            snapshot.Description = "Write the state of the world and the pack to the log.";

            Schedule flush = m_scheduler.Add("flush log",
                ScheduleAction.FlushLog, ScheduleRepeat.Interval);
            flush.IntervalText = "15m";
            flush.Description = "Trim the event log so it does not grow without limit.";

            Schedule updates = m_scheduler.Add("check for updates",
                ScheduleAction.CheckForUpdates, ScheduleRepeat.Daily);
            updates.AddTimeOfDay("09:00");
            updates.Description = "Look for a newer build on the update channel.";

            m_scheduler.Arm(DateTime.Now);

            Log("scheduler armed with " + m_scheduler.Schedules.Count + " jobs");
            Notify();
        }

        /// <summary>
        /// Runs whatever is due. Called from the interface's own timer, so the
        /// jobs fire on the UI thread and can touch the grid safely.
        /// </summary>
        public List<Schedule> RunDueSchedules()
        {
            return m_scheduler.RunDue(DateTime.Now, ExecuteSchedule);
        }

        /// <summary>Does the work for one scheduled job.</summary>
        public void ExecuteSchedule(Schedule schedule)
        {
            if (schedule == null)
                return;

            switch (schedule.Action)
            {
                case ScheduleAction.Step:
                    Step();
                    break;

                case ScheduleAction.Start:
                    Start();
                    break;

                case ScheduleAction.Stop:
                    Stop();
                    break;

                case ScheduleAction.Clear:
                    Clear();
                    break;

                case ScheduleAction.Randomize:
                    Randomize(m_config.GetDouble(ConfigDefaults.RandomDensity));
                    break;

                case ScheduleAction.Reset:
                    Reset();
                    break;

                case ScheduleAction.RunEcosystem:
                    RunEcosystem(m_config.GetInt(ConfigDefaults.TicksPerRun));
                    break;

                case ScheduleAction.NewWorld:
                    BuildWorld(true);
                    break;

                case ScheduleAction.CheckForUpdates:
                    Log("update check: current build is " + m_version.Build +
                        ", channel " + m_channel.Name);                    break;

                case ScheduleAction.FlushLog:
                    TrimLog();
                    break;

                case ScheduleAction.Snapshot:
                    Log(Snapshot());
                    break;
            }
        }

        /// <summary>Keeps the event log from growing without bound.</summary>
        public void TrimLog()
        {
            while (m_eventLog.Count > 300)
                m_eventLog.RemoveAt(0);
        }

        /// <summary>One line describing everything that matters right now.</summary>
        public string Snapshot()
        {
            string world = m_ecosystem == null
                ? "no world"
                : m_ecosystem.TotalPopulation() + " head, tick " + m_ecosystem.Tick;

            string pack = m_pack == null
                ? "no pack"
                : m_pack.Count + " wolves";

            return "snapshot: gen " + m_generation + ", pop " + m_game.LiveCellCount +
                   ", " + world + ", " + pack;
        }

        // ------------------------------------------------------------------
        // Updates
        // ------------------------------------------------------------------

        /// <summary>
        /// Reads the update manifest and lists what it found. Returns the number
        /// of releases, or -1 if the manifest could not be read.
        /// </summary>
        public int LoadUpdateManifest()
        {
            string path = m_config.GetString(ConfigDefaults.ManifestPath);

            if (System.IO.Path.IsPathRooted(path) == false)
            {
                path = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, path);
            }

            if (!System.IO.File.Exists(path))
            {
                Log("update manifest not found at " + path);
                return -1;
            }

            try
            {
                System.Xml.XmlDocument document = new System.Xml.XmlDocument();
                document.Load(path);

                m_channel = new UpdateChannel(
                    m_config.GetString(ConfigDefaults.UpdateChannel));

                m_channel.AllowPrerelease = m_config.GetBool(ConfigDefaults.AllowPrerelease);

                System.Xml.XmlNodeList releases = document.SelectNodes("/lifeengine/releases/release");

                if (releases != null)
                {
                    for (int i = 0; i < releases.Count; i++)
                    {
                        Release release = ReadRelease(releases[i]);

                        if (release != null)
                            m_channel.Add(release);
                    }
                }

                Log("update manifest read: " + m_channel.Releases.Count + " releases on " +
                    m_channel.Name);

                Notify();

                return m_channel.Releases.Count;
            }
            catch (Exception error)
            {
                Log("update manifest could not be read: " + error.Message);
                return -1;
            }
        }

        private static Release ReadRelease(System.Xml.XmlNode node)
        {
            if (node == null || node.Attributes == null)
                return null;

            System.Xml.XmlAttribute build = node.Attributes["build"];

            if (build == null)
                return null;

            Release release = new Release(
                build.Value,
                Attribute(node, "version"),
                Attribute(node, "name"));

            release.ReleasedOn = Attribute(node, "releasedOn");
            release.Developer = ChildText(node, "developer");

            string kind = Attribute(node, "kind");

            if (string.Equals(kind, "full", StringComparison.OrdinalIgnoreCase))
                release.Kind = ReleaseKind.Full;
            else if (string.Equals(kind, "breaking", StringComparison.OrdinalIgnoreCase))
                release.Kind = ReleaseKind.Breaking;
            else
                release.Kind = ReleaseKind.Patch;

            release.Path = ChildText(node, "path");
            release.Url = ChildText(node, "url");
            release.Sha256 = ChildText(node, "sha256");
            release.Notes = ChildText(node, "notes");

            long size;

            if (long.TryParse(ChildText(node, "size"), out size))
                release.SizeBytes = size;

            // Qualified as System.Version because this class has a property
            // called Version, which would otherwise shadow the type name here.
            System.Version minimum;

            if (System.Version.TryParse(ChildText(node, "minimumVersion"), out minimum))
                release.MinimumVersion = minimum;

            System.Xml.XmlNodeList changes = node.SelectNodes("changes/change");

            if (changes != null)
            {
                for (int i = 0; i < changes.Count; i++)
                    release.AddChange(changes[i].InnerText.Trim());
            }

            return release;
        }

        private static string Attribute(System.Xml.XmlNode node, string name)
        {
            System.Xml.XmlAttribute attribute = node.Attributes == null
                ? null
                : node.Attributes[name];

            return attribute == null ? string.Empty : attribute.Value;
        }

        private static string ChildText(System.Xml.XmlNode node, string name)
        {
            System.Xml.XmlNode child = node.SelectSingleNode(name);

            return child == null ? string.Empty : child.InnerText.Trim();
        }

        /// <summary>What the updater would do, as text.</summary>
        public string DescribeUpdates()
        {
            if (m_channel.Releases.Count == 0)
                return "No releases loaded. Use Updates, Load Manifest.";

            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.AppendLine("Installed build: " + m_version.Build);
            builder.AppendLine("Channel:         " + m_channel.Name);
            builder.AppendLine();

            Release newer = m_channel.GetNewer(m_version.Build);

            if (newer == null)
            {
                builder.AppendLine("This build is current.");
                builder.AppendLine();
            }
            else
            {
                builder.AppendLine("A newer build is available: " + newer.Build +
                                   " (" + newer.Name + ")");
                builder.AppendLine();
            }

            List<Release> pending = m_channel.GetPending(m_version.Build);

            builder.AppendLine(PatchPlanner.DescribePlan(PatchPlanner.PlanApplyOrder(pending)));

            builder.AppendLine();
            builder.AppendLine("All releases on this channel");

            for (int i = 0; i < m_channel.Releases.Count; i++)
            {
                Release release = m_channel.Releases[i];

                builder.AppendLine("  " + release.Build + "  " + release.Version + "  " +
                                   release.Kind + "  " + release.SizeText() + "  " + release.Name);
            }

            return builder.ToString();
        }

        // ------------------------------------------------------------------
        // IScriptHost: what a script is allowed to reach
        // ------------------------------------------------------------------

        public int GridSize
        {
            get { return m_game.Columns; }
        }

        public int Generation
        {
            get { return m_generation; }
        }

        public int Population
        {
            get { return m_game.LiveCellCount; }
        }

        public string Rules
        {
            get { return m_game.Rules; }
        }

        public bool IsRunning
        {
            get { return m_isRunning; }
        }

        private bool m_isRunning;

        public bool GetCell(int x, int y)
        {
            return m_game.GetCellState(x, y);
        }

        public void SetCell(int x, int y, bool alive)
        {
            bool current = m_game.GetCellState(x, y);

            if (current != alive)
                m_game.ToggleCellState(x, y);
        }

        public void ToggleCell(int x, int y)
        {
            m_game.ToggleCellState(x, y);
        }

        public void Clear()
        {
            m_game.ClearGrid();
            m_generation = 0;
            Notify();
        }

        public void Randomize(double density)
        {
            m_game.Randomize(density);
            m_generation = 0;
            Notify();
        }

        public void Step()
        {
            m_game.Step();
            m_generation++;
            Notify();
        }

        public void Start()
        {
            m_game.SetStart();
            m_isRunning = true;
            Notify();
        }

        public void Stop()
        {
            m_isRunning = false;
            Notify();
        }

        public void Reset()
        {
            m_game.ResetGrid();
            m_generation = 0;
            m_isRunning = false;
            Notify();
        }

        public void SetRules(string rules)
        {
            if (string.IsNullOrEmpty(rules))
                return;

            m_game.ChangeRules(rules);
            Notify();
        }

        public void SetGridSize(int size)
        {
            if (size < 2 || size > 4000)
                return;

            m_game = new LifeGame(size);
            m_generation = 0;
            m_isRunning = false;

            Log("grid resized to " + size + " a side");

            Notify();
        }

        /// <summary>The grid model itself, for the renderer.</summary>
        public LifeGame Game
        {
            get { return m_game; }
        }

        /// <summary>Rebuilds the grid from a size in the settings.</summary>
        public void ApplyGridSizeFromSettings()
        {
            SetGridSize(m_config.GetInt(ConfigDefaults.GridSize));
        }

        /// <summary>The interval the simulation timer should use, in milliseconds.</summary>
        public int TimerInterval()
        {
            int speed = m_config.GetInt(ConfigDefaults.Speed);

            return (11 - speed) * 10;
        }

        /// <summary>The three cell colours, read from the settings.</summary>
        public Color LiveColor
        {
            get { return m_config.GetColor(ConfigDefaults.LiveColor); }
        }

        public Color DeadColor
        {
            get { return m_config.GetColor(ConfigDefaults.DeadColor); }
        }

        public Color HistoryColor
        {
            get { return m_config.GetColor(ConfigDefaults.HistoryColor); }
        }

        public override string ToString()
        {
            return "AppContext (" + m_version.TitleLine() + ")";
        }
    }
}
