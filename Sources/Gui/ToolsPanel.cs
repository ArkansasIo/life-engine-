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
using LifeEngine.Scripting;

namespace LifeEngine.Gui
{
    /// <summary>
    /// The Tools tab: a script console, a settings editor, the scheduler table
    /// and the update list, one behind a set of sub-tabs.
    ///
    /// Each sub-tab is built the same way — a control strip, a list, and a text
    /// pane — so the four read alike rather than each inventing its own layout.
    /// </summary>
    public sealed class ToolsPanel : UserControl
    {
        private AppContext m_context;

        private TabControl m_tabs;

        // Scripts
        private TextBox m_scriptSource;
        private TextBox m_scriptOutput;
        private ComboBox m_scriptNames;

        // Settings
        private ListView m_settings;
        private TextBox m_settingValue;

        // Scheduler
        private ListView m_jobs;

        // Updates
        private TextBox m_updateText;

        public ToolsPanel(AppContext context)
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

            m_tabs = new TabControl();
            m_tabs.Dock = DockStyle.Fill;

            m_tabs.TabPages.Add(BuildScriptTab());
            m_tabs.TabPages.Add(BuildSettingsTab());
            m_tabs.TabPages.Add(BuildSchedulerTab());
            m_tabs.TabPages.Add(BuildUpdateTab());

            Controls.Add(m_tabs);
        }

        // ---- Scripts ------------------------------------------------------

        private TabPage BuildScriptTab()
        {
            TabPage page = NewPage("Scripts");

            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.Orientation = Orientation.Horizontal;
            split.SplitterDistance = 260;

            m_scriptSource = new TextBox();
            m_scriptSource.Dock = DockStyle.Fill;
            m_scriptSource.Multiline = true;
            m_scriptSource.ScrollBars = ScrollBars.Both;
            m_scriptSource.WordWrap = false;
            m_scriptSource.Font = new Font("Consolas", 9.0f);
            m_scriptSource.AcceptsTab = true;
            m_scriptSource.BackColor = Color.FromArgb(22, 26, 32);
            m_scriptSource.ForeColor = Color.Gainsboro;
            m_scriptSource.Text =
                "-- LifeEngine script console.\r\n" +
                "-- Run this, or pick a loaded script above.\r\n" +
                "\r\n" +
                "print(\"grid \" .. gridsize() .. \", gen \" .. generation())\r\n" +
                "\r\n" +
                "class Counter\r\n" +
                "  value = 0\r\n" +
                "\r\n" +
                "  function init(start)\r\n" +
                "    self.value = start\r\n" +
                "  end\r\n" +
                "\r\n" +
                "  function bump()\r\n" +
                "    self.value = self.value + 1\r\n" +
                "    return self.value\r\n" +
                "  end\r\n" +
                "end\r\n" +
                "\r\n" +
                "counter = new Counter(10)\r\n" +
                "counter.bump()\r\n" +
                "print(\"counter is \" .. counter.value)\r\n";

            split.Panel1.Controls.Add(m_scriptSource);

            m_scriptOutput = new TextBox();
            m_scriptOutput.Dock = DockStyle.Fill;
            m_scriptOutput.Multiline = true;
            m_scriptOutput.ReadOnly = true;
            m_scriptOutput.ScrollBars = ScrollBars.Both;
            m_scriptOutput.WordWrap = false;
            m_scriptOutput.Font = new Font("Consolas", 9.0f);
            m_scriptOutput.BackColor = Color.FromArgb(16, 20, 26);
            m_scriptOutput.ForeColor = Color.LightGreen;
            m_scriptOutput.Text = "Output appears here.";

            split.Panel2.Controls.Add(m_scriptOutput);

            FlowLayoutPanel strip = new FlowLayoutPanel();
            strip.Dock = DockStyle.Bottom;
            strip.Height = 34;
            strip.FlowDirection = FlowDirection.LeftToRight;
            strip.WrapContents = false;

            m_scriptNames = new ComboBox();
            m_scriptNames.DropDownStyle = ComboBoxStyle.DropDownList;
            m_scriptNames.Width = 170;

            strip.Controls.Add(m_scriptNames);

            strip.Controls.Add(MakeButton("Run editor", delegate
            {
                ScriptResult result = m_context.RunScriptSource(m_scriptSource.Text, "console");
                ShowScriptResult(result);
            }));

            strip.Controls.Add(MakeButton("Run loaded", delegate
            {
                if (m_scriptNames.SelectedItem == null)
                {
                    m_scriptOutput.Text = "No script selected.";
                    return;
                }

                ScriptResult result = m_context.RunScript(m_scriptNames.SelectedItem.ToString());
                ShowScriptResult(result);
            }));

            strip.Controls.Add(MakeButton("Reload folder", delegate
            {
                int loaded = m_context.Scripts.LoadDirectory();

                m_context.Log("scripts reloaded: " + loaded + " file(s)");
                RefreshScriptNames();

                m_scriptOutput.Text = "Loaded " + loaded + " script(s) from " +
                                      m_context.Scripts.Directory;
            }));

            strip.Controls.Add(MakeButton("Clear output", delegate
            {
                m_scriptOutput.Text = string.Empty;
            }));

            page.Controls.Add(split);
            page.Controls.Add(strip);

            return page;
        }

        private void ShowScriptResult(ScriptResult result)
        {
            if (result == null)
                return;

            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            if (!string.IsNullOrEmpty(result.Output))
                builder.AppendLine(result.Output);

            builder.AppendLine();
            builder.AppendLine("---");
            builder.AppendLine(result.Summary());

            if (!result.Succeeded)
                builder.AppendLine("ERROR: " + result.Error);

            m_scriptOutput.Text = builder.ToString();
        }

        private void RefreshScriptNames()
        {
            string selected = m_scriptNames.SelectedItem as string;

            m_scriptNames.Items.Clear();

            IList<string> names = m_context.Scripts.Names;

            for (int i = 0; i < names.Count; i++)
                m_scriptNames.Items.Add(names[i]);

            if (selected != null && m_scriptNames.Items.Contains(selected))
                m_scriptNames.SelectedItem = selected;
            else if (m_scriptNames.Items.Count > 0)
                m_scriptNames.SelectedIndex = 0;
        }

        // ---- Settings -----------------------------------------------------

        private TabPage BuildSettingsTab()
        {
            TabPage page = NewPage("Settings");

            m_settings = new ListView();
            m_settings.Dock = DockStyle.Fill;
            m_settings.View = View.Details;
            m_settings.FullRowSelect = true;
            m_settings.GridLines = true;
            m_settings.HideSelection = false;
            m_settings.BackColor = Color.FromArgb(22, 26, 32);
            m_settings.ForeColor = Color.Gainsboro;
            m_settings.Columns.Add("Setting", 250);
            m_settings.Columns.Add("Value", 130);
            m_settings.Columns.Add("Type", 60);
            m_settings.Columns.Add("Default", 110);
            m_settings.Columns.Add("What it does", 420);
            m_settings.SelectedIndexChanged += delegate { ShowSelectedSetting(); };
            m_settings.DoubleClick += delegate { EditSelectedSetting(); };

            FlowLayoutPanel strip = new FlowLayoutPanel();
            strip.Dock = DockStyle.Bottom;
            strip.Height = 34;
            strip.FlowDirection = FlowDirection.LeftToRight;
            strip.WrapContents = false;

            m_settingValue = new TextBox();
            m_settingValue.Width = 190;

            strip.Controls.Add(new Label
            {
                Text = "Value",
                ForeColor = Color.Gainsboro,
                AutoSize = true,
                Padding = new Padding(4, 8, 2, 0)
            });

            strip.Controls.Add(m_settingValue);

            strip.Controls.Add(MakeButton("Apply", delegate { EditSelectedSetting(); }));

            strip.Controls.Add(MakeButton("Reset this", delegate
            {
                ConfigEntry entry = SelectedSetting();

                if (entry == null)
                    return;

                entry.Reset();
                RefreshSettings();
            }));

            strip.Controls.Add(MakeButton("Reset all", delegate
            {
                if (MessageBox.Show(this, "Put every setting back to its default?",
                        "Reset settings", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question) == DialogResult.OK)
                {
                    m_context.ResetSettings();
                    RefreshSettings();
                }
            }));

            strip.Controls.Add(MakeButton("Save", delegate
            {
                m_context.SaveSettings();
                RefreshSettings();
            }));

            strip.Controls.Add(MakeButton("Reload", delegate
            {
                m_context.ReloadSettings();
                RefreshSettings();
            }));

            page.Controls.Add(m_settings);
            page.Controls.Add(strip);

            return page;
        }

        private ConfigEntry SelectedSetting()
        {
            if (m_settings.SelectedItems.Count == 0)
                return null;

            string key = m_settings.SelectedItems[0].Tag as string;

            return m_context.Config.Get(key);
        }

        private void ShowSelectedSetting()
        {
            ConfigEntry entry = SelectedSetting();

            if (entry == null)
                return;

            m_settingValue.Text = entry.Value;
        }

        private void EditSelectedSetting()
        {
            ConfigEntry entry = SelectedSetting();

            if (entry == null)
                return;

            if (!entry.SetFromText(m_settingValue.Text))
            {
                MessageBox.Show(this,
                    "\"" + m_settingValue.Text + "\" is not a valid " + entry.TypeName +
                    " for " + entry.Key + ".",
                    "Bad value", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }

            m_context.Log("setting changed: " + entry.Key + " = " + entry.Value);

            RefreshSettings();
        }

        private void RefreshSettings()
        {
            m_settings.BeginUpdate();
            m_settings.Items.Clear();

            IList<ConfigEntry> entries = m_context.Config.Entries;

            for (int i = 0; i < entries.Count; i++)
            {
                ConfigEntry entry = entries[i];

                ListViewItem item = new ListViewItem(entry.Key);

                item.SubItems.Add(entry.Value);
                item.SubItems.Add(entry.TypeName);
                item.SubItems.Add(entry.DefaultValue);
                item.SubItems.Add(entry.Description);

                item.Tag = entry.Key;

                if (entry.IsModified())
                    item.ForeColor = Color.Goldenrod;

                m_settings.Items.Add(item);
            }

            m_settings.EndUpdate();
        }

        // ---- Scheduler ----------------------------------------------------

        private TabPage BuildSchedulerTab()
        {
            TabPage page = NewPage("Scheduler");

            m_jobs = new ListView();
            m_jobs.Dock = DockStyle.Fill;
            m_jobs.View = View.Details;
            m_jobs.FullRowSelect = true;
            m_jobs.GridLines = true;
            m_jobs.CheckBoxes = true;
            m_jobs.BackColor = Color.FromArgb(22, 26, 32);
            m_jobs.ForeColor = Color.Gainsboro;
            m_jobs.Columns.Add("Job", 150);
            m_jobs.Columns.Add("Action", 130);
            m_jobs.Columns.Add("Repeat", 90);
            m_jobs.Columns.Add("Runs", 50);
            m_jobs.Columns.Add("Last run", 140);
            m_jobs.Columns.Add("Next run", 140);
            m_jobs.ItemChecked += OnJobChecked;

            FlowLayoutPanel strip = new FlowLayoutPanel();
            strip.Dock = DockStyle.Bottom;
            strip.Height = 34;
            strip.FlowDirection = FlowDirection.LeftToRight;
            strip.WrapContents = false;

            strip.Controls.Add(MakeButton("Install default jobs", delegate
            {
                m_context.InstallDefaultSchedules();
                RefreshJobs();
            }));

            strip.Controls.Add(MakeButton("Run due now", delegate
            {
                List<Schedule> ran = m_context.RunDueSchedules();

                m_context.Log("scheduler: " + ran.Count + " job(s) ran");
                RefreshJobs();
            }));

            strip.Controls.Add(MakeButton("Run selected", delegate
            {
                if (m_jobs.SelectedItems.Count == 0)
                    return;

                Schedule schedule = m_jobs.SelectedItems[0].Tag as Schedule;

                if (schedule == null)
                    return;

                m_context.ExecuteSchedule(schedule);
                RefreshJobs();
            }));

            strip.Controls.Add(MakeButton("Refresh", delegate { RefreshJobs(); }));

            page.Controls.Add(m_jobs);
            page.Controls.Add(strip);

            return page;
        }

        private void OnJobChecked(object sender, ItemCheckedEventArgs e)
        {
            Schedule schedule = e.Item.Tag as Schedule;

            if (schedule != null)
                schedule.Enabled = e.Item.Checked;
        }

        private void RefreshJobs()
        {
            m_jobs.BeginUpdate();
            m_jobs.Items.Clear();

            IList<Schedule> schedules = m_context.Scheduler.Schedules;

            for (int i = 0; i < schedules.Count; i++)
            {
                Schedule schedule = schedules[i];

                ListViewItem item = new ListViewItem(schedule.Name);

                item.SubItems.Add(schedule.Action.ToString());
                item.SubItems.Add(schedule.Repeat.ToString());
                item.SubItems.Add(schedule.Runs.ToString());

                item.SubItems.Add(schedule.LastRun == DateTime.MinValue
                    ? "never"
                    : schedule.LastRun.ToString("yyyy-MM-dd HH:mm:ss"));

                item.SubItems.Add(schedule.NextRun == DateTime.MaxValue
                    ? "never"
                    : schedule.NextRun.ToString("yyyy-MM-dd HH:mm:ss"));

                item.Checked = schedule.Enabled;
                item.Tag = schedule;

                m_jobs.Items.Add(item);
            }

            m_jobs.EndUpdate();
        }

        // ---- Updates ------------------------------------------------------

        private TabPage BuildUpdateTab()
        {
            TabPage page = NewPage("Updates");

            m_updateText = new TextBox();
            m_updateText.Dock = DockStyle.Fill;
            m_updateText.Multiline = true;
            m_updateText.ReadOnly = true;
            m_updateText.ScrollBars = ScrollBars.Both;
            m_updateText.WordWrap = false;
            m_updateText.Font = new Font("Consolas", 9.0f);
            m_updateText.BackColor = Color.FromArgb(22, 26, 32);
            m_updateText.ForeColor = Color.Gainsboro;
            m_updateText.Text = m_context.Version.Describe();

            FlowLayoutPanel strip = new FlowLayoutPanel();
            strip.Dock = DockStyle.Bottom;
            strip.Height = 34;
            strip.FlowDirection = FlowDirection.LeftToRight;
            strip.WrapContents = false;

            strip.Controls.Add(MakeButton("Load manifest", delegate
            {
                int count = m_context.LoadUpdateManifest();

                m_updateText.Text = count < 0
                    ? "The manifest could not be read.\r\n\r\n" + m_context.Version.Describe()
                    : m_context.DescribeUpdates();
            }));

            strip.Controls.Add(MakeButton("What would apply", delegate
            {
                m_updateText.Text = m_context.DescribeUpdates();
            }));

            strip.Controls.Add(MakeButton("Version info", delegate
            {
                m_updateText.Text = m_context.Version.Describe();
            }));

            page.Controls.Add(m_updateText);
            page.Controls.Add(strip);

            return page;
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static TabPage NewPage(string title)
        {
            TabPage page = new TabPage(title);

            page.BackColor = Color.FromArgb(30, 34, 40);
            page.ForeColor = Color.Gainsboro;
            page.Padding = new Padding(4);

            return page;
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

        /// <summary>Refreshes whichever sub-tab is showing.</summary>
        public void Refresh1()
        {
            RefreshScriptNames();
            RefreshSettings();
            RefreshJobs();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && m_context != null)
                m_context.Changed -= OnContextChanged;

            base.Dispose(disposing);
        }
    }
}
