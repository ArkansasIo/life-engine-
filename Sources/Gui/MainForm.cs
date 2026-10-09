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
using System.Windows.Forms;

using LifeEngine.Configuration;

namespace LifeEngine.Gui
{
    /// <summary>
    /// The main window: the ribbon across the top, the tabbed control panel down
    /// the left, the grid in the middle, the event log across the bottom and the
    /// status bar under that.
    ///
    /// The form owns no state of its own. Everything it shows comes from
    /// <see cref="AppContext"/>, and everything it does is a call into it, so a
    /// panel can be rebuilt or hidden without the form losing track of anything.
    /// </summary>
    public sealed class MainForm : Form
    {
        private AppContext m_context;

        private MenuStrip m_menu;
        private StatusStrip m_status;
        private ToolStripStatusLabel m_stateLabel;
        private ToolStripStatusLabel m_ruleLabel;
        private ToolStripStatusLabel m_generationLabel;
        private ToolStripStatusLabel m_populationLabel;
        private ToolStripStatusLabel m_sizeLabel;
        private ToolStripStatusLabel m_speedLabel;
        private ToolStripStatusLabel m_zoomLabel;
        private ToolStripStatusLabel m_worldLabel;

        private SplitContainer m_outerSplit;
        private SplitContainer m_innerSplit;
        private TabControl m_tabs;
        private LifeGrid m_grid;
        private ListBox m_eventLog;

        private WorldPanel m_worldPanel;
        private WolfPanel m_wolfPanel;
        private ToolsPanel m_toolsPanel;

        private Timer m_simulationTimer;
        private Timer m_schedulerTimer;

        private int m_zoom;
        private const int MaxZoom = 5;
        private const double ZoomRate = 1.5;

        private bool m_painting;
        private bool m_paintState;
        private int m_panX;
        private int m_panY;
        private bool m_panning;

        public MainForm()
        {
            m_context = new AppContext();

            BuildWindow();
            BuildMenu();
            BuildTabs();
            BuildStatusBar();

            m_context.Changed += OnContextChanged;

            m_context.InstallDefaultSchedules();

            StartTimers();

            m_context.Log("interface ready");

            RefreshEverything();
        }

        // ------------------------------------------------------------------
        // Window
        // ------------------------------------------------------------------

        private void BuildWindow()
        {
            Text = m_context.Config.GetString(ConfigDefaults.Name) + " " + m_context.Version.ShortVersion;

            Icon = FormIcon.Load();

            ClientSize = new Size(
                m_context.Config.GetInt(ConfigDefaults.WindowWidth),
                m_context.Config.GetInt(ConfigDefaults.WindowHeight));

            MinimumSize = new Size(760, 520);

            BackColor = Color.FromArgb(26, 30, 36);
            ForeColor = Color.Gainsboro;

            StartPosition = m_context.Config.GetBool(ConfigDefaults.CenterOnStart)
                ? FormStartPosition.CenterScreen
                : FormStartPosition.WindowsDefaultLocation;

            KeyPreview = true;

            // A dark window, so the panels and the grid do not glare.
            MainMenuStrip = null;
        }

        // ------------------------------------------------------------------
        // The ribbon
        // ------------------------------------------------------------------

        private void BuildMenu()
        {
            m_menu = new MenuStrip();
            m_menu.Dock = DockStyle.Top;
            m_menu.BackColor = Color.FromArgb(38, 44, 54);
            m_menu.ForeColor = Color.Gainsboro;
            m_menu.Renderer = new DarkMenuRenderer();

            m_menu.Items.Add(BuildFileMenu());
            m_menu.Items.Add(BuildSimulationMenu());
            m_menu.Items.Add(BuildViewMenu());
            m_menu.Items.Add(BuildWorldMenu());
            m_menu.Items.Add(BuildEcosystemMenu());
            m_menu.Items.Add(BuildWolvesMenu());
            m_menu.Items.Add(BuildBestiaryMenu());
            m_menu.Items.Add(BuildDataMenu());
            m_menu.Items.Add(BuildToolsMenu());
            m_menu.Items.Add(BuildUpdatesMenu());
            m_menu.Items.Add(BuildHelpMenu());

            MainMenuStrip = m_menu;

            Controls.Add(m_menu);
        }

        private ToolStripMenuItem BuildFileMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem("&File");

            menu.DropDownItems.Add(Item("&New grid", Keys.Control | Keys.N, delegate
            {
                m_context.Clear();
            }));

            menu.DropDownItems.Add(Item("&Randomize", Keys.Control | Keys.R, delegate
            {
                m_context.Randomize(m_context.Config.GetDouble(ConfigDefaults.RandomDensity));
            }));

            menu.DropDownItems.Add(new ToolStripSeparator());

            menu.DropDownItems.Add(Item("&Clear grid", Keys.Control | Keys.Delete, delegate
            {
                m_context.Clear();
            }));

            menu.DropDownItems.Add(Item("Rese&t to start", Keys.Control | Keys.Back, delegate
            {
                m_context.Reset();
            }));

            menu.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem save = new ToolStripMenuItem("&Save settings");

            save.Click += delegate { m_context.SaveSettings(); };
            menu.DropDownItems.Add(save);

            ToolStripMenuItem reload = new ToolStripMenuItem("Reload settings");

            reload.Click += delegate { m_context.ReloadSettings(); };
            menu.DropDownItems.Add(reload);

            menu.DropDownItems.Add(new ToolStripSeparator());

            menu.DropDownItems.Add(Item("E&xit", Keys.Alt | Keys.F4, delegate { Close(); }));

            return menu;
        }

        private ToolStripMenuItem BuildSimulationMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem("&Simulation");

            menu.DropDownItems.Add(Item("&Start", Keys.F5, delegate { m_context.Start(); }));

            menu.DropDownItems.Add(Item("S&top", Keys.F6, delegate { m_context.Stop(); }));

            menu.DropDownItems.Add(Item("Ste&p", Keys.F7, delegate { m_context.Step(); }));

            menu.DropDownItems.Add(new ToolStripSeparator());

            // Speed.
            ToolStripMenuItem speed = new ToolStripMenuItem("S&peed");

            int[] speeds = { 1, 2, 5, 10 };

            for (int i = 0; i < speeds.Length; i++)
            {
                int value = speeds[i];

                ToolStripMenuItem entry = new ToolStripMenuItem(value.ToString());

                entry.Click += delegate
                {
                    m_context.Config.SetInt(ConfigDefaults.Speed, value);
                    ApplyTimerInterval();
                    RefreshEverything();
                };

                speed.DropDownItems.Add(entry);
            }

            menu.DropDownItems.Add(speed);

            // Grid size.
            ToolStripMenuItem size = new ToolStripMenuItem("&Grid size");

            int[] sizes = { 25, 50, 75, 100, 150, 200, 250 };

            for (int i = 0; i < sizes.Length; i++)
            {
                int value = sizes[i];

                ToolStripMenuItem entry = new ToolStripMenuItem(value.ToString());

                entry.Click += delegate
                {
                    m_context.Config.SetInt(ConfigDefaults.GridSize, value);
                    m_context.SetGridSize(value);
                };

                size.DropDownItems.Add(entry);
            }

            menu.DropDownItems.Add(size);

            // Rules: the shape and the cadence, both as sub-menus, so the
            // running questions are reachable without leaving the keyboard.
            menu.DropDownItems.Add(BuildRulesSubMenu());
            menu.DropDownItems.Add(BuildCadenceSubMenu());

            menu.DropDownItems.Add(new ToolStripSeparator());

            menu.DropDownItems.Add(Item("Reset &zoom", Keys.Control | Keys.D0, delegate
            {
                m_zoom = 0;
                ApplyZoom();
            }));

            return menu;
        }

        /// <summary>
        /// The 24 rulesets as a sub-menu. The current one is ticked, and ticking
        /// a new one changes the rules on the running simulation.
        /// </summary>
        private ToolStripMenuItem BuildRulesSubMenu()
        {
            ToolStripMenuItem rules = new ToolStripMenuItem("&Rules");

            string[] names =
            {
                "23/3 - Conway's Life", "125/36 - 2x2", "34/34 - 34 Life",
                "1358/357 - Amoeba", "35678/4678 - Anneal", "4567/345 - Assimilation",
                "235678/378 - Coagulations", "45678/3 - Coral", "34678/3678 - Day & Night",
                "5678/35678 - Diamoeba", "012345678/3 - Flakes", "1/1 - Gnarl",
                "23/36 - HighLife", "34678/0123478 - Inverse Life", "5/345 - Long Life",
                "12345/3 - Maze", "1234/3 - Mazectric", "245/368 - Move",
                "238/357 - Pseudo Life", "1357/1357 - Replicator", "/2 - Seeds",
                "/234 - Serviettes", "235678/3678 - Stains", "2345/45678 - Walled cities"
            };

            for (int i = 0; i < names.Length; i++)
            {
                ToolStripMenuItem entry = new ToolStripMenuItem(names[i]);

                entry.Tag = names[i].Substring(0, names[i].IndexOf(' '));

                entry.Click += delegate(object sender, EventArgs e)
                {
                    ToolStripMenuItem picked = sender as ToolStripMenuItem;

                    if (picked == null)
                        return;

                    // Untick the rest of the group, then tick this one.
                    for (int j = 0; j < picked.Owner.Items.Count; j++)
                    {
                        ToolStripMenuItem other = picked.Owner.Items[j] as ToolStripMenuItem;

                        if (other != null)
                            other.Checked = false;
                    }

                    picked.Checked = true;

                    m_context.ApplyRules(picked.Tag as string);
                };

                rules.DropDownItems.Add(entry);
            }

            // Only one ruleset is live at a time.
            rules.DropDownOpening += delegate
            {
                string current = m_context.Game.Rules;

                for (int i = 0; i < rules.DropDownItems.Count; i++)
                {
                    ToolStripMenuItem entry = rules.DropDownItems[i] as ToolStripMenuItem;

                    if (entry == null)
                        continue;

                    entry.Checked = string.Equals(entry.Tag as string, current,
                                                  StringComparison.Ordinal);
                }
            };

            return rules;
        }

        /// <summary>
        /// How fast the simulation runs: the four named speeds and the
        /// millisecond interval each one stands for.
        /// </summary>
        private ToolStripMenuItem BuildCadenceSubMenu()
        {
            ToolStripMenuItem cadence = new ToolStripMenuItem("&Cadence");

            int[] speeds = { 1, 2, 5, 10 };

            for (int i = 0; i < speeds.Length; i++)
            {
                int value = speeds[i];

                ToolStripMenuItem entry = new ToolStripMenuItem(
                    value + "  (" + ((11 - value) * 10) + " ms per generation)");

                entry.Tag = value;

                entry.Click += delegate(object sender, EventArgs e)
                {
                    ToolStripMenuItem picked = sender as ToolStripMenuItem;

                    if (picked == null)
                        return;

                    for (int j = 0; j < picked.Owner.Items.Count; j++)
                    {
                        ToolStripMenuItem other = picked.Owner.Items[j] as ToolStripMenuItem;

                        if (other != null)
                            other.Checked = false;
                    }

                    picked.Checked = true;

                    m_context.Config.SetInt(ConfigDefaults.Speed, (int)picked.Tag);
                    ApplyTimerInterval();
                    RefreshEverything();
                };

                cadence.DropDownItems.Add(entry);
            }

            cadence.DropDownOpening += delegate
            {
                int current = m_context.Config.GetInt(ConfigDefaults.Speed);

                for (int i = 0; i < cadence.DropDownItems.Count; i++)
                {
                    ToolStripMenuItem entry = cadence.DropDownItems[i] as ToolStripMenuItem;

                    if (entry == null || !(entry.Tag is int))
                        continue;

                    entry.Checked = (int)entry.Tag == current;
                }
            };

            return cadence;
        }

        private ToolStripMenuItem BuildViewMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem("&View");

            menu.DropDownItems.Add(Item("Zoom &in", Keys.Control | Keys.Oemplus, delegate
            {
                if (m_zoom < MaxZoom)
                {
                    m_zoom++;
                    ApplyZoom();
                }
            }));

            menu.DropDownItems.Add(Item("Zoom &out", Keys.Control | Keys.OemMinus, delegate
            {
                if (m_zoom > 0)
                {
                    m_zoom--;
                    ApplyZoom();
                }
            }));

            menu.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem lines = new ToolStripMenuItem("Show &grid lines");

            lines.CheckOnClick = true;
            lines.Checked = m_context.Config.GetBool(ConfigDefaults.ShowGrid);

            lines.CheckedChanged += delegate
            {
                m_context.Config.SetBool(ConfigDefaults.ShowGrid, lines.Checked);
                ApplyAppearance();
            };

            lines.ShortcutKeys = Keys.Control | Keys.G;
            lines.ShowShortcutKeys = true;

            menu.DropDownItems.Add(lines);

            ToolStripMenuItem sidebar = new ToolStripMenuItem("Show &sidebar");

            sidebar.CheckOnClick = true;
            sidebar.Checked = m_context.Config.GetBool(ConfigDefaults.SidebarVisible);

            sidebar.CheckedChanged += delegate
            {
                m_context.Config.SetBool(ConfigDefaults.SidebarVisible, sidebar.Checked);
                ApplySidebar();
            };

            sidebar.ShortcutKeys = Keys.Control | Keys.B;
            sidebar.ShowShortcutKeys = true;

            menu.DropDownItems.Add(sidebar);

            menu.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem live = new ToolStripMenuItem("&Live cell colour");

            live.Click += delegate { PickColour(ConfigDefaults.LiveColor); };
            menu.DropDownItems.Add(live);

            ToolStripMenuItem dead = new ToolStripMenuItem("&Dead cell colour");

            dead.Click += delegate { PickColour(ConfigDefaults.DeadColor); };
            menu.DropDownItems.Add(dead);

            ToolStripMenuItem history = new ToolStripMenuItem("&History cell colour");

            history.Click += delegate { PickColour(ConfigDefaults.HistoryColor); };
            menu.DropDownItems.Add(history);

            return menu;
        }

        private ToolStripMenuItem BuildWorldMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem("&World");

            menu.DropDownItems.Add(Item("&New world", Keys.Control | Keys.W, delegate
            {
                m_context.BuildWorld(true);
                ShowTab(0);
            }));

            ToolStripMenuItem empty = new ToolStripMenuItem("New world (no life)");

            empty.Click += delegate
            {
                m_context.BuildWorld(false);
                ShowTab(0);
            };

            menu.DropDownItems.Add(empty);

            menu.DropDownItems.Add(new ToolStripSeparator());

            menu.DropDownItems.Add(Item("&Run 100 ticks", Keys.Control | Keys.T, delegate
            {
                m_context.RunEcosystem(100);
                ShowTab(0);
            }));

            ToolStripMenuItem runSettings = new ToolStripMenuItem("Run ticks from settings");

            runSettings.Click += delegate
            {
                m_context.RunEcosystem(m_context.Config.GetInt(ConfigDefaults.TicksPerRun));
                ShowTab(0);
            };

            menu.DropDownItems.Add(runSettings);

            menu.DropDownItems.Add(Item("Step one tick", Keys.None, delegate
            {
                m_context.RunEcosystem(1);
                ShowTab(0);
            }));

            menu.DropDownItems.Add(new ToolStripSeparator());

            menu.DropDownItems.Add(Item("&Clear life", Keys.None, delegate
            {
                m_context.ClearEcosystem();
                ShowTab(0);
            }));

            ToolStripMenuItem reseed = new ToolStripMenuItem("Reseed with a new seed");

            reseed.Click += delegate
            {
                int seed = m_context.Random.Next();

                m_context.Config.SetInt(ConfigDefaults.WorldSeed, seed);
                m_context.BuildWorld(true);

                ShowTab(0);
            };

            menu.DropDownItems.Add(reseed);

            menu.DropDownItems.Add(new ToolStripSeparator());

            menu.DropDownItems.Add(Item("World &report", Keys.None, delegate
            {
                ShowReport("World report", m_context.DescribeWorld());
            }));

            return menu;
        }

        private ToolStripMenuItem BuildWolvesMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem("W&olves");

            menu.DropDownItems.Add(Item("&Found pack", Keys.None, delegate
            {
                m_context.FoundPack("The Grey Company", "Fenrir");
                ShowTab(1);
            }));

            menu.DropDownItems.Add(new ToolStripSeparator());

            menu.DropDownItems.Add(Item("Run one &night", Keys.None, delegate
            {
                m_context.RunNight(false);
                ShowTab(1);
            }));

            ToolStripMenuItem moon = new ToolStripMenuItem("Run a night under a &full moon");

            moon.Click += delegate
            {
                m_context.RunNight(true);
                ShowTab(1);
            };

            menu.DropDownItems.Add(moon);

            menu.DropDownItems.Add(Item("Run a &season", Keys.None, delegate
            {
                m_context.RunSeason();
                ShowTab(1);
            }));

            menu.DropDownItems.Add(new ToolStripSeparator());

            menu.DropDownItems.Add(Item("Pack &report", Keys.None, delegate
            {
                ShowReport("Pack report", m_context.DescribePack());
            }));

            return menu;
        }

        /// <summary>
        /// The ecosystem: the shape of the world and how its species behave.
        /// The two multipliers are the knobs that decide whether a run ends in
        /// a balance or a collapse, so they get a sub-menu of their own.
        /// </summary>
        private ToolStripMenuItem BuildEcosystemMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem("&Ecosystem");

            menu.DropDownItems.Add(Item("Build &world", Keys.None, delegate
            {
                if (m_context.BuildWorld(true))
                    ShowTab(0);
            }));

            menu.DropDownItems.Add(Item("Build world (&barren)", Keys.None, delegate
            {
                if (m_context.BuildWorld(false))
                    ShowTab(0);
            }));

            menu.DropDownItems.Add(new ToolStripSeparator());

            menu.DropDownItems.Add(Item("&Run one tick", Keys.None, delegate
            {
                m_context.RunEcosystem(1);
                ShowTab(0);
            }));

            menu.DropDownItems.Add(Item("Run &ten ticks", Keys.None, delegate
            {
                m_context.RunEcosystem(10);
                ShowTab(0);
            }));

            menu.DropDownItems.Add(Item("Run &hundred ticks", Keys.None, delegate
            {
                m_context.RunEcosystem(100);
                ShowTab(0);
            }));

            menu.DropDownItems.Add(new ToolStripSeparator());

            menu.DropDownItems.Add(BuildGrowthSubMenu());
            menu.DropDownItems.Add(BuildDeathSubMenu());

            menu.DropDownItems.Add(new ToolStripSeparator());

            menu.DropDownItems.Add(Item("Clear &life", Keys.None, delegate
            {
                m_context.ClearEcosystem();
                ShowTab(0);
            }));

            menu.DropDownItems.Add(Item("&Census", Keys.None, delegate
            {
                ShowReport("Census", m_context.DescribeWorld());
            }));

            return menu;
        }

        /// <summary>Growth multiplier: 0.1x at the bottom, 12x at the top.</summary>
        private ToolStripMenuItem BuildGrowthSubMenu()
        {
            ToolStripMenuItem growth = new ToolStripMenuItem("&Growth rate");

            double[] rates = { 0.1, 0.25, 0.5, 1.0, 2.0, 4.0, 8.0, 12.0 };

            for (int i = 0; i < rates.Length; i++)
            {
                double value = rates[i];

                ToolStripMenuItem entry = new ToolStripMenuItem(value + "x");

                entry.Tag = value;

                entry.Click += delegate(object sender, EventArgs e)
                {
                    ToolStripMenuItem picked = sender as ToolStripMenuItem;

                    if (picked == null)
                        return;

                    for (int j = 0; j < picked.Owner.Items.Count; j++)
                    {
                        ToolStripMenuItem other = picked.Owner.Items[j] as ToolStripMenuItem;

                        if (other != null)
                            other.Checked = false;
                    }

                    picked.Checked = true;

                    m_context.Config.SetDouble(ConfigDefaults.GrowthMultiplier, (double)picked.Tag);

                    m_context.Log("growth rate set to " + picked.Tag + "x");
                    m_context.Notify();
                };

                growth.DropDownItems.Add(entry);
            }

            growth.DropDownOpening += delegate
            {
                double current = m_context.Config.GetDouble(ConfigDefaults.GrowthMultiplier);

                for (int i = 0; i < growth.DropDownItems.Count; i++)
                {
                    ToolStripMenuItem entry = growth.DropDownItems[i] as ToolStripMenuItem;

                    if (entry == null || !(entry.Tag is double))
                        continue;

                    entry.Checked = Math.Abs((double)entry.Tag - current) < 0.0001;
                }
            };

            return growth;
        }

        /// <summary>Death multiplier, on the same ladder as growth.</summary>
        private ToolStripMenuItem BuildDeathSubMenu()
        {
            ToolStripMenuItem death = new ToolStripMenuItem("&Death rate");

            double[] rates = { 0.1, 0.25, 0.5, 1.0, 2.0, 4.0, 8.0, 12.0 };

            for (int i = 0; i < rates.Length; i++)
            {
                double value = rates[i];

                ToolStripMenuItem entry = new ToolStripMenuItem(value + "x");

                entry.Tag = value;

                entry.Click += delegate(object sender, EventArgs e)
                {
                    ToolStripMenuItem picked = sender as ToolStripMenuItem;

                    if (picked == null)
                        return;

                    for (int j = 0; j < picked.Owner.Items.Count; j++)
                    {
                        ToolStripMenuItem other = picked.Owner.Items[j] as ToolStripMenuItem;

                        if (other != null)
                            other.Checked = false;
                    }

                    picked.Checked = true;

                    m_context.Config.SetDouble(ConfigDefaults.DeathMultiplier, (double)picked.Tag);

                    m_context.Log("death rate set to " + picked.Tag + "x");
                    m_context.Notify();
                };

                death.DropDownItems.Add(entry);
            }

            death.DropDownOpening += delegate
            {
                double current = m_context.Config.GetDouble(ConfigDefaults.DeathMultiplier);

                for (int i = 0; i < death.DropDownItems.Count; i++)
                {
                    ToolStripMenuItem entry = death.DropDownItems[i] as ToolStripMenuItem;

                    if (entry == null || !(entry.Tag is double))
                        continue;

                    entry.Checked = Math.Abs((double)entry.Tag - current) < 0.0001;
                }
            };

            return death;
        }

        /// <summary>
        /// The bestiary: the creatures a fight can be staged with, grouped by
        /// the kind of creature it is, because that is the distinction that
        /// decides which side it fights on.
        /// </summary>
        private ToolStripMenuItem BuildBestiaryMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem("&Bestiary");

            menu.DropDownItems.Add(BuildBeastGroup("&Prey", Combat.BeastKind.Prey));
            menu.DropDownItems.Add(BuildBeastGroup("&Rivals", Combat.BeastKind.Rival));
            menu.DropDownItems.Add(BuildBeastGroup("Preda&tors", Combat.BeastKind.Predator));
            menu.DropDownItems.Add(BuildBeastGroup("&Others", Combat.BeastKind.Other));

            menu.DropDownItems.Add(new ToolStripSeparator());

            menu.DropDownItems.Add(Item("Bestiary &summary", Keys.None, delegate
            {
                ShowReport("Bestiary", Combat.Bestiary.Describe());
            }));

            return menu;
        }

        /// <summary>One kind of creature, as a sub-menu.</summary>
        private ToolStripMenuItem BuildBeastGroup(string title, Combat.BeastKind kind)
        {
            ToolStripMenuItem group = new ToolStripMenuItem(title);

            List<Combat.Beast> beasts = Combat.Bestiary.OfKind(kind);

            if (beasts == null || beasts.Count == 0)
            {
                ToolStripMenuItem none = new ToolStripMenuItem("(none defined)");

                none.Enabled = false;
                group.DropDownItems.Add(none);

                return group;
            }

            for (int i = 0; i < beasts.Count; i++)
            {
                Combat.Beast beast = beasts[i];

                ToolStripMenuItem entry = new ToolStripMenuItem(beast.Name);

                entry.Click += delegate
                {
                    ShowReport("Beast: " + beast.Name, DescribeBeast(beast));
                };

                group.DropDownItems.Add(entry);
            }

            return group;
        }

        /// <summary>A beast as a short block of text for a report window.</summary>
        private static string DescribeBeast(Combat.Beast beast)
        {
            if (beast == null)
                return "No such beast.";

            System.Text.StringBuilder text = new System.Text.StringBuilder();

            text.AppendLine(beast.Name);
            text.AppendLine(new string('-', beast.Name.Length));
            text.AppendLine();
            text.AppendLine("Kind        " + beast.Kind);
            text.AppendLine("Side        " + beast.Side);
            text.AppendLine("Symbol      " + beast.Symbol);
            text.AppendLine("Strength    " + beast.Strength);
            text.AppendLine("Speed       " + beast.Speed);
            text.AppendLine("Mass        " + beast.Mass + " kg");
            text.AppendLine("Defence     " + beast.Defence);
            text.AppendLine("Aggression  " + beast.Aggression);
            text.AppendLine("Health bonus " + beast.HealthBonus);
            text.AppendLine();
            text.AppendLine(beast.Description);

            return text.ToString();
        }

        /// <summary>
        /// Data: turning a run into something that outlives it. Every entry
        /// here reads the live objects and writes a file; nothing is imported.
        /// </summary>
        private ToolStripMenuItem BuildDataMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem("&Data");

            menu.DropDownItems.Add(Item("Export &everything (CSV)", Keys.None, delegate
            {
                ExportEverything();
            }));

            menu.DropDownItems.Add(Item("Export everything (Excel &workbook)", Keys.None, delegate
            {
                ExportWorkbook();
            }));

            menu.DropDownItems.Add(new ToolStripSeparator());

            for (int i = 0; i < ExportBuilders.Length; i++)
            {
                ExportEntry entry = ExportBuilders[i];

                menu.DropDownItems.Add(Item("Export " + entry.Label, Keys.None, delegate
                {
                    ExportOne(entry);
                }));
            }

            menu.DropDownItems.Add(new ToolStripSeparator());

            menu.DropDownItems.Add(Item("What would be exported", Keys.None, delegate
            {
                ShowReport("Export", DescribeExport());
            }));

            return menu;
        }

        /// <summary>
        /// One exportable table: a label to show and the builder that produces
        /// it. Held as data so the menu, the report and the export itself all
        /// read from one list rather than three that can drift apart.
        /// </summary>
        private sealed class ExportEntry
        {
            public string Label;
            public string FileName;
            public Func<AppContext, Data.DataTable> Build;

            public ExportEntry(string label, string fileName, Func<AppContext, Data.DataTable> build)
            {
                Label = label;
                FileName = fileName;
                Build = build;
            }
        }

        private static readonly ExportEntry[] ExportBuilders =
        {
            new ExportEntry("&version", "version", Data.ExportBuilder.BuildVersion),
            new ExportEntry("&grid summary", "grid-summary", Data.ExportBuilder.BuildGridSummary),
            new ExportEntry("&generation", "generation", Data.ExportBuilder.BuildGeneration),
            new ExportEntry("&world", "world", Data.ExportBuilder.BuildWorld),
            new ExportEntry("&biome census", "biome-census", Data.ExportBuilder.BuildBiomeCensus),
            new ExportEntry("&species", "species", Data.ExportBuilder.BuildSpecies),
            new ExportEntry("tile &sample", "tile-sample", Data.ExportBuilder.BuildTileSample),
            new ExportEntry("&pack", "pack", Data.ExportBuilder.BuildPack),
            new ExportEntry("&wolves", "wolves", Data.ExportBuilder.BuildWolves),
            new ExportEntry("&settings", "settings", Data.ExportBuilder.BuildSettings),
            new ExportEntry("sc&hedules", "schedules", Data.ExportBuilder.BuildSchedules),
            new ExportEntry("&releases", "releases", Data.ExportBuilder.BuildReleases),
            new ExportEntry("&events", "events", Data.ExportBuilder.BuildEvents)
        };

        /// <summary>Which of the export tables has anything to say right now.</summary>
        private string DescribeExport()
        {
            System.Text.StringBuilder text = new System.Text.StringBuilder();

            text.AppendLine("Export reads the live objects. What each table holds:");
            text.AppendLine();

            for (int i = 0; i < ExportBuilders.Length; i++)
            {
                ExportEntry entry = ExportBuilders[i];

                string state;

                try
                {
                    Data.DataTable data = entry.Build(m_context);

                    state = data == null ? "unavailable" : data.Rows.Count + " row(s)";
                }
                catch (Exception error)
                {
                    state = "failed - " + error.Message;
                }

                text.AppendLine("  " + entry.FileName.PadRight(16) + state);
            }

            text.AppendLine();
            text.AppendLine("Output goes to the Exports folder beside the program.");

            return text.ToString();
        }

        /// <summary>Exports every table that can be built, one CSV file each.</summary>
        private void ExportEverything()
        {
            string folder = ExportFolder();

            System.Text.StringBuilder report = new System.Text.StringBuilder();

            report.AppendLine("Exported to " + folder);
            report.AppendLine();

            int written = 0;

            for (int i = 0; i < ExportBuilders.Length; i++)
            {
                string message = WriteOne(folder, ExportBuilders[i]);

                report.AppendLine(message);
                m_context.Log(message);

                written++;
            }

            report.AppendLine();
            report.AppendLine(written + " file(s) written.");

            m_context.Notify();

            ShowReport("Export", report.ToString());
        }

        /// <summary>Exports every table into one Excel workbook.</summary>
        private void ExportWorkbook()
        {
            string folder = ExportFolder();
            string path = System.IO.Path.Combine(folder, "LifeEngine.xls");

            List<Data.DataTable> tables = new List<Data.DataTable>();

            for (int i = 0; i < ExportBuilders.Length; i++)
            {
                try
                {
                    Data.DataTable data = ExportBuilders[i].Build(m_context);

                    if (data != null && data.Rows.Count > 0)
                        tables.Add(data);
                }
                catch (Exception)
                {
                    // A table that cannot be built is left out rather than
                    // aborting the workbook.
                }
            }

            if (tables.Count == 0)
            {
                ShowReport("Export", "Nothing to export yet.\r\n\r\nBuild a world or found a pack first.");
                return;
            }

            System.Text.StringBuilder report = new System.Text.StringBuilder();

            try
            {
                List<string> problems = Data.SpreadsheetWriter.WriteWorkbook(path, tables);

                report.AppendLine("Wrote " + tables.Count + " table(s) to " + path);

                if (problems != null && problems.Count > 0)
                {
                    report.AppendLine();
                    report.AppendLine("Problems:");

                    for (int i = 0; i < problems.Count; i++)
                        report.AppendLine("  " + problems[i]);
                }

                m_context.Log("workbook written: " + path);
            }
            catch (Exception error)
            {
                report.AppendLine("The workbook could not be written: " + error.Message);
                report.AppendLine();
                report.AppendLine("The Jet provider is part of Windows only where Office or the " +
                                  "Access runtime is installed. Use " +
                                  "Export everything (CSV) instead - it needs only a file system.");

                m_context.Log("workbook export failed: " + error.Message);
            }

            m_context.Notify();

            ShowReport("Export", report.ToString());
        }

        /// <summary>Exports one named table.</summary>
        private void ExportOne(ExportEntry entry)
        {
            string folder = ExportFolder();
            string message = WriteOne(folder, entry);

            m_context.Log(message);
            ShowReport("Export " + entry.FileName, message);
        }

        /// <summary>Writes one table as CSV, returning a line describing what happened.</summary>
        private string WriteOne(string folder, ExportEntry entry)
        {
            try
            {
                Data.DataTable data = entry.Build(m_context);

                if (data == null || data.Rows.Count == 0)
                    return entry.FileName + ": nothing to write";

                string path = System.IO.Path.Combine(folder, entry.FileName + ".csv");

                Data.SpreadsheetWriter.WriteCsv(path, data);

                return entry.FileName + ": " + data.Rows.Count + " row(s)";
            }
            catch (Exception error)
            {
                return entry.FileName + ": failed - " + error.Message;
            }
        }

        /// <summary>The export folder, created on first use.</summary>
        private static string ExportFolder()
        {
            string folder = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Exports");

            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            return folder;
        }

        private ToolStripMenuItem BuildToolsMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem("&Tools");

            menu.DropDownItems.Add(Item("&Script console", Keys.Control | Keys.E, delegate
            {
                ShowTab(2);
            }));

            ToolStripMenuItem settings = new ToolStripMenuItem("&Settings editor");

            settings.Click += delegate { ShowTab(2); };

            menu.DropDownItems.Add(settings);

            menu.DropDownItems.Add(new ToolStripSeparator());

            menu.DropDownItems.Add(Item("Install default &jobs", Keys.None, delegate
            {
                m_context.InstallDefaultSchedules();
                ShowTab(2);
            }));

            menu.DropDownItems.Add(Item("Run due jobs &now", Keys.None, delegate
            {
                List<Schedule> ran = m_context.RunDueSchedules();

                m_context.Log("scheduler: " + ran.Count + " job(s) ran");
                ShowTab(2);
            }));

            menu.DropDownItems.Add(new ToolStripSeparator());

            menu.DropDownItems.Add(Item("&Event log", Keys.None, delegate
            {
                ShowReport("Event log", string.Join("\r\n", ToArray(m_context.EventLog)));
            }));

            return menu;
        }

        private ToolStripMenuItem BuildUpdatesMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem("&Updates");

            menu.DropDownItems.Add(Item("&Load manifest", Keys.None, delegate
            {
                m_context.LoadUpdateManifest();
                ShowTab(3);
            }));

            menu.DropDownItems.Add(Item("What would &apply", Keys.None, delegate
            {
                ShowTab(3);
            }));

            menu.DropDownItems.Add(new ToolStripSeparator());

            menu.DropDownItems.Add(Item("&Version information", Keys.None, delegate
            {
                ShowReport("Version", m_context.Version.Describe());
            }));

            return menu;
        }

        private ToolStripMenuItem BuildHelpMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem("&Help");

            menu.DropDownItems.Add(Item("&Controls", Keys.F1, delegate
            {
                ShowReport("Controls", ControlsHelp());
            }));

            menu.DropDownItems.Add(Item("&About", Keys.None, delegate
            {
                ShowReport("About", m_context.Version.Describe() +
                    "\r\nLicense: GNU General Public License v3 or later." +
                    "\r\nCopyright (c) 2026 ArkansasIo");
            }));

            return menu;
        }

        private static ToolStripMenuItem Item(string text, Keys shortcut, EventHandler onClick)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);

            if (shortcut != Keys.None)
            {
                item.ShortcutKeys = shortcut;
                item.ShowShortcutKeys = true;
            }

            item.Click += onClick;

            return item;
        }

        // ------------------------------------------------------------------
        // The tabbed sidebar
        // ------------------------------------------------------------------

        private void BuildTabs()
        {
            m_grid = new LifeGrid();
            m_grid.Dock = DockStyle.Fill;
            m_grid.TabStop = false;
            m_grid.MouseDown += OnGridMouseDown;
            m_grid.MouseMove += OnGridMouseMove;
            m_grid.MouseUp += OnGridMouseUp;
            m_grid.MouseWheel += OnGridMouseWheel;

            Panel gridHost = new Panel();
            gridHost.Dock = DockStyle.Fill;
            gridHost.AutoScroll = true;
            gridHost.BackColor = Color.FromArgb(18, 22, 28);
            gridHost.Controls.Add(m_grid);

            m_eventLog = new ListBox();
            m_eventLog.Dock = DockStyle.Fill;
            m_eventLog.BackColor = Color.FromArgb(18, 22, 28);
            m_eventLog.ForeColor = Color.FromArgb(170, 180, 195);
            m_eventLog.Font = new Font("Consolas", 8.0f);
            m_eventLog.IntegralHeight = false;

            m_tabs = new TabControl();
            m_tabs.Dock = DockStyle.Fill;

            m_worldPanel = new WorldPanel(m_context);
            m_wolfPanel = new WolfPanel(m_context);
            m_toolsPanel = new ToolsPanel(m_context);

            TabPage worldTab = new TabPage("World");
            worldTab.BackColor = Color.FromArgb(30, 34, 40);
            worldTab.Controls.Add(m_worldPanel);

            TabPage wolfTab = new TabPage("Wolves");
            wolfTab.BackColor = Color.FromArgb(30, 34, 40);
            wolfTab.Controls.Add(m_wolfPanel);

            TabPage toolsTab = new TabPage("Tools");
            toolsTab.BackColor = Color.FromArgb(30, 34, 40);
            toolsTab.Controls.Add(m_toolsPanel);

            TabPage logTab = new TabPage("Log");
            logTab.BackColor = Color.FromArgb(30, 34, 40);
            logTab.Controls.Add(m_eventLog);

            m_tabs.TabPages.Add(worldTab);
            m_tabs.TabPages.Add(wolfTab);
            m_tabs.TabPages.Add(toolsTab);
            m_tabs.TabPages.Add(logTab);

            // Right: the grid over the event log. Left: the tabs.
            m_innerSplit = new SplitContainer();
            m_innerSplit.Dock = DockStyle.Fill;
            m_innerSplit.Orientation = Orientation.Horizontal;
            m_innerSplit.SplitterDistance = 420;
            m_innerSplit.Panel1.Controls.Add(gridHost);
            m_innerSplit.Panel2.Controls.Add(m_eventLog);

            m_outerSplit = new SplitContainer();
            m_outerSplit.Dock = DockStyle.Fill;
            m_outerSplit.Orientation = Orientation.Vertical;
            m_outerSplit.SplitterDistance = m_context.Config.GetInt(ConfigDefaults.SidebarWidth);
            m_outerSplit.Panel1.Controls.Add(m_tabs);
            m_outerSplit.Panel2.Controls.Add(m_innerSplit);

            Controls.Add(m_outerSplit);

            // The menu has to be added last for docking order, so re-add it.
            Controls.SetChildIndex(m_outerSplit, 0);
        }

        private void ShowTab(int index)
        {
            if (m_tabs != null && index >= 0 && index < m_tabs.TabPages.Count)
                m_tabs.SelectedIndex = index;
        }

        // ------------------------------------------------------------------
        // Status bar
        // ------------------------------------------------------------------

        private void BuildStatusBar()
        {
            m_status = new StatusStrip();
            m_status.Dock = DockStyle.Bottom;
            m_status.BackColor = Color.FromArgb(38, 44, 54);
            m_status.ForeColor = Color.Gainsboro;
            m_status.SizingGrip = false;

            m_stateLabel = AddStatus("Ready", 90);
            m_ruleLabel = AddStatus("Rule: 23/3", 200);
            m_generationLabel = AddStatus("Gen: 0", 80);
            m_populationLabel = AddStatus("Pop: 0", 90);
            m_sizeLabel = AddStatus("Size: 100", 80);
            m_speedLabel = AddStatus("Speed: 10", 80);
            m_zoomLabel = AddStatus("Zoom: 1x", 80);
            m_worldLabel = AddStatus("No world", 220);

            Controls.Add(m_status);
        }

        private ToolStripStatusLabel AddStatus(string text, int width)
        {
            ToolStripStatusLabel label = new ToolStripStatusLabel(text);

            label.AutoSize = false;
            label.Width = width;
            label.TextAlign = ContentAlignment.MiddleLeft;

            m_status.Items.Add(label);

            return label;
        }

        // ------------------------------------------------------------------
        // Timers
        // ------------------------------------------------------------------

        private void StartTimers()
        {
            m_simulationTimer = new Timer();
            m_simulationTimer.Interval = m_context.TimerInterval();
            m_simulationTimer.Tick += delegate
            {
                m_context.Step();
            };

            m_schedulerTimer = new Timer();
            m_schedulerTimer.Interval = 10000;
            m_schedulerTimer.Tick += delegate
            {
                List<Schedule> ran = m_context.RunDueSchedules();

                if (ran.Count > 0)
                    m_context.Log("scheduler ran " + ran.Count + " job(s)");
            };

            m_schedulerTimer.Start();
        }

        private void ApplyTimerInterval()
        {
            if (m_simulationTimer != null)
                m_simulationTimer.Interval = m_context.TimerInterval();
        }

        private void ApplyRunning()
        {
            if (m_simulationTimer == null)
                return;

            if (m_context.IsRunning && !m_simulationTimer.Enabled)
                m_simulationTimer.Start();
            else if (!m_context.IsRunning && m_simulationTimer.Enabled)
                m_simulationTimer.Stop();
        }

        // ------------------------------------------------------------------
        // Appearance
        // ------------------------------------------------------------------

        private void ApplyAppearance()
        {
            m_grid.CellColorAlive = m_context.LiveColor;
            m_grid.CellColorDead = m_context.DeadColor;
            m_grid.CellColorHistory = m_context.HistoryColor;
            m_grid.LinesVisible = m_context.Config.GetBool(ConfigDefaults.ShowGrid);
            m_grid.LinesSize = (float)m_context.Config.GetDouble(ConfigDefaults.GridLineSize);

            m_grid.Invalidate();
        }

        private void ApplySidebar()
        {
            bool visible = m_context.Config.GetBool(ConfigDefaults.SidebarVisible);

            m_outerSplit.Panel1Collapsed = !visible;
        }

        private void ApplyZoom()
        {
            int baseSize = m_context.Game.Columns;

            // The grid is drawn at one pixel per cell times the zoom, so a zoom
            // step is a straight multiplication of the control's size.
            double factor = Math.Pow(ZoomRate, m_zoom);

            int size = (int)Math.Round(baseSize * factor);

            if (size < 100)
                size = 100;

            if (size > 20000)
                size = 20000;

            m_grid.Size = new Size(size, size);

            if (m_zoomLabel != null)
                m_zoomLabel.Text = "Zoom: " + (m_zoom + 1) + "x";
        }

        private void PickColour(string key)
        {
            using (ColorDialog picker = new ColorDialog())
            {
                picker.Color = m_context.Config.GetColor(key);
                picker.FullOpen = true;

                if (picker.ShowDialog(this) == DialogResult.OK)
                {
                    m_context.Config.SetColor(key, picker.Color);
                    ApplyAppearance();
                }
            }
        }

        // ------------------------------------------------------------------
        // Mouse on the grid
        // ------------------------------------------------------------------

        private void OnGridMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                m_panning = true;
                m_panX = e.X;
                m_panY = e.Y;
                m_grid.Cursor = Cursors.SizeAll;
                return;
            }

            if (e.Button != MouseButtons.Left)
                return;

            bool alive = !m_context.GetCell(CellX(e), CellY(e));

            m_painting = true;
            m_paintState = alive;

            m_context.SetCell(CellX(e), CellY(e), alive);
            m_context.Notify();
        }

        private void OnGridMouseMove(object sender, MouseEventArgs e)
        {
            if (m_panning)
            {
                Panel host = m_grid.Parent as Panel;

                if (host != null)
                {
                    host.AutoScrollPosition = new Point(
                        -host.AutoScrollPosition.X - (e.X - m_panX),
                        -host.AutoScrollPosition.Y - (e.Y - m_panY));
                }

                return;
            }

            if (!m_painting)
                return;

            m_context.SetCell(CellX(e), CellY(e), m_paintState);
            m_context.Notify();
        }

        private void OnGridMouseUp(object sender, MouseEventArgs e)
        {
            m_painting = false;
            m_panning = false;
            m_grid.Cursor = Cursors.Default;
        }

        private void OnGridMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0 && m_zoom < MaxZoom)
                m_zoom++;
            else if (e.Delta < 0 && m_zoom > 0)
                m_zoom--;
            else
                return;

            ApplyZoom();
        }

        /// <summary>Turns a pixel on the grid control into a cell column.</summary>
        private int CellX(MouseEventArgs e)
        {
            int columns = m_context.Game.Columns;

            return e.X * columns / Math.Max(1, m_grid.Width);
        }

        private int CellY(MouseEventArgs e)
        {
            int rows = m_context.Game.Rows;

            return e.Y * rows / Math.Max(1, m_grid.Height);
        }

        // ------------------------------------------------------------------
        // Keyboard
        // ------------------------------------------------------------------

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Space:
                    if (m_context.IsRunning)
                        m_context.Stop();
                    else
                        m_context.Start();

                    return true;

                case Keys.Escape:
                    m_context.Stop();
                    return true;

                case Keys.Left:
                    ScrollView(-40, 0);
                    return true;

                case Keys.Right:
                    ScrollView(40, 0);
                    return true;

                case Keys.Up:
                    ScrollView(0, -40);
                    return true;

                case Keys.Down:
                    ScrollView(0, 40);
                    return true;

                case Keys.PageUp:
                    ScrollView(0, -400);
                    return true;

                case Keys.PageDown:
                    ScrollView(0, 400);
                    return true;

                case Keys.Home:
                    ScrollView(-100000, -100000);
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Scrolls the panel the grid sits in. Named ScrollView rather than
        /// Scroll because ScrollableControl already has a Scroll method, and
        /// hiding an inherited member to save a word is not a trade worth making.
        /// </summary>
        private void ScrollView(int dx, int dy)
        {
            Panel host = m_grid.Parent as Panel;

            if (host == null)
                return;

            host.AutoScrollPosition = new Point(
                -host.AutoScrollPosition.X - dx,
                -host.AutoScrollPosition.Y - dy);
        }

        // ------------------------------------------------------------------
        // Refresh
        // ------------------------------------------------------------------

        private void OnContextChanged(object sender, EventArgs e)
        {
            RefreshEverything();
        }

        /// <summary>Pulls every displayed value back out of the context.</summary>
        public void RefreshEverything()
        {
            LifeGame game = m_context.Game;

            // The grid control has to be resized before its buffers, or
            // UpdateGrid throws on a size mismatch.
            if (m_grid.Columns != game.Columns || m_grid.Rows != game.Rows)
            {
                m_grid.Columns = game.Columns;
                m_grid.Rows = game.Rows;

                ApplyZoom();
            }

            m_grid.UpdateGrid(game.GameGrid, game.HistoryGrid);

            ApplyAppearance();
            ApplyRunning();

            m_stateLabel.Text = m_context.IsRunning ? "Running" : "Ready";
            m_ruleLabel.Text = "Rule: " + game.Rules;
            m_generationLabel.Text = "Gen: " + m_context.Generation;
            m_populationLabel.Text = "Pop: " + m_context.Population;
            m_sizeLabel.Text = "Size: " + game.Columns;
            m_speedLabel.Text = "Speed: " + m_context.Config.GetInt(ConfigDefaults.Speed);

            Ecosystem.Ecosystem ecosystem = m_context.Ecosystem;

            if (ecosystem == null)
            {
                m_worldLabel.Text = "No world";
            }
            else
            {
                m_worldLabel.Text = "World: " + ecosystem.TotalPopulation() +
                                    " head, tick " + ecosystem.Tick;
            }

            RefreshLog();
        }

        private void RefreshLog()
        {
            if (m_eventLog == null)
                return;

            IList<string> events = m_context.EventLog;

            // Only rebuild when something has actually been added, so the log
            // does not flicker on every simulation tick.
            if (m_eventLog.Items.Count == events.Count)
                return;

            m_eventLog.BeginUpdate();

            int from = m_eventLog.Items.Count;

            if (from > events.Count)
            {
                m_eventLog.Items.Clear();
                from = 0;
            }

            for (int i = from; i < events.Count; i++)
                m_eventLog.Items.Add(events[i]);

            if (m_eventLog.Items.Count > 0)
                m_eventLog.TopIndex = m_eventLog.Items.Count - 1;

            m_eventLog.EndUpdate();
        }

        // ------------------------------------------------------------------
        // Reports
        // ------------------------------------------------------------------

        private void ShowReport(string title, string text)
        {
            Form window = new Form();

            TextBox view = new TextBox();

            view.Multiline = true;
            view.ReadOnly = true;
            view.ScrollBars = ScrollBars.Both;
            view.WordWrap = false;
            view.Dock = DockStyle.Fill;
            view.Font = new Font("Consolas", 9.0f);
            view.BackColor = Color.FromArgb(22, 26, 32);
            view.ForeColor = Color.Gainsboro;
            view.Text = text;

            window.Text = title;
            window.ClientSize = new Size(660, 520);
            window.StartPosition = FormStartPosition.CenterParent;
            window.BackColor = Color.FromArgb(26, 30, 36);
            window.Controls.Add(view);
            window.Show(this);
        }

        private static string[] ToArray(IList<string> lines)
        {
            string[] array = new string[lines.Count];

            for (int i = 0; i < lines.Count; i++)
                array[i] = lines[i];

            return array;
        }

        private static string ControlsHelp()
        {
            return
                "Keyboard\r\n" +
                "  F5            start\r\n" +
                "  F6            stop\r\n" +
                "  F7            step one generation\r\n" +
                "  Space         start or stop\r\n" +
                "  Escape        stop\r\n" +
                "  Ctrl+R        randomize\r\n" +
                "  Ctrl+N        new grid\r\n" +
                "  Ctrl+Del      clear\r\n" +
                "  Ctrl+Back     reset to start\r\n" +
                "  Ctrl+G        toggle grid lines\r\n" +
                "  Ctrl+B        toggle the sidebar\r\n" +
                "  Ctrl+W        new world\r\n" +
                "  Ctrl+T        run 100 ecosystem ticks\r\n" +
                "  Ctrl+E        script console\r\n" +
                "  Ctrl+0        reset zoom\r\n" +
                "  Ctrl+plus     zoom in\r\n" +
                "  Ctrl+minus    zoom out\r\n" +
                "  Arrows        scroll the grid\r\n" +
                "  F1            this help\r\n" +
                "\r\n" +
                "Mouse on the grid\r\n" +
                "  Left click    toggle a cell\r\n" +
                "  Left drag     paint\r\n" +
                "  Right drag    pan\r\n" +
                "  Wheel         zoom\r\n";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (m_context.Config.GetBool(ConfigDefaults.SaveSettingsOnExit))
                m_context.SaveSettings();

            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_simulationTimer != null)
                {
                    m_simulationTimer.Stop();
                    m_simulationTimer.Dispose();
                }

                if (m_schedulerTimer != null)
                {
                    m_schedulerTimer.Stop();
                    m_schedulerTimer.Dispose();
                }

                if (m_context != null)
                    m_context.Changed -= OnContextChanged;
            }

            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Draws the menu strip in the dark colours the rest of the window uses.
    /// The default renderer draws a light gradient that clashes with everything
    /// around it.
    /// </summary>
    internal sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer()
            : base(new DarkColours())
        {
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Selected ? Color.White : Color.Gainsboro;

            base.OnRenderItemText(e);
        }
    }

    /// <summary>The colour table the dark menu renderer draws from.</summary>
    internal sealed class DarkColours : ProfessionalColorTable
    {
        private static readonly Color Back = Color.FromArgb(38, 44, 54);
        private static readonly Color Hover = Color.FromArgb(58, 68, 84);
        private static readonly Color Border = Color.FromArgb(70, 80, 96);

        public override Color MenuStripGradientBegin { get { return Back; } }
        public override Color MenuStripGradientEnd { get { return Back; } }
        public override Color MenuItemSelected { get { return Hover; } }
        public override Color MenuItemSelectedGradientBegin { get { return Hover; } }
        public override Color MenuItemSelectedGradientEnd { get { return Hover; } }
        public override Color MenuItemBorder { get { return Border; } }
        public override Color MenuBorder { get { return Border; } }
        public override Color ToolStripDropDownBackground { get { return Back; } }
        public override Color ImageMarginGradientBegin { get { return Back; } }
        public override Color ImageMarginGradientMiddle { get { return Back; } }
        public override Color ImageMarginGradientEnd { get { return Back; } }
        public override Color SeparatorDark { get { return Border; } }
        public override Color SeparatorLight { get { return Back; } }
    }
}
