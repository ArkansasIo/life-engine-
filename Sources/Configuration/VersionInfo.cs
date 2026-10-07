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
using System.Globalization;
using System.Reflection;

namespace LifeEngine.Configuration
{
    /// <summary>
    /// The version the program is, the build it came from and the date it was
    /// produced. Read once from the assembly, then handed to the About box, the
    /// title bar and the settings file, so there is one answer to "which build
    /// is this?" rather than three.
    /// </summary>
    public sealed class VersionInfo
    {
        private Version m_version;
        private string m_build;
        private string m_buildDate;
        private string m_product;
        private string m_company;
        private string m_copyright;

        /// <summary>Major.Minor.Patch.Build, as compiled into the assembly.</summary>
        public Version Version
        {
            get { return m_version; }
        }

        /// <summary>The dotted version, e.g. "1.0.0.0".</summary>
        public string VersionText
        {
            get { return m_version == null ? "0.0.0.0" : m_version.ToString(); }
        }

        /// <summary>Major.Minor.Patch, the part a person quotes.</summary>
        public string ShortVersion
        {
            get
            {
                if (m_version == null)
                    return "0.0.0";

                return m_version.Major + "." + m_version.Minor + "." + m_version.Build;
            }
        }

        /// <summary>A name for this particular build, e.g. "2026.10.07-r1".</summary>
        public string Build
        {
            get { return m_build; }
            set { m_build = value; }
        }

        /// <summary>When this build was produced, as text.</summary>
        public string BuildDate
        {
            get { return m_buildDate; }
            set { m_buildDate = value; }
        }

        public string Product
        {
            get { return m_product; }
        }

        public string Company
        {
            get { return m_company; }
        }

        public string Copyright
        {
            get { return m_copyright; }
        }

        /// <summary>
        /// The running assembly's identity. Falls back to safe text rather than
        /// throwing, because this is called from the About box and the settings
        /// writer where an exception would be worse than a blank field.
        /// </summary>
        public static VersionInfo FromAssembly()
        {
            VersionInfo info = new VersionInfo();

            info.m_version = new Version(0, 0, 0, 0);
            info.m_product = "LifeEngine";
            info.m_company = "ArkansasIo";
            info.m_copyright = "Copyright © 2026 ArkansasIo";
            info.m_build = "local";
            info.m_buildDate = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();

                info.m_version = assembly.GetName().Version;

                info.m_product = ReadAttribute(assembly, typeof(AssemblyProductAttribute)) ?? info.m_product;
                info.m_company = ReadAttribute(assembly, typeof(AssemblyCompanyAttribute)) ?? info.m_company;
                info.m_copyright = ReadAttribute(assembly, typeof(AssemblyCopyrightAttribute)) ?? info.m_copyright;

                info.m_build = BuildVersion.FromDate(info.m_buildDate);
            }
            catch (Exception)
            {
                // Leave the fallbacks in place.
            }

            return info;
        }

        private static string ReadAttribute(Assembly assembly, Type attributeType)
        {
            object[] attributes = assembly.GetCustomAttributes(attributeType, false);

            if (attributes == null || attributes.Length == 0)
                return null;

            AssemblyProductAttribute product = attributes[0] as AssemblyProductAttribute;

            if (product != null)
                return product.Product;

            AssemblyCompanyAttribute company = attributes[0] as AssemblyCompanyAttribute;

            if (company != null)
                return company.Company;

            AssemblyCopyrightAttribute copyright = attributes[0] as AssemblyCopyrightAttribute;

            if (copyright != null)
                return copyright.Copyright;

            return null;
        }

        /// <summary>One line, the sort of thing a title bar or a log shows.</summary>
        public string TitleLine()
        {
            return m_product + " " + ShortVersion;
        }

        /// <summary>Everything known about this build, for the About box.</summary>
        public string Describe()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.AppendLine("Product:    " + m_product);
            builder.AppendLine("Version:    " + VersionText);
            builder.AppendLine("Build:      " + m_build);
            builder.AppendLine("Built:      " + m_buildDate);
            builder.AppendLine("Company:    " + m_company);
            builder.AppendLine("Copyright:  " + m_copyright);
            builder.AppendLine();
            builder.AppendLine("Game of Life is a cellular automata simulator which defaults to the");
            builder.AppendLine("rules for Conway's Game of Life. It can also be set to use several");
            builder.AppendLine("other GoL rulesets.");

            return builder.ToString();
        }

        public override string ToString()
        {
            return TitleLine();
        }
    }

    /// <summary>
    /// Turns a date into a build name, and a build name back into a date. The
    /// name is the date, so a build always says when it was made, and two builds
    /// from the same day are told apart by their revision.
    /// </summary>
    public static class BuildVersion
    {
        /// <summary>A build name for a date, e.g. "2026.10.07".</summary>
        public static string FromDate()
        {
            return FromDate(DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        /// <summary>A build name from "yyyy-MM-dd", e.g. "2026.10.07".</summary>
        public static string FromDate(string date)
        {
            if (string.IsNullOrEmpty(date))
                return "unknown";

            return date.Replace('-', '.').Replace('/', '.');
        }

        /// <summary>A build name with a revision, e.g. "2026.10.07-r3".</summary>
        public static string WithRevision(int revision)
        {
            if (revision < 1)
                return FromDate();

            return FromDate() + "-r" + revision.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Splits a build name into its date and revision. The revision is 0 if
        /// the name does not carry one.
        /// </summary>
        public static bool TryParseBuild(string build, out string date, out int revision)
        {
            date = string.Empty;
            revision = 0;

            if (string.IsNullOrEmpty(build))
                return false;

            string text = build.Trim();
            int dash = text.IndexOf("-r");

            if (dash >= 0)
            {
                string tail = text.Substring(dash + 2);
                text = text.Substring(0, dash);

                if (!int.TryParse(tail, NumberStyles.Integer, CultureInfo.InvariantCulture, out revision))
                    revision = 0;
            }

            date = text;

            return !string.IsNullOrEmpty(date);
        }

        /// <summary>
        /// Compares two builds by their date and revision. Negative if the first
        /// is older, positive if newer, zero if they are the same build.
        /// </summary>
        public static int Compare(string left, string right)
        {
            string leftDate;
            int leftRevision;
            string rightDate;
            int rightRevision;

            TryParseBuild(left, out leftDate, out leftRevision);
            TryParseBuild(right, out rightDate, out rightRevision);

            int dateComparison = string.CompareOrdinal(Normalise(leftDate), Normalise(rightDate));

            if (dateComparison != 0)
                return dateComparison;

            return leftRevision.CompareTo(rightRevision);
        }

        /// <summary>True if the left build is more recent than the right one.</summary>
        public static bool IsNewer(string left, string right)
        {
            return Compare(left, right) > 0;
        }

        private static string Normalise(string date)
        {
            if (string.IsNullOrEmpty(date))
                return "0000.00.00";

            return date.PadLeft(10, '0');
        }
    }
}
