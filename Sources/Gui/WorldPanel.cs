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

using LifeEngine.Ecosystem;

namespace LifeEngine.Gui
{
    /// <summary>
    /// The World and Ecosystem tab: the map, the species table, and the
    /// controls that drive them.
    ///
    /// The panel is rebuilt from the world every time it is refreshed rather
    /// than being incrementally updated, because the world can be replaced
    /// wholesale by a new seed and incremental updates would then be wrong.
    /// </summary>
    public sealed class WorldPanel : UserControl
    {
        private AppContext m_context;

        private WorldMapView m_map;
        private ListView m_speciesList;
        private TextBox m_summary;
        private Label m_tileInfo;
        private CheckBox m_showLife;
        private CheckBox m_showRivers;
        private NumericUpDown m_ticks;
        private Button m_runTicks;
        private Button m_stepTick;
        private Button m_clear;

        public WorldPanel(AppContext context)
        {
            m_context = context;

            BuildLayout();

            m_context.Changed += OnContextChanged;

            Refresh1();
        }

        private void OnContextChanged(object sender, EventArgs e)
        {
            Refresh1();
        }

        // ------------------------------------------------------------------
        // Layout
        // ------------------------------------------------------------------

        private void BuildLayout()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(30, 34, 40);
            ForeColor = Color.Gainsboro;
            Padding = new Padding(6);

            TableLayoutPanel grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Fill;
            grid.ColumnCount = 2;
            grid.RowCount = 3;
            grid.BackColor = Color.Transparent;
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62.0f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38.0f));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100.0f));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 26.0f));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 34.0f));

            // ---- The map --------------------------------------------------

            m_map = new WorldMapView(m_context);
            m_map.Dock = DockStyle.Fill;
            m_map.TileSelected += delegate { UpdateTileInfo(); };
            grid.Controls.Add(m_map, 0, 0);

            // ---- The species list and summary -----------------------------

            SplitContainer right = new SplitContainer();
            right.Dock = DockStyle.Fill;
            right.Orientation = Orientation.Horizontal;
            right.SplitterDistance = 150;
            right.BackColor = Color.FromArgb(30, 34, 40);

            m_speciesList = new ListView();
            m_speciesList.Dock = DockStyle.Fill;
            m_speciesList.View = View.Details;
            m_speciesList.FullRowSelect = true;
            m_speciesList.GridLines = true;
            m_speciesList.BackColor = Color.FromArgb(22, 26, 32);
            m_speciesList.ForeColor = Color.Gainsboro;
            m_speciesList.Columns.Add("Species", 118);
            m_speciesList.Columns.Add("Level", 92);
            m_speciesList.Columns.Add("Head", 56);
            m_speciesList.Columns.Add("Tiles", 48);

            right.Panel1.Controls.Add(m_speciesList);

            m_summary = new TextBox();
            m_summary.Dock = DockStyle.Fill;
            m_summary.Multiline = true;
            m_summary.ReadOnly = true;
            m_summary.ScrollBars = ScrollBars.Vertical;
            m_summary.WordWrap = false;
            m_summary.Font = new Font("Consolas", 8.0f);
            m_summary.BackColor = Color.FromArgb(22, 26, 32);
            m_summary.ForeColor = Color.Gainsboro;

            right.Panel2.Controls.Add(m_summary);

            grid.Controls.Add(right, 1, 0);

            // ---- The tile readout -----------------------------------------

            m_tileInfo = new Label();
            m_tileInfo.Dock = DockStyle.Fill;
            m_tileInfo.TextAlign = ContentAlignment.MiddleLeft;
            m_tileInfo.ForeColor = Color.Silver;
            m_tileInfo.Font = new Font("Consolas", 8.0f);
            m_tileInfo.Text = "Click the map for a tile.";

            grid.Controls.Add(m_tileInfo, 0, 1);
            grid.SetColumnSpan(m_tileInfo, 2);

            // ---- The control strip ----------------------------------------

            FlowLayoutPanel strip = new FlowLayoutPanel();
            strip.Dock = DockStyle.Fill;
            strip.FlowDirection = FlowDirection.LeftToRight;
            strip.WrapContents = false;
            strip.BackColor = Color.Transparent;

            m_showLife = new CheckBox();
            m_showLife.Text = "Show life";
            m_showLife.Checked = true;
            m_showLife.ForeColor = Color.Gainsboro;
            m_showLife.AutoSize = true;
            m_showLife.CheckedChanged += delegate
            {
                m_map.ShowLife = m_showLife.Checked;
                UpdateTileInfo();
            };

            m_showRivers = new CheckBox();
            m_showRivers.Text = "Show rivers";
            m_showRivers.Checked = true;
            m_showRivers.ForeColor = Color.Gainsboro;
            m_showRivers.AutoSize = true;
            m_showRivers.CheckedChanged += delegate { m_map.ShowRivers = m_showRivers.Checked; };

            m_ticks = new NumericUpDown();
            m_ticks.Minimum = 1;
            m_ticks.Maximum = 100000;
            m_ticks.Value = 100;
            m_ticks.Width = 70;

            m_runTicks = MakeButton("Run ticks", delegate
            {
                m_context.RunEcosystem((int)m_ticks.Value);
            });

            m_stepTick = MakeButton("Step tick", delegate
            {
                m_context.RunEcosystem(1);
            });

            m_clear = MakeButton("Clear life", delegate
            {
                m_context.ClearEcosystem();
            });

            strip.Controls.Add(m_showLife);
            strip.Controls.Add(m_showRivers);
            strip.Controls.Add(new Label { Text = "Ticks", ForeColor = Color.Gainsboro, AutoSize = true, Padding = new Padding(8, 8, 2, 0) });
            strip.Controls.Add(m_ticks);
            strip.Controls.Add(m_runTicks);
            strip.Controls.Add(m_stepTick);
            strip.Controls.Add(m_clear);

            grid.Controls.Add(strip, 0, 2);
            grid.SetColumnSpan(strip, 2);

            Controls.Add(grid);
        }

        private static Button MakeButton(string text, EventHandler onClick)
        {
            Button button = new Button();

            button.Text = text;
            button.AutoSize = true;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.FromArgb(52, 58, 68);
            button.ForeColor = Color.Gainsboro;
            button.Click += onClick;

            return button;
        }

        // ------------------------------------------------------------------
        // Refresh
        // ------------------------------------------------------------------

        /// <summary>Rebuilds the species table and the summary from the world.</summary>
        public void Refresh1()
        {
            m_map.Invalidate();

            Ecosystem.Ecosystem ecosystem = m_context.Ecosystem;

            m_speciesList.BeginUpdate();
            m_speciesList.Items.Clear();

            if (ecosystem == null)
            {
                m_speciesList.EndUpdate();
                m_summary.Text = "No world has been built yet.\r\n\r\nWorld menu, New World.";
                return;
            }

            Biology.WorldMap world = ecosystem.World;

            int columns = world.Columns;
            int rows = world.Rows;

            for (int i = 0; i < ecosystem.Species.Count; i++)
            {
                Ecosystem.Species species = ecosystem.Species[i];

                int head = ecosystem.GetPopulation(species);
                int tiles = CountTiles(world, species);

                ListViewItem item = new ListViewItem(species.Name);
                item.SubItems.Add(species.TrophicLevel.ToString());
                item.SubItems.Add(head.ToString());
                item.SubItems.Add(tiles.ToString());

                if (head <= 0)
                    item.ForeColor = Color.FromArgb(150, 90, 90);

                m_speciesList.Items.Add(item);
            }

            m_speciesList.EndUpdate();

            m_summary.Text = ecosystem.Summarise();
        }

        private static int CountTiles(Biology.WorldMap world, Ecosystem.Species species)
        {
            int count = 0;

            foreach (Biology.Tile tile in world.Tiles)
            {
                if (tile.GetPopulation(species) > 0)
                    count++;
            }

            return count;
        }

        private void UpdateTileInfo()
        {
            m_tileInfo.Text = m_map.DescribeSelection();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && m_context != null)
                m_context.Changed -= OnContextChanged;

            base.Dispose(disposing);
        }
    }
}
