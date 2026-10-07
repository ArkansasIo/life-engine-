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
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace LifeEngine.Data
{
    /// <summary>
    /// A table of rows, each row a dictionary of column name to text. This is
    /// the lowest common shape between a SQL result, an Excel sheet and a CSV
    /// file, so one exporter serves all three.
    /// </summary>
    public sealed class DataTable
    {
        private List<string> m_columns;
        private List<string[]> m_rows;
        private string m_name;

        /// <summary>The sheet or table name.</summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>The column headings, in order.</summary>
        public IList<string> Columns
        {
            get { return m_columns; }
        }

        /// <summary>The rows, each the same length as <see cref="Columns"/>.</summary>
        public IList<string[]> Rows
        {
            get { return m_rows; }
        }

        public int Count
        {
            get { return m_rows.Count; }
        }

        public DataTable(string name)
        {
            m_name = name;
            m_columns = new List<string>();
            m_rows = new List<string[]>();
        }

        /// <summary>Declares the columns. Call before adding any row.</summary>
        public DataTable WithColumns(params string[] columns)
        {
            m_columns.Clear();

            if (columns != null)
                m_columns.AddRange(columns);

            return this;
        }

        /// <summary>Appends a row. Extra values are dropped, missing ones are blank.</summary>
        public void Add(params object[] values)
        {
            string[] row = new string[m_columns.Count];

            for (int i = 0; i < m_columns.Count; i++)
            {
                if (values != null && i < values.Length && values[i] != null)
                    row[i] = Convert(values[i]);
                else
                    row[i] = string.Empty;
            }

            m_rows.Add(row);
        }

        /// <summary>Turns a value into text the way a spreadsheet expects it.</summary>
        public static string Convert(object value)
        {
            if (value == null)
                return string.Empty;

            if (value is bool)
                return ((bool)value) ? "true" : "false";

            if (value is double)
            {
                double number = (double)value;

                if (double.IsNaN(number) || double.IsInfinity(number))
                    return string.Empty;

                return number.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
            }

            if (value is float)
                return ((float)value).ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);

            if (value is DateTime)
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");

            if (value is IFormattable)
                return ((IFormattable)value).ToString(null, System.Globalization.CultureInfo.InvariantCulture);

            return value.ToString();
        }

        public override string ToString()
        {
            return m_name + " (" + m_rows.Count + " rows x " + m_columns.Count + " columns)";
        }
    }

    /// <summary>
    /// Writes a <see cref="DataTable"/> out as a spreadsheet or a CSV file.
    ///
    /// The Excel writer uses the Jet provider, which is part of the Windows
    /// installation on any machine that has Office or the Access runtime. Where
    /// it is not present, the caller gets a clear failure and can fall back to
    /// CSV, which needs nothing but the file system.
    /// </summary>
    public static class SpreadsheetWriter
    {
        /// <summary>
        /// Writes one or more tables to an .xlsx or .xls file, one worksheet
        /// each. Returns a list of problems; empty means it worked.
        /// </summary>
        public static List<string> WriteWorkbook(string path, IList<DataTable> tables)
        {
            List<string> problems = new List<string>();

            if (string.IsNullOrEmpty(path))
            {
                problems.Add("No output path was given.");
                return problems;
            }

            if (tables == null || tables.Count == 0)
            {
                problems.Add("There is nothing to write.");
                return problems;
            }

            try
            {
                string directory = Path.GetDirectoryName(path);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string extension = Path.GetExtension(path);

                if (string.IsNullOrEmpty(extension))
                {
                    path = path + ".xls";
                    extension = ".xls";
                }

                // Jet only knows the older format, so the connection string
                // says so whatever the file is called.
                string connectionString =
                    "Provider=Microsoft.Jet.OLEDB.4.0;" +
                    "Data Source=" + path + ";" +
                    "Extended Properties=\"Excel 8.0;HDR=YES;\";";

                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    for (int t = 0; t < tables.Count; t++)
                        WriteSheet(connection, tables[t], problems);
                }
            }
            catch (Exception error)
            {
                problems.Add("Could not write " + path + ": " + error.Message);
                problems.Add("If the Jet provider is not installed, export to CSV instead.");
            }

            return problems;
        }

        private static void WriteSheet(OleDbConnection connection, DataTable table, List<string> problems)
        {
            if (table == null || table.Columns.Count == 0)
                return;

            string sheet = SanitiseSheetName(table.Name);

            try
            {
                // CREATE TABLE makes the worksheet, with a text column per
                // heading. Excel decides the real types from the data.
                using (OleDbCommand create = connection.CreateCommand())
                {
                    create.CommandText = BuildCreateTable(sheet, table);
                    create.ExecuteNonQuery();
                }

                for (int r = 0; r < table.Rows.Count; r++)
                {
                    using (OleDbCommand insert = connection.CreateCommand())
                    {
                        insert.CommandText = BuildInsert(sheet, table, r);
                        insert.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception error)
            {
                problems.Add("Sheet \"" + sheet + "\": " + error.Message);
            }
        }

        private static string BuildCreateTable(string sheet, DataTable table)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.Append("CREATE TABLE [");
            builder.Append(sheet);
            builder.Append("] (");

            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");

                builder.Append("[");
                builder.Append(SanitiseColumnName(table.Columns[i]));
                builder.Append("] Text");
            }

            builder.Append(")");

            return builder.ToString();
        }

        private static string BuildInsert(string sheet, DataTable table, int rowIndex)
        {
            string[] row = table.Rows[rowIndex];

            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.Append("INSERT INTO [");
            builder.Append(sheet);
            builder.Append("] (");

            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");

                builder.Append("[");
                builder.Append(SanitiseColumnName(table.Columns[i]));
                builder.Append("]");
            }

            builder.Append(") VALUES (");

            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");

                // A string literal: quotes doubled, which is how Jet escapes them.
                builder.Append("'");
                builder.Append(row[i].Replace("'", "''"));
                builder.Append("'");
            }

            builder.Append(")");

            return builder.ToString();
        }

        /// <summary>Excel sheet names cannot hold these characters or exceed 31.</summary>
        private static string SanitiseSheetName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "Sheet";

            char[] bad = { '\\', '/', '*', '?', ':', '[', ']' };

            string clean = name;

            for (int i = 0; i < bad.Length; i++)
                clean = clean.Replace(bad[i], '_');

            if (clean.Length > 31)
                clean = clean.Substring(0, 31);

            return clean;
        }

        private static string SanitiseColumnName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "Column";

            return name.Replace("]", "_").Replace("[", "_");
        }

        /// <summary>
        /// Writes a table as CSV. This is the export that always works, because
        /// it needs nothing but a file stream.
        /// </summary>
        public static List<string> WriteCsv(string path, DataTable table)
        {
            List<string> problems = new List<string>();

            if (string.IsNullOrEmpty(path) || table == null)
            {
                problems.Add("Nothing to write.");
                return problems;
            }

            try
            {
                string directory = Path.GetDirectoryName(path);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                System.Text.StringBuilder builder = new System.Text.StringBuilder();

                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if (i > 0)
                        builder.Append(',');

                    builder.Append(Quote(table.Columns[i]));
                }

                builder.AppendLine();

                for (int r = 0; r < table.Rows.Count; r++)
                {
                    string[] row = table.Rows[r];

                    for (int i = 0; i < row.Length; i++)
                    {
                        if (i > 0)
                            builder.Append(',');

                        builder.Append(Quote(row[i]));
                    }

                    builder.AppendLine();
                }

                File.WriteAllText(path, builder.ToString());
            }
            catch (Exception error)
            {
                problems.Add("Could not write " + path + ": " + error.Message);
            }

            return problems;
        }

        /// <summary>Wraps a value in quotes if it holds a comma, a quote or a newline.</summary>
        private static string Quote(string value)
        {
            if (value == null)
                return "\"\"";

            if (value.IndexOf(',') < 0 && value.IndexOf('"') < 0 &&
                value.IndexOf('\n') < 0 && value.IndexOf('\r') < 0)
                return value;

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
