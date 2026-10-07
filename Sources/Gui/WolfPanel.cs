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

using LifeEngine.Biology;
using LifeEngine.WolfLogic;

namespace LifeEngine.Gui
{
    /// <summary>
    /// The Wolves tab: the pack roster with its dominance order, the alpha pair,
    /// the hunt log, and the controls that move a night or a season on.
    ///
    /// The roster is sorted by fighting power, which is the same order the pack
    /// itself uses, so what is shown is what decides who leads.
    /// </summary>
    public sealed class WolfPanel : UserControl
    {
        private AppContext m_context;

        private ListView m_roster;
        private ListBox m_log;
        private TextBox m_report;
        private Label m_alphaLabel;
        private CheckBox m_fullMoon;
        private NumericUpDown m_nights;

        public WolfPanel(AppContext context)
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
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58.0f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42.0f));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 30.0f));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100.0f));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 38.0f));

            // ---- The alpha banner -----------------------------------------

            m_alphaLabel = new Label();
            m_alphaLabel.Dock = DockStyle.Fill;
            m_alphaLabel.Font = new Font("Segoe UI", 10.0f, FontStyle.Bold);
            m_alphaLabel.ForeColor = Color.Goldenrod;
            m_alphaLabel.TextAlign = ContentAlignment.MiddleLeft;
            m_alphaLabel.Text = "No pack.";

            grid.Controls.Add(m_alphaLabel, 0, 0);
            grid.SetColumnSpan(m_alphaLabel, 2);

            // ---- The roster ------------------------------------------------

            m_roster = new ListView();
            m_roster.Dock = DockStyle.Fill;
            m_roster.View = View.Details;
            m_roster.FullRowSelect = true;
            m_roster.GridLines = true;
            m_roster.BackColor = Color.FromArgb(22, 26, 32);
            m_roster.ForeColor = Color.Gainsboro;
            m_roster.Columns.Add("Rank", 68);
            m_roster.Columns.Add("Name", 96);
            m_roster.Columns.Add("Age", 44);
            m_roster.Columns.Add("Health", 52);
            m_roster.Columns.Add("Energy", 52);
            m_roster.Columns.Add("Str", 38);
            m_roster.Columns.Add("Ago", 38);
            m_roster.Columns.Add("Kills", 40);
            m_roster.Columns.Add("W/L", 46);

            grid.Controls.Add(m_roster, 0, 1);

            // ---- The log and the report ------------------------------------

            SplitContainer right = new SplitContainer();
            right.Dock = DockStyle.Fill;
            right.Orientation = Orientation.Horizontal;
            right.SplitterDistance = 170;
            right.BackColor = Color.FromArgb(30, 34, 40);

            m_log = new ListBox();
            m_log.Dock = DockStyle.Fill;
            m_log.BackColor = Color.FromArgb(22, 26, 32);
            m_log.ForeColor = Color.Gainsboro;
            m_log.Font = new Font("Consolas", 8.0f);

            right.Panel1.Controls.Add(m_log);

            m_report = new TextBox();
            m_report.Dock = DockStyle.Fill;
            m_report.Multiline = true;
            m_report.ReadOnly = true;
            m_report.ScrollBars = ScrollBars.Both;
            m_report.WordWrap = false;
            m_report.Font = new Font("Consolas", 8.0f);
            m_report.BackColor = Color.FromArgb(22, 26, 32);
            m_report.ForeColor = Color.Gainsboro;

            right.Panel2.Controls.Add(m_report);

            grid.Controls.Add(right, 1, 1);

            // ---- The control strip -----------------------------------------

            FlowLayoutPanel strip = new FlowLayoutPanel();
            strip.Dock = DockStyle.Fill;
            strip.FlowDirection = FlowDirection.LeftToRight;
            strip.WrapContents = true;
            strip.BackColor = Color.Transparent;

            m_nights = new NumericUpDown();
            m_nights.Minimum = 1;
            m_nights.Maximum = 10000;
            m_nights.Value = 1;
            m_nights.Width = 60;

            m_fullMoon = new CheckBox();
            m_fullMoon.Text = "Full moon";
            m_fullMoon.ForeColor = Color.Gainsboro;
            m_fullMoon.AutoSize = true;

            strip.Controls.Add(MakeButton("Found pack", delegate
            {
                m_context.FoundPack("The Grey Company", "Fenrir");
            }));

            strip.Controls.Add(new Label
            {
                Text = "Nights",
                ForeColor = Color.Gainsboro,
                AutoSize = true,
                Padding = new Padding(8, 8, 2, 0)
            });

            strip.Controls.Add(m_nights);

            strip.Controls.Add(MakeButton("Run night", delegate
            {
                for (int i = 0; i < (int)m_nights.Value; i++)
                    m_context.RunNight(m_fullMoon.Checked);

                Refresh1();
            }));

            strip.Controls.Add(MakeButton("Run season", delegate
            {
                m_context.RunSeason();
                Refresh1();
            }));

            strip.Controls.Add(m_fullMoon);

            strip.Controls.Add(MakeButton("Report", delegate
            {
                m_report.Text = m_context.DescribePack();
            }));

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

        /// <summary>Rebuilds the roster and the log from the pack.</summary>
        public void Refresh1()
        {
            WolfPack pack = m_context.Pack;

            m_roster.BeginUpdate();
            m_roster.Items.Clear();

            if (pack == null)
            {
                m_roster.EndUpdate();
                m_alphaLabel.Text = "No pack. Use Wolves, Found Pack.";
                m_report.Text = "No pack has been founded yet.";
                RefreshLog();
                return;
            }

            Wolf alpha = pack.Alpha;

            m_alphaLabel.Text = "Alpha: " +
                (alpha == null ? "none" : alpha.Name) +
                "   |   " + pack.Count + " wolves" +
                "   |   cohesion " + Math.Round(pack.Cohesion, 2) +
                "   |   alpha changes " + pack.GenerationNumber;

            List<Wolf> members = new List<Wolf>(pack.Members);

            members.Sort(delegate (Wolf left, Wolf right)
            {
                return right.FightingPower().CompareTo(left.FightingPower());
            });

            for (int i = 0; i < members.Count; i++)
            {
                Wolf wolf = members[i];

                ListViewItem item = new ListViewItem(wolf.Rank.ToString());

                item.SubItems.Add(wolf.Name);
                item.SubItems.Add(Math.Round(wolf.AgeYears, 1).ToString());
                item.SubItems.Add(Math.Round(wolf.Health, 2).ToString());
                item.SubItems.Add(Math.Round(wolf.Energy, 2).ToString());
                item.SubItems.Add(Math.Round(wolf.Traits.Strength, 2).ToString());
                item.SubItems.Add(Math.Round(wolf.Traits.Aggression, 2).ToString());
                item.SubItems.Add(wolf.Kills.ToString());
                item.SubItems.Add(wolf.DominanceWins + "/" + wolf.DominanceLosses);

                if (wolf.Rank == PackRank.Alpha)
                    item.ForeColor = Color.Goldenrod;
                else if (!wolf.IsAlive)
                    item.ForeColor = Color.FromArgb(150, 90, 90);
                else if (wolf.Rank == PackRank.Omega)
                    item.ForeColor = Color.FromArgb(150, 150, 170);

                m_roster.Items.Add(item);
            }

            m_roster.EndUpdate();

            if (string.IsNullOrEmpty(m_report.Text) || m_report.Text.StartsWith("No pack"))
                m_report.Text = pack.Describe();

            RefreshLog();
        }

        /// <summary>Shows the tail of the event log, which is where hunts are recorded.</summary>
        private void RefreshLog()
        {
            IList<string> events = m_context.EventLog;

            int from = Math.Max(0, events.Count - 120);

            m_log.BeginUpdate();
            m_log.Items.Clear();

            for (int i = from; i < events.Count; i++)
                m_log.Items.Add(events[i]);

            if (m_log.Items.Count > 0)
                m_log.TopIndex = m_log.Items.Count - 1;

            m_log.EndUpdate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && m_context != null)
                m_context.Changed -= OnContextChanged;

            base.Dispose(disposing);
        }
    }
}
