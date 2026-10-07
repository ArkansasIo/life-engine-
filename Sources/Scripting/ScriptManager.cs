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
using System.IO;

namespace LifeEngine.Scripting
{
    /// <summary>What happened when a script was run.</summary>
    public sealed class ScriptResult
    {
        private bool m_succeeded;
        private string m_error;
        private string m_output;
        private int m_steps;
        private double m_milliseconds;
        private string m_sourceName;

        /// <summary>True if the script ran to the end without throwing.</summary>
        public bool Succeeded
        {
            get { return m_succeeded; }
        }

        /// <summary>What went wrong, if anything.</summary>
        public string Error
        {
            get { return m_error; }
        }

        /// <summary>Everything the script printed.</summary>
        public string Output
        {
            get { return m_output; }
        }

        /// <summary>How many statements it executed.</summary>
        public int Steps
        {
            get { return m_steps; }
        }

        /// <summary>How long it took.</summary>
        public double Milliseconds
        {
            get { return m_milliseconds; }
        }

        public string SourceName
        {
            get { return m_sourceName; }
        }

        internal void SetResult(bool succeeded, string error, string output,
                                int steps, double milliseconds, string sourceName)
        {
            m_succeeded = succeeded;
            m_error = error;
            m_output = output;
            m_steps = steps;
            m_milliseconds = milliseconds;
            m_sourceName = sourceName;
        }

        /// <summary>A one-line summary, for a status bar or a log.</summary>
        public string Summary()
        {
            if (m_succeeded)
                return m_sourceName + ": ran " + m_steps + " steps in " +
                       Math.Round(m_milliseconds, 1) + " ms";

            return m_sourceName + ": " + m_error;
        }

        public override string ToString()
        {
            return Summary();
        }
    }

    /// <summary>
    /// Holds the scripts the program knows about and runs them.
    ///
    /// A manager owns one interpreter, so every script shares the same globals
    /// and the same class table: a class declared in one file is visible in the
    /// next, which is what makes a small script library possible.
    /// </summary>
    public sealed class ScriptManager
    {
        private Interpreter m_interpreter;
        private IScriptHost m_host;
        private Dictionary<string, string> m_scripts;
        private List<string> m_order;
        private List<ScriptResult> m_history;
        private string m_directory;

        /// <summary>The interpreter every script runs in.</summary>
        public Interpreter Interpreter
        {
            get { return m_interpreter; }
        }

        /// <summary>The program the scripts can reach.</summary>
        public IScriptHost Host
        {
            get { return m_host; }
            set
            {
                m_host = value;

                if (value != null)
                    LifeEngineBinding.Install(m_interpreter, value);
            }
        }

        /// <summary>The names of the loaded scripts, in load order.</summary>
        public IList<string> Names
        {
            get { return m_order; }
        }

        /// <summary>The last few results, newest last.</summary>
        public IList<ScriptResult> History
        {
            get { return m_history; }
        }

        /// <summary>Where scripts are loaded from and saved to.</summary>
        public string Directory
        {
            get { return m_directory; }
            set { m_directory = value; }
        }

        public ScriptManager(IScriptHost host)
        {
            m_interpreter = new Interpreter();
            m_scripts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            m_order = new List<string>();
            m_history = new List<ScriptResult>();
            m_directory = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Scripts");

            Host = host;
        }

        // ------------------------------------------------------------------
        // Loading
        // ------------------------------------------------------------------

        /// <summary>Adds or replaces a script in memory.</summary>
        public void Load(string name, string source)
        {
            if (string.IsNullOrEmpty(name))
                return;

            if (!m_scripts.ContainsKey(name))
                m_order.Add(name);

            m_scripts[name] = source ?? string.Empty;
        }

        /// <summary>Loads a script file, naming it after the file.</summary>
        public bool LoadFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return false;

            try
            {
                Load(Path.GetFileName(path), File.ReadAllText(path));
                return true;
            }
            catch (Exception error)
            {
                m_host.Log("Could not read " + path + ": " + error.Message);
                return false;
            }
        }

        /// <summary>Loads every .lua file in the script directory.</summary>
        public int LoadDirectory()
        {
            int loaded = 0;

            if (!System.IO.Directory.Exists(m_directory))
                return 0;

            string[] files = System.IO.Directory.GetFiles(m_directory, "*.lua");

            Array.Sort(files, StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < files.Length; i++)
            {
                if (LoadFile(files[i]))
                    loaded++;
            }

            return loaded;
        }

        public string GetSource(string name)
        {
            string source;

            if (name != null && m_scripts.TryGetValue(name, out source))
                return source;

            return null;
        }

        public bool Remove(string name)
        {
            if (name == null || !m_scripts.ContainsKey(name))
                return false;

            m_scripts.Remove(name);
            m_order.Remove(name);

            return true;
        }

        // ------------------------------------------------------------------
        // Running
        // ------------------------------------------------------------------

        /// <summary>Runs a named script.</summary>
        public ScriptResult Run(string name)
        {
            string source = GetSource(name);

            if (source == null)
            {
                ScriptResult missing = new ScriptResult();

                missing.SetResult(false, "There is no script called '" + name + "'",
                                  string.Empty, 0, 0.0, name ?? "?");

                Remember(missing);

                return missing;
            }

            return RunSource(source, name);
        }

        /// <summary>Runs source text directly, without loading it first.</summary>
        public ScriptResult RunSource(string source, string name)
        {
            ScriptResult result = new ScriptResult();

            DateTime started = DateTime.Now;

            m_interpreter.ClearOutput();

            string error = string.Empty;
            bool succeeded;

            try
            {
                m_interpreter.Run(source, name);
                succeeded = true;
            }
            catch (ScriptError scriptError)
            {
                error = scriptError.Message;
                succeeded = false;
            }
            catch (Exception other)
            {
                error = other.GetType().Name + ": " + other.Message;
                succeeded = false;
            }

            double milliseconds = (DateTime.Now - started).TotalMilliseconds;

            result.SetResult(succeeded, error, m_interpreter.TakeOutput(),
                             m_interpreter.Steps, milliseconds, name);

            Remember(result);

            return result;
        }

        /// <summary>Runs every loaded script, in load order, stopping at the first failure.</summary>
        public List<ScriptResult> RunAll()
        {
            List<ScriptResult> results = new List<ScriptResult>();

            for (int i = 0; i < m_order.Count; i++)
            {
                ScriptResult result = Run(m_order[i]);

                results.Add(result);

                if (!result.Succeeded)
                    break;
            }

            return results;
        }

        /// <summary>Calls a global function defined by the scripts.</summary>
        public Value Call(string functionName, params Value[] arguments)
        {
            return m_interpreter.Call(functionName, arguments);
        }

        /// <summary>Writes a script out to the script directory.</summary>
        public bool Save(string name, string source)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            try
            {
                if (!System.IO.Directory.Exists(m_directory))
                    System.IO.Directory.CreateDirectory(m_directory);

                string path = Path.Combine(m_directory, name);

                File.WriteAllText(path, source ?? string.Empty);

                Load(name, source);

                return true;
            }
            catch (Exception error)
            {
                m_host.Log("Could not write " + name + ": " + error.Message);
                return false;
            }
        }

        private void Remember(ScriptResult result)
        {
            m_history.Add(result);

            // Keep the history short: it is for the last few runs, not an archive.
            while (m_history.Count > 50)
                m_history.RemoveAt(0);
        }

        /// <summary>A table of the loaded scripts, for a script to inspect.</summary>
        public Value Describe()
        {
            Value table = Value.Table();

            for (int i = 0; i < m_order.Count; i++)
            {
                Value entry = Value.Table();

                entry.SetField("name", Value.Of(m_order[i]));
                entry.SetField("length", Value.Of((double)(GetSource(m_order[i]) ?? string.Empty).Length));

                table.Append(entry);
            }

            return table;
        }

        public override string ToString()
        {
            return "ScriptManager (" + m_order.Count + " scripts)";
        }
    }
}
