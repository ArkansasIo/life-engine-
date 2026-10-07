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
using System.Text;

namespace LifeEngine.Configuration
{
    /// <summary>
    /// One setting: a name, a category, a value and the rules about what the
    /// value may be. Everything the program can be configured with is described
    /// here rather than being a bare field, so a settings file can be validated
    /// and a settings editor can be built without special cases.
    /// </summary>
    public sealed class ConfigEntry
    {
        private string m_key;
        private string m_value;
        private string m_defaultValue;
        private string m_category;
        private string m_description;
        private string m_typeName;
        private double m_minimum;
        private double m_maximum;
        private bool m_hasRange;

        /// <summary>The name the setting is known by, e.g. "Simulation.Speed".</summary>
        public string Key
        {
            get { return m_key; }
        }

        /// <summary>The value as text. Use the Get* methods for typed reads.</summary>
        public string Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        /// <summary>The value the setting shipped with, used when resetting.</summary>
        public string DefaultValue
        {
            get { return m_defaultValue; }
        }

        /// <summary>The group the setting belongs to, e.g. "Simulation".</summary>
        public string Category
        {
            get { return m_category; }
        }

        /// <summary>A sentence explaining what the setting does.</summary>
        public string Description
        {
            get { return m_description; }
        }

        /// <summary>"bool", "int", "double", "string" or "color".</summary>
        public string TypeName
        {
            get { return m_typeName; }
        }

        /// <summary>Smallest allowed number, for numeric settings.</summary>
        public double Minimum
        {
            get { return m_minimum; }
        }

        /// <summary>Largest allowed number, for numeric settings.</summary>
        public double Maximum
        {
            get { return m_maximum; }
        }

        /// <summary>True if this setting has a numeric range to clamp to.</summary>
        public bool HasRange
        {
            get { return m_hasRange; }
        }

        public ConfigEntry(string key, string category, string typeName, string defaultValue, string description)
        {
            m_key = key;
            m_category = category;
            m_typeName = typeName;
            m_defaultValue = defaultValue;
            m_value = defaultValue;
            m_description = description;
            m_minimum = 0.0;
            m_maximum = 0.0;
            m_hasRange = false;
        }

        /// <summary>Gives a numeric setting a range to clamp to.</summary>
        public ConfigEntry WithRange(double minimum, double maximum)
        {
            m_minimum = minimum;
            m_maximum = maximum;
            m_hasRange = true;

            return this;
        }

        // ------------------------------------------------------------------
        // Typed reads
        // ------------------------------------------------------------------

        public bool GetBool()
        {
            bool parsed;

            if (bool.TryParse(m_value, out parsed))
                return parsed;

            return ParseBool(m_defaultValue);
        }

        public int GetInt()
        {
            int parsed;

            if (int.TryParse(m_value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                return Clamp(parsed);

            return Clamp(ParseInt(m_defaultValue));
        }

        public double GetDouble()
        {
            double parsed;

            if (double.TryParse(m_value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                return Clamp(parsed);

            return Clamp(ParseDouble(m_defaultValue));
        }

        public string GetString()
        {
            return m_value ?? string.Empty;
        }

        public Color GetColor()
        {
            Color colour;

            if (TryParseColor(m_value, out colour))
                return colour;

            if (TryParseColor(m_defaultValue, out colour))
                return colour;

            return Color.Black;
        }

        // ------------------------------------------------------------------
        // Typed writes
        // ------------------------------------------------------------------

        public void Set(bool value)
        {
            m_value = value ? "true" : "false";
        }

        public void Set(int value)
        {
            m_value = Clamp(value).ToString(CultureInfo.InvariantCulture);
        }

        public void Set(double value)
        {
            m_value = Clamp(value).ToString(CultureInfo.InvariantCulture);
        }

        public void Set(string value)
        {
            m_value = value ?? string.Empty;
        }

        public void Set(Color value)
        {
            m_value = ToColorString(value);
        }

        /// <summary>
        /// Stores a value from text, refusing anything that does not fit the
        /// declared type or range. Returns false if the text was rejected, in
        /// which case the old value is left alone.
        /// </summary>
        public bool SetFromText(string text)
        {
            if (text == null)
                return false;

            switch (m_typeName)
            {
                case "bool":
                    if (text != "true" && text != "false")
                        return false;
                    break;

                case "int":
                    {
                        int parsed;

                        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                            return false;

                        text = Clamp(parsed).ToString(CultureInfo.InvariantCulture);
                        break;
                    }

                case "double":
                    {
                        double parsed;

                        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                            return false;

                        text = Clamp(parsed).ToString(CultureInfo.InvariantCulture);
                        break;
                    }

                case "color":
                    {
                        Color parsed;

                        if (!TryParseColor(text, out parsed))
                            return false;

                        text = ToColorString(parsed);
                        break;
                    }
            }

            m_value = text;

            return true;
        }

        /// <summary>Puts the setting back to the value it shipped with.</summary>
        public void Reset()
        {
            m_value = m_defaultValue;
        }

        /// <summary>True if the value has been changed from the default.</summary>
        public bool IsModified()
        {
            return !string.Equals(m_value, m_defaultValue, StringComparison.Ordinal);
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private int Clamp(int value)
        {
            if (!m_hasRange)
                return value;

            if (value < (int)m_minimum)
                return (int)m_minimum;

            if (value > (int)m_maximum)
                return (int)m_maximum;

            return value;
        }

        private double Clamp(double value)
        {
            if (!m_hasRange)
                return value;

            if (value < m_minimum)
                return m_minimum;

            if (value > m_maximum)
                return m_maximum;

            return value;
        }

        private static bool ParseBool(string text)
        {
            return string.Equals(text, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static int ParseInt(string text)
        {
            int parsed;

            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                return parsed;

            return 0;
        }

        private static double ParseDouble(string text)
        {
            double parsed;

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                return parsed;

            return 0.0;
        }

        /// <summary>Accepts a colour name or "#rrggbb" / "#aarrggbb".</summary>
        public static bool TryParseColor(string text, out Color colour)
        {
            colour = Color.Black;

            if (string.IsNullOrEmpty(text))
                return false;

            string trimmed = text.Trim();

            if (trimmed.StartsWith("#"))
            {
                try
                {
                    colour = ColorTranslator.FromHtml(trimmed);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            Color known = Color.FromName(trimmed);

            // Color.FromName yields a zero-alpha colour for an unknown name.
            if (known.A == 0 && known.R == 0 && known.G == 0 && known.B == 0 &&
                !string.Equals(trimmed, "Black", StringComparison.OrdinalIgnoreCase))
                return false;

            colour = known;

            return true;
        }

        public static string ToColorString(Color colour)
        {
            return "#" + colour.R.ToString("x2") + colour.G.ToString("x2") + colour.B.ToString("x2");
        }

        public override string ToString()
        {
            return m_key + " = " + m_value;
        }
    }

    /// <summary>
    /// The whole configuration: a flat set of named entries, each in a category.
    /// Missing entries fall back to the default, so an old or partial file still
    /// loads and a new setting never breaks a run.
    /// </summary>
    public sealed class Config
    {
        private Dictionary<string, ConfigEntry> m_entries;
        private List<ConfigEntry> m_order;

        public Config()
        {
            m_entries = new Dictionary<string, ConfigEntry>(StringComparer.OrdinalIgnoreCase);
            m_order = new List<ConfigEntry>();
        }

        /// <summary>Every entry, in the order it was declared.</summary>
        public IList<ConfigEntry> Entries
        {
            get { return m_order; }
        }

        public int Count
        {
            get { return m_order.Count; }
        }

        // ------------------------------------------------------------------
        // Declaring settings
        // ------------------------------------------------------------------

        public ConfigEntry Add(string key, string category, string typeName, string defaultValue, string description)
        {
            ConfigEntry entry = new ConfigEntry(key, category, typeName, defaultValue, description);

            m_entries[key] = entry;
            m_order.Add(entry);

            return entry;
        }

        public ConfigEntry AddBool(string key, string category, bool defaultValue, string description)
        {
            return Add(key, category, "bool", defaultValue ? "true" : "false", description);
        }

        public ConfigEntry AddInt(string key, string category, int defaultValue, int minimum, int maximum, string description)
        {
            return Add(key, category, "int", defaultValue.ToString(CultureInfo.InvariantCulture), description)
                .WithRange(minimum, maximum);
        }

        public ConfigEntry AddDouble(string key, string category, double defaultValue, double minimum, double maximum, string description)
        {
            return Add(key, category, "double", defaultValue.ToString(CultureInfo.InvariantCulture), description)
                .WithRange(minimum, maximum);
        }

        public ConfigEntry AddString(string key, string category, string defaultValue, string description)
        {
            return Add(key, category, "string", defaultValue, description);
        }

        public ConfigEntry AddColor(string key, string category, string defaultValue, string description)
        {
            return Add(key, category, "color", defaultValue, description);
        }

        // ------------------------------------------------------------------
        // Access
        // ------------------------------------------------------------------

        public ConfigEntry Get(string key)
        {
            ConfigEntry entry;

            if (key != null && m_entries.TryGetValue(key, out entry))
                return entry;

            return null;
        }

        public ConfigEntry this[string key]
        {
            get { return Get(key); }
        }

        public bool Has(string key)
        {
            return Get(key) != null;
        }

        public bool GetBool(string key)
        {
            ConfigEntry entry = Get(key);

            if (entry == null)
                throw new KeyNotFoundException("No such setting: " + key);

            return entry.GetBool();
        }

        public int GetInt(string key)
        {
            ConfigEntry entry = Get(key);

            if (entry == null)
                throw new KeyNotFoundException("No such setting: " + key);

            return entry.GetInt();
        }

        public double GetDouble(string key)
        {
            ConfigEntry entry = Get(key);

            if (entry == null)
                throw new KeyNotFoundException("No such setting: " + key);

            return entry.GetDouble();
        }

        public string GetString(string key)
        {
            ConfigEntry entry = Get(key);

            if (entry == null)
                throw new KeyNotFoundException("No such setting: " + key);

            return entry.GetString();
        }

        public Color GetColor(string key)
        {
            ConfigEntry entry = Get(key);

            if (entry == null)
                throw new KeyNotFoundException("No such setting: " + key);

            return entry.GetColor();
        }

        public void Set(string key, string value)
        {
            ConfigEntry entry = Get(key);

            if (entry != null)
                entry.SetFromText(value);
        }

        public void SetBool(string key, bool value)
        {
            ConfigEntry entry = Get(key);

            if (entry != null)
                entry.Set(value);
        }

        public void SetInt(string key, int value)
        {
            ConfigEntry entry = Get(key);

            if (entry != null)
                entry.Set(value);
        }

        public void SetDouble(string key, double value)
        {
            ConfigEntry entry = Get(key);

            if (entry != null)
                entry.Set(value);
        }

        public void SetString(string key, string value)
        {
            ConfigEntry entry = Get(key);

            if (entry != null)
                entry.Set(value);
        }

        public void SetColor(string key, Color value)
        {
            ConfigEntry entry = Get(key);

            if (entry != null)
                entry.Set(value);
        }

        // ------------------------------------------------------------------
        // Bulk operations
        // ------------------------------------------------------------------

        /// <summary>Puts every entry back to its default.</summary>
        public void ResetAll()
        {
            for (int i = 0; i < m_order.Count; i++)
                m_order[i].Reset();
        }

        /// <summary>The categories present, in first-declared order.</summary>
        public List<string> GetCategories()
        {
            List<string> categories = new List<string>();

            for (int i = 0; i < m_order.Count; i++)
            {
                if (!categories.Contains(m_order[i].Category))
                    categories.Add(m_order[i].Category);
            }

            return categories;
        }

        /// <summary>Every entry in a category.</summary>
        public List<ConfigEntry> GetByCategory(string category)
        {
            List<ConfigEntry> result = new List<ConfigEntry>();

            for (int i = 0; i < m_order.Count; i++)
            {
                if (string.Equals(m_order[i].Category, category, StringComparison.OrdinalIgnoreCase))
                    result.Add(m_order[i]);
            }

            return result;
        }

        /// <summary>A plain-text dump of every setting, grouped by category.</summary>
        public string Describe()
        {
            StringBuilder builder = new StringBuilder();

            List<string> categories = GetCategories();

            for (int c = 0; c < categories.Count; c++)
            {
                builder.AppendLine("[" + categories[c] + "]");

                List<ConfigEntry> entries = GetByCategory(categories[c]);

                for (int e = 0; e < entries.Count; e++)
                {
                    ConfigEntry entry = entries[e];
                    string marker = entry.IsModified() ? " *" : string.Empty;

                    builder.AppendLine("  " + Pad(entry.Key, 34) + entry.Value + marker);
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static string Pad(string text, int width)
        {
            if (text == null)
                text = string.Empty;

            if (text.Length >= width)
                return text + " ";

            return text + new string(' ', width - text.Length);
        }

        public override string ToString()
        {
            return "Config (" + m_order.Count + " settings)";
        }
    }
}
