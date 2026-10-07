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

namespace LifeEngine.Scripting
{
    /// <summary>
    /// The interface the scripting layer uses to reach the program.
    ///
    /// The interpreter never touches a grid or a form directly: it goes through
    /// this, so the language can be tested with a stub host, and a script cannot
    /// reach anything the host does not expose.
    /// </summary>
    public interface IScriptHost
    {
        /// <summary>Cells per side of the current grid.</summary>
        int GridSize { get; }

        /// <summary>The current generation number.</summary>
        int Generation { get; }

        /// <summary>How many cells are alive.</summary>
        int Population { get; }

        /// <summary>The active ruleset, in survive/birth form.</summary>
        string Rules { get; }

        /// <summary>Whether the simulation is running.</summary>
        bool IsRunning { get; }

        /// <summary>Reads one cell. Out of range reads false.</summary>
        bool GetCell(int x, int y);

        /// <summary>Sets one cell. Out of range does nothing.</summary>
        void SetCell(int x, int y, bool alive);

        /// <summary>Flips one cell.</summary>
        void ToggleCell(int x, int y);

        /// <summary>Empties the grid.</summary>
        void Clear();

        /// <summary>Fills the grid at the given density, 0 to 1.</summary>
        void Randomize(double density);

        /// <summary>Advances one generation.</summary>
        void Step();

        /// <summary>Starts the simulation.</summary>
        void Start();

        /// <summary>Pauses the simulation.</summary>
        void Stop();

        /// <summary>Restores the state captured at the last Start.</summary>
        void Reset();

        /// <summary>Changes the ruleset, in survive/birth form.</summary>
        void SetRules(string rules);

        /// <summary>Resizes the grid, discarding the board.</summary>
        void SetGridSize(int size);

        /// <summary>Writes a line to whatever the host logs to.</summary>
        void Log(string message);
    }

    /// <summary>
    /// A host that does nothing, for testing a script without a program around
    /// it. It remembers the cells it was given, so a test can assert on them.
    /// </summary>
    public sealed class HeadlessScriptHost : IScriptHost
    {
        private int m_gridSize;
        private bool[] m_cells;
        private int m_generation;
        private string m_rules;
        private bool m_running;
        private List<string> m_log;

        public int GridSize
        {
            get { return m_gridSize; }
        }

        public int Generation
        {
            get { return m_generation; }
        }

        public int Population
        {
            get
            {
                int alive = 0;

                for (int i = 0; i < m_cells.Length; i++)
                {
                    if (m_cells[i])
                        alive++;
                }

                return alive;
            }
        }

        public string Rules
        {
            get { return m_rules; }
        }

        public bool IsRunning
        {
            get { return m_running; }
        }

        /// <summary>Everything the script logged.</summary>
        public IList<string> Messages
        {
            get { return m_log; }
        }

        public HeadlessScriptHost(int gridSize)
        {
            m_gridSize = gridSize < 2 ? 2 : gridSize;
            m_cells = new bool[m_gridSize * m_gridSize];
            m_generation = 0;
            m_rules = "23/3";
            m_running = false;
            m_log = new List<string>();
        }

        public bool GetCell(int x, int y)
        {
            if (x < 0 || x >= m_gridSize || y < 0 || y >= m_gridSize)
                return false;

            return m_cells[x + y * m_gridSize];
        }

        public void SetCell(int x, int y, bool alive)
        {
            if (x < 0 || x >= m_gridSize || y < 0 || y >= m_gridSize)
                return;

            m_cells[x + y * m_gridSize] = alive;
        }

        public void ToggleCell(int x, int y)
        {
            if (x < 0 || x >= m_gridSize || y < 0 || y >= m_gridSize)
                return;

            m_cells[x + y * m_gridSize] = !m_cells[x + y * m_gridSize];
        }

        public void Clear()
        {
            for (int i = 0; i < m_cells.Length; i++)
                m_cells[i] = false;

            m_generation = 0;
            m_running = false;
        }

        public void Randomize(double density)
        {
            Random random = new Random(20261007);

            for (int i = 0; i < m_cells.Length; i++)
                m_cells[i] = random.NextDouble() <= density;
        }

        public void Step()
        {
            m_generation++;
        }

        public void Start()
        {
            m_running = true;
        }

        public void Stop()
        {
            m_running = false;
        }

        public void Reset()
        {
            m_generation = 0;
            m_running = false;
        }

        public void SetRules(string rules)
        {
            if (!string.IsNullOrEmpty(rules))
                m_rules = rules;
        }

        public void SetGridSize(int size)
        {
            if (size < 2)
                return;

            m_gridSize = size;
            m_cells = new bool[m_gridSize * m_gridSize];
            m_generation = 0;
        }

        public void Log(string message)
        {
            m_log.Add(message ?? string.Empty);
        }
    }

    /// <summary>
    /// Installs the LifeEngine functions into an interpreter: grid, sim, rules
    /// and log. This is the whole surface a script gets.
    /// </summary>
    public static class LifeEngineBinding
    {
        /// <summary>
        /// Registers the host functions. The script reaches them by name, e.g.
        /// sim.step(), grid.get(x, y), rules.set("23/3").
        /// </summary>
        public static void Install(Interpreter interpreter, IScriptHost host)
        {
            if (interpreter == null || host == null)
                return;

            Scope globals = interpreter.Globals;

            globals.Declare("gridsize", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                return Value.Of((double)host.GridSize);
            }));

            globals.Declare("generation", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                return Value.Of((double)host.Generation);
            }));

            globals.Declare("population", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                return Value.Of((double)host.Population);
            }));

            globals.Declare("log", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                for (int n = 0; n < a.Count; n++)
                    host.Log(a[n].AsText());

                return Value.Nil();
            }));

            globals.Declare("grid", BuildGridTable(host));
            globals.Declare("sim", BuildSimTable(host));
            globals.Declare("rules", BuildRulesTable(host));
            globals.Declare("state", BuildStateTable(host));
        }

        private static Value BuildGridTable(IScriptHost host)
        {
            Value table = Value.Table();

            table.SetField("get", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                int x = (int)Argument(a, 0).AsNumber();
                int y = (int)Argument(a, 1).AsNumber();

                return Value.Of(host.GetCell(x, y));
            }));

            table.SetField("set", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                int x = (int)Argument(a, 0).AsNumber();
                int y = (int)Argument(a, 1).AsNumber();

                host.SetCell(x, y, Argument(a, 2).IsTruthy());

                return Value.Nil();
            }));

            table.SetField("toggle", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                int x = (int)Argument(a, 0).AsNumber();
                int y = (int)Argument(a, 1).AsNumber();

                host.ToggleCell(x, y);

                return Value.Nil();
            }));

            table.SetField("clear", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                host.Clear();
                return Value.Nil();
            }));

            table.SetField("randomize", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                double density = a.Count > 0 ? a[0].AsNumber() : 0.15;

                host.Randomize(density);

                return Value.Nil();
            }));

            table.SetField("size", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                return Value.Of((double)host.GridSize);
            }));

            table.SetField("resize", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                host.SetGridSize((int)Argument(a, 0).AsNumber());
                return Value.Nil();
            }));

            // Fills a rectangle, so a script can draw a block or a glider.
            table.SetField("fill", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                int x = (int)Argument(a, 0).AsNumber();
                int y = (int)Argument(a, 1).AsNumber();
                int width = (int)Argument(a, 2).AsNumber();
                int height = (int)Argument(a, 3).AsNumber();
                bool alive = a.Count > 4 ? a[4].IsTruthy() : true;

                for (int dy = 0; dy < height; dy++)
                {
                    for (int dx = 0; dx < width; dx++)
                        host.SetCell(x + dx, y + dy, alive);
                }

                return Value.Nil();
            }));

            // Counts the live neighbours of a cell, the same eight the engine uses.
            table.SetField("neighbours", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                int x = (int)Argument(a, 0).AsNumber();
                int y = (int)Argument(a, 1).AsNumber();
                int count = 0;

                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        if (host.GetCell(x + dx, y + dy))
                            count++;
                    }
                }

                return Value.Of((double)count);
            }));

            return table;
        }

        private static Value BuildSimTable(IScriptHost host)
        {
            Value table = Value.Table();

            table.SetField("step", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                host.Step();
                return Value.Nil();
            }));

            table.SetField("run", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                int steps = a.Count > 0 ? (int)a[0].AsNumber() : 1;

                if (steps < 0)
                    steps = 0;

                if (steps > 100000)
                    steps = 100000;

                for (int n = 0; n < steps; n++)
                    host.Step();

                return Value.Nil();
            }));

            table.SetField("start", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                host.Start();
                return Value.Nil();
            }));

            table.SetField("stop", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                host.Stop();
                return Value.Nil();
            }));

            table.SetField("reset", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                host.Reset();
                return Value.Nil();
            }));

            table.SetField("generation", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                return Value.Of((double)host.Generation);
            }));

            table.SetField("population", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                return Value.Of((double)host.Population);
            }));

            table.SetField("running", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                return Value.Of(host.IsRunning);
            }));

            return table;
        }

        private static Value BuildRulesTable(IScriptHost host)
        {
            Value table = Value.Table();

            table.SetField("get", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                return Value.Of(host.Rules);
            }));

            table.SetField("set", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                host.SetRules(Argument(a, 0).AsText());
                return Value.Nil();
            }));

            return table;
        }

        private static Value BuildStateTable(IScriptHost host)
        {
            Value table = Value.Table();

            table.SetField("grid", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                return Value.Of((double)host.GridSize);
            }));

            table.SetField("generation", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                return Value.Of((double)host.Generation);
            }));

            table.SetField("population", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                return Value.Of((double)host.Population);
            }));

            table.SetField("rules", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                return Value.Of(host.Rules);
            }));

            table.SetField("alive", Value.Native(delegate (Interpreter i, List<Value> a)
            {
                if (host.GridSize <= 0)
                    return Value.Of(0.0);

                double total = host.GridSize * host.GridSize;

                return Value.Of(host.Population / total);
            }));

            return table;
        }

        private static Value Argument(List<Value> arguments, int index)
        {
            if (arguments == null || index >= arguments.Count)
                return Value.Nil();

            return arguments[index];
        }
    }
}
