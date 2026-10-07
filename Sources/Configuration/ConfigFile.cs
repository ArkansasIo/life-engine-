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
using System.Globalization;
using System.IO;
using System.Text;

namespace LifeEngine.Configuration
{
    /// <summary>
    /// Reads and writes <see cref="Config"/> files.
    ///
    /// The format is a simple ini:
    ///
    ///   # a comment
    ///   [Simulation]
    ///   Speed = 10
    ///   ShowGrid = true
    ///   LiveColor = #00bfff
    ///
    /// Anything the file does not mention keeps its default, and anything in the
    /// file that the program does not know about is ignored and reported, so an
    /// old file still loads and a typo is visible rather than silent.
    /// </summary>
    public static class ConfigFile
    {
        /// <summary>The header written at the top of a file the program saves.</summary>
        public const string Header =
            "# LifeEngine settings\n" +
            "# Lines look like  Key = Value  under a  [Category]  heading.\n" +
            "# An entry whose key is unknown here is ignored; a missing entry keeps\n" +
            "# the value the program was built with.\n";

        /// <summary>
        /// Raised for every line that could not be applied, so a caller can
        /// report what was ignored. Null means ignore silently.
        /// </summary>
        public static List<string> Load(Config config, string path)
        {
            List<string> problems = new List<string>();

            if (config == null || string.IsNullOrEmpty(path))
                return problems;

            if (!File.Exists(path))
                return problems;

            string[] lines;

            try
            {
                lines = File.ReadAllLines(path);
            }
            catch (Exception error)
            {
                problems.Add("Could not read " + path + ": " + error.Message);
                return problems;
            }

            string category = string.Empty;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                int lineNumber = i + 1;

                if (line.Length == 0 || line.StartsWith("#") || line.StartsWith(";"))
                    continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    category = line.Substring(1, line.Length - 2).Trim();
                    continue;
                }

                int equals = line.IndexOf('=');

                if (equals < 0)
                {
                    problems.Add("Line " + lineNumber + ": no '=' in \"" + line + "\"");
                    continue;
                }

                string key = line.Substring(0, equals).Trim();
                string value = line.Substring(equals + 1).Trim();

                if (key.Length == 0)
                {
                    problems.Add("Line " + lineNumber + ": empty key");
                    continue;
                }

                // Accept both "Simulation.Speed" and a bare "Speed" under a
                // [Simulation] heading.
                ConfigEntry entry = Resolve(config, key, category);

                if (entry == null)
                {
                    problems.Add("Line " + lineNumber + ": unknown setting \"" + key + "\"");
                    continue;
                }

                if (!entry.SetFromText(value))
                {
                    problems.Add("Line " + lineNumber + ": \"" + value +
                                 "\" is not a valid " + entry.TypeName + " for " + entry.Key);
                }
            }

            return problems;
        }

        /// <summary>Loads a file, or returns defaults if it does not exist.</summary>
        public static Config LoadOrDefault(string path, out List<string> problems)
        {
            Config config = ConfigDefaults.Create();

            problems = Load(config, path);

            return config;
        }

        public static List<string> Save(Config config, string path)
        {
            List<string> problems = new List<string>();

            if (config == null || string.IsNullOrEmpty(path))
            {
                problems.Add("Nothing to save.");
                return problems;
            }

            try
            {
                string directory = Path.GetDirectoryName(path);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(path, Serialise(config));
            }
            catch (Exception error)
            {
                problems.Add("Could not write " + path + ": " + error.Message);
            }

            return problems;
        }

        /// <summary>Renders the configuration as file text.</summary>
        public static string Serialise(Config config)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(Header);
            builder.AppendLine();

            List<string> categories = config.GetCategories();

            for (int c = 0; c < categories.Count; c++)
            {
                builder.AppendLine("[" + categories[c] + "]");

                List<ConfigEntry> entries = config.GetByCategory(categories[c]);

                for (int e = 0; e < entries.Count; e++)
                {
                    ConfigEntry entry = entries[e];

                    if (!string.IsNullOrEmpty(entry.Description))
                        builder.AppendLine("# " + entry.Description);

                    builder.AppendLine(entry.Key + " = " + entry.Value);
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static ConfigEntry Resolve(Config config, string key, string category)
        {
            ConfigEntry entry = config.Get(key);

            if (entry != null)
                return entry;

            // "Simulation.Speed" also answers to a bare "Speed" in the file.
            int dot = key.IndexOf('.');

            if (dot >= 0)
            {
                string bare = key.Substring(dot + 1);
                entry = config.Get(bare);

                if (entry != null)
                    return entry;
            }

            if (!string.IsNullOrEmpty(category))
            {
                entry = config.Get(category + "." + key);

                if (entry != null)
                    return entry;
            }

            return null;
        }

        /// <summary>The file next to the executable, if it is writable, otherwise
        /// under the user's application data.</summary>
        public static string DefaultPath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = "LifeEngine.settings";

            try
            {
                string beside = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                // A file we can write next to the program beats one buried in
                // AppData, but a program in Program Files cannot be written to.
                string directory = Path.GetDirectoryName(beside);

                if (!string.IsNullOrEmpty(directory))
                {
                    string probe = Path.Combine(directory, ".write-test");

                    try
                    {
                        File.WriteAllText(probe, string.Empty);
                        File.Delete(probe);
                        return beside;
                    }
                    catch (Exception)
                    {
                        // Fall through to the roaming profile.
                    }
                }
            }
            catch (Exception)
            {
                // Fall through to the roaming profile.
            }

            string applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            return Path.Combine(Path.Combine(applicationData, "LifeEngine"), fileName);
        }
    }
}
