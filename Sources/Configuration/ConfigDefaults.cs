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

namespace LifeEngine.Configuration
{
    /// <summary>
    /// Every setting the program reads, declared in one place.
    ///
    /// A setting exists because it is declared here. A file that mentions an
    /// unknown key is reported and ignored; a file that omits a key leaves that
    /// setting at the default below. That is what lets an old settings file
    /// still load after a new setting is added.
    ///
    /// The defaults here match Sources/LifeEngine.settings, which is the same
    /// list written out as a documented file.
    /// </summary>
    public static class ConfigDefaults
    {
        // Categories, so the spelling is not scattered through the file.
        public const string CategoryApplication = "Application";
        public const string CategoryWindow = "Window";
        public const string CategorySimulation = "Simulation";
        public const string CategoryAppearance = "Appearance";
        public const string CategoryWorld = "World";
        public const string CategoryEcosystem = "Ecosystem";
        public const string CategoryUpdates = "Updates";
        public const string CategoryDiagnostics = "Diagnostics";

        // Application
        public const string Name = "Application.Name";
        public const string Developer = "Application.Developer";
        public const string UpdateChannel = "Application.UpdateChannel";
        public const string CheckForUpdates = "Application.CheckForUpdates";
        public const string SaveSettingsOnExit = "Application.SaveSettingsOnExit";
        public const string SettingsFile = "Application.SettingsFile";

        // Window
        public const string WindowWidth = "Window.Width";
        public const string WindowHeight = "Window.Height";
        public const string CenterOnStart = "Window.CenterOnStart";
        public const string SidebarVisible = "Window.SidebarVisible";
        public const string SidebarWidth = "Window.SidebarWidth";

        // Simulation
        public const string GridSize = "Simulation.GridSize";
        public const string Speed = "Simulation.Speed";
        public const string Rules = "Simulation.Rules";
        public const string RandomDensity = "Simulation.RandomDensity";
        public const string AutoStart = "Simulation.AutoStart";

        // Appearance
        public const string ShowGrid = "Appearance.ShowGrid";
        public const string GridLineSize = "Appearance.GridLineSize";
        public const string LiveColor = "Appearance.LiveColor";
        public const string DeadColor = "Appearance.DeadColor";
        public const string HistoryColor = "Appearance.HistoryColor";
        public const string BackgroundColor = "Appearance.BackgroundColor";
        public const string Zoom = "Appearance.Zoom";

        // World
        public const string WorldSeed = "World.Seed";
        public const string SeaLevel = "World.SeaLevel";
        public const string MountainLine = "World.MountainLine";
        public const string CutRivers = "World.CutRivers";
        public const string TerrainDetail = "World.TerrainDetail";

        // Ecosystem
        public const string TicksPerRun = "Ecosystem.TicksPerRun";
        public const string FollowSimulation = "Ecosystem.FollowSimulation";
        public const string GrowthMultiplier = "Ecosystem.GrowthMultiplier";
        public const string DeathMultiplier = "Ecosystem.DeathMultiplier";
        public const string AllowExtinction = "Ecosystem.AllowExtinction";
        public const string Migration = "Ecosystem.Migration";

        // Updates
        public const string ManifestPath = "Updates.ManifestPath";
        public const string AllowPrerelease = "Updates.AllowPrerelease";
        public const string VerifyChecksums = "Updates.VerifyChecksums";
        public const string RestartAfterUpdate = "Updates.RestartAfterUpdate";
        public const string ApplyPatchesIncrementally = "Updates.ApplyPatchesIncrementally";

        // Diagnostics
        public const string LogLevel = "Diagnostics.LogLevel";
        public const string LogFile = "Diagnostics.LogFile";
        public const string LogUnhandledExceptions = "Diagnostics.LogUnhandledExceptions";
        public const string ProfileSimulation = "Diagnostics.ProfileSimulation";

        /// <summary>
        /// Builds a configuration holding every setting at its default.
        /// </summary>
        public static Config Create()
        {
            Config config = new Config();

            // ---- Application ---------------------------------------------

            config.AddString(Name, CategoryApplication, "LifeEngine",
                "Name shown in the title bar and the About box.");

            config.AddString(Developer, CategoryApplication, "ArkansasIo",
                "Who builds this program.");

            config.AddString(UpdateChannel, CategoryApplication, "stable",
                "The channel updates are taken from: stable or beta.");

            config.AddBool(CheckForUpdates, CategoryApplication, true,
                "Whether to look for a newer build on start-up.");

            config.AddBool(SaveSettingsOnExit, CategoryApplication, true,
                "Whether to write the settings file back when the program closes.");

            config.AddString(SettingsFile, CategoryApplication, string.Empty,
                "Path to a settings file to load instead of the default one.");

            // ---- Window ---------------------------------------------------

            config.AddInt(WindowWidth, CategoryWindow, 604, 240, 10000,
                "Starting width of the main window in pixels.");

            config.AddInt(WindowHeight, CategoryWindow, 688, 240, 10000,
                "Starting height of the main window in pixels.");

            config.AddBool(CenterOnStart, CategoryWindow, true,
                "Whether the window opens centred on the screen.");

            config.AddBool(SidebarVisible, CategoryWindow, true,
                "Whether the control sidebar is shown.");

            config.AddInt(SidebarWidth, CategoryWindow, 220, 120, 800,
                "Width of the control sidebar in pixels.");

            // ---- Simulation -----------------------------------------------

            config.AddInt(GridSize, CategorySimulation, 100, 25, 250,
                "Cells per side: 25, 50, 75, 100, 150, 200 or 250.");

            config.AddInt(Speed, CategorySimulation, 10, 1, 10,
                "Speed preset. The timer interval is (11 - Speed) * 10 milliseconds.");

            config.AddString(Rules, CategorySimulation, "23/3",
                "The ruleset, in survive/birth notation.");

            config.AddDouble(RandomDensity, CategorySimulation, 0.15, 0.01, 1.0,
                "Density used when the grid is randomised.");

            config.AddBool(AutoStart, CategorySimulation, false,
                "Whether the simulation starts as soon as the program opens.");

            // ---- Appearance -----------------------------------------------

            config.AddBool(ShowGrid, CategoryAppearance, true,
                "Whether the fine grid lines between cells are drawn.");

            config.AddDouble(GridLineSize, CategoryAppearance, 1.0, 0.0, 8.0,
                "Thickness of those lines, in pixels.");

            config.AddColor(LiveColor, CategoryAppearance, "#00bfff",
                "Colour of a live cell.");

            config.AddColor(DeadColor, CategoryAppearance, "#ffffff",
                "Colour of an empty cell.");

            config.AddColor(HistoryColor, CategoryAppearance, "#90ee90",
                "Colour of a cell that has been alive before.");

            config.AddColor(BackgroundColor, CategoryAppearance, "#ffffff",
                "Colour behind the cells.");

            config.AddInt(Zoom, CategoryAppearance, 1, 1, 5,
                "Zoom at start-up.");

            // ---- World ----------------------------------------------------

            config.AddInt(WorldSeed, CategoryWorld, 20261007, int.MinValue, int.MaxValue,
                "Seed for world generation. The same seed always builds the same planet.");

            config.AddDouble(SeaLevel, CategoryWorld, 0.0, -1.0, 1.0,
                "Fraction of the elevation range at or below which everything is water.");

            config.AddDouble(MountainLine, CategoryWorld, 2000.0, 0.0, 9000.0,
                "Height in metres at which mountains become impassable.");

            config.AddBool(CutRivers, CategoryWorld, true,
                "Whether rivers are cut from the high ground down to the sea.");

            config.AddDouble(TerrainDetail, CategoryWorld, 1.0, 0.1, 4.0,
                "Scale factor for elevation detail.");

            // ---- Ecosystem ------------------------------------------------

            config.AddInt(TicksPerRun, CategoryEcosystem, 100, 1, 100000,
                "How many ticks the ecosystem runs when the Run command is used.");

            config.AddBool(FollowSimulation, CategoryEcosystem, false,
                "Whether the ecosystem advances on every simulation step.");

            config.AddDouble(GrowthMultiplier, CategoryEcosystem, 1.0, 0.1, 12.0,
                "Multiplier on every species' growth rate.");

            config.AddDouble(DeathMultiplier, CategoryEcosystem, 1.0, 0.1, 12.0,
                "Multiplier on every species' death rate.");

            config.AddBool(AllowExtinction, CategoryEcosystem, true,
                "Whether a species with nobody left is dropped from the registry.");

            config.AddBool(Migration, CategoryEcosystem, true,
                "Whether species spread into empty neighbouring tiles.");

            // ---- Updates --------------------------------------------------

            config.AddString(ManifestPath, CategoryUpdates, @"Updates\updates.xml",
                "Path or URL of the update manifest.");

            config.AddBool(AllowPrerelease, CategoryUpdates, false,
                "Whether prerelease builds are offered.");

            config.AddBool(VerifyChecksums, CategoryUpdates, true,
                "Whether a package's SHA-256 is checked before it is applied.");

            config.AddBool(RestartAfterUpdate, CategoryUpdates, true,
                "Whether the program may restart itself after applying an update.");

            config.AddBool(ApplyPatchesIncrementally, CategoryUpdates, true,
                "Whether only the changed files in a patch are written.");

            // ---- Diagnostics ----------------------------------------------

            config.AddString(LogLevel, CategoryDiagnostics, "info",
                "Verbosity of the log: silent, error, warning, info or debug.");

            config.AddString(LogFile, CategoryDiagnostics, string.Empty,
                "Where the log is written. Empty means the settings folder.");

            config.AddBool(LogUnhandledExceptions, CategoryDiagnostics, true,
                "Whether a crash is written to the log before the program closes.");

            config.AddBool(ProfileSimulation, CategoryDiagnostics, false,
                "Whether timings are printed around each generation.");

            return config;
        }

        /// <summary>Every setting key, for a settings editor or a dump.</summary>
        public static string[] AllKeys()
        {
            return new string[]
            {
                Name, Developer, UpdateChannel, CheckForUpdates, SaveSettingsOnExit, SettingsFile,
                WindowWidth, WindowHeight, CenterOnStart, SidebarVisible, SidebarWidth,
                GridSize, Speed, Rules, RandomDensity, AutoStart,
                ShowGrid, GridLineSize, LiveColor, DeadColor, HistoryColor, BackgroundColor, Zoom,
                WorldSeed, SeaLevel, MountainLine, CutRivers, TerrainDetail,
                TicksPerRun, FollowSimulation, GrowthMultiplier, DeathMultiplier,
                AllowExtinction, Migration,
                ManifestPath, AllowPrerelease, VerifyChecksums, RestartAfterUpdate,
                ApplyPatchesIncrementally,
                LogLevel, LogFile, LogUnhandledExceptions, ProfileSimulation
            };
        }

        public static string Describe()
        {
            return "ConfigDefaults (" + AllKeys().Length + " settings)";
        }
    }
}
