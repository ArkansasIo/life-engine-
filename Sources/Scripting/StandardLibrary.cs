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
    /// The functions a script gets for free: print, the maths, the string
    /// helpers, and the table functions.
    ///
    /// Every one of them is a plain C# method matching
    /// <see cref="NativeFunction"/>, registered under a name in the globals.
    /// </summary>
    public static class StandardLibrary
    {
        /// <summary>Registers every built-in on an interpreter.</summary>
        public static void Install(Interpreter interpreter)
        {
            if (interpreter == null)
                return;

            Scope globals = interpreter.Globals;

            globals.Declare("print", Value.Native(Print));
            globals.Declare("len", Value.Native(Len));
            globals.Declare("type", Value.Native(TypeOf));
            globals.Declare("tostring", Value.Native(ToString));
            globals.Declare("tonumber", Value.Native(ToNumber));
            globals.Declare("abs", Value.Native(Abs));
            globals.Declare("floor", Value.Native(Floor));
            globals.Declare("ceil", Value.Native(Ceil));
            globals.Declare("round", Value.Native(Round));
            globals.Declare("sqrt", Value.Native(Sqrt));
            globals.Declare("pow", Value.Native(Pow));
            globals.Declare("min", Value.Native(Min));
            globals.Declare("max", Value.Native(Max));
            globals.Declare("clamp", Value.Native(Clamp));
            globals.Declare("sin", Value.Native(Sin));
            globals.Declare("cos", Value.Native(Cos));
            globals.Declare("tan", Value.Native(Tan));
            globals.Declare("atan2", Value.Native(Atan2));
            globals.Declare("random", Value.Native(Random));
            globals.Declare("randomseed", Value.Native(RandomSeed));

            globals.Declare("upper", Value.Native(Upper));
            globals.Declare("lower", Value.Native(Lower));
            globals.Declare("sub", Value.Native(Substring));
            globals.Declare("find", Value.Native(Find));
            globals.Declare("rep", Value.Native(Repeat));
            globals.Declare("split", Value.Native(Split));
            globals.Declare("join", Value.Native(Join));
            globals.Declare("format", Value.Native(Format));

            globals.Declare("append", Value.Native(Append));
            globals.Declare("keys", Value.Native(Keys));
            globals.Declare("count", Value.Native(Count));
            globals.Declare("remove", Value.Native(Remove));
            globals.Declare("copy", Value.Native(Copy));
            globals.Declare("sort", Value.Native(Sort));

            globals.Declare("math", BuildMathTable());
            globals.Declare("string", BuildStringTable());
            globals.Declare("table", BuildTableTable());
        }

        // ------------------------------------------------------------------
        // Output and conversion
        // ------------------------------------------------------------------

        /// <summary>print(...) joins its arguments with spaces and a newline.</summary>
        public static Value Print(Interpreter interpreter, List<Value> arguments)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            for (int i = 0; i < arguments.Count; i++)
            {
                if (i > 0)
                    builder.Append(' ');

                builder.Append(arguments[i].AsText());
            }

            if (interpreter != null)
                interpreter.Output.Add(builder.ToString());

            return Value.Nil();
        }

        public static Value Len(Interpreter interpreter, List<Value> arguments)
        {
            Value value = Argument(arguments, 0);

            if (value.Kind == ValueKind.Text)
                return Value.Of((double)value.Text.Length);

            return Value.Of((double)value.Length);
        }

        public static Value TypeOf(Interpreter interpreter, List<Value> arguments)
        {
            return Value.Of(Argument(arguments, 0).Kind.ToString().ToLowerInvariant());
        }

        public static Value ToString(Interpreter interpreter, List<Value> arguments)
        {
            return Value.Of(Argument(arguments, 0).AsText());
        }

        public static Value ToNumber(Interpreter interpreter, List<Value> arguments)
        {
            Value value = Argument(arguments, 0);

            try
            {
                return Value.Of(value.AsNumber());
            }
            catch (ScriptError)
            {
                return Value.Nil();
            }
        }

        // ------------------------------------------------------------------
        // Maths
        // ------------------------------------------------------------------

        public static Value Abs(Interpreter interpreter, List<Value> arguments)
        {
            return Value.Of(Math.Abs(Argument(arguments, 0).AsNumber()));
        }

        public static Value Floor(Interpreter interpreter, List<Value> arguments)
        {
            return Value.Of(Math.Floor(Argument(arguments, 0).AsNumber()));
        }

        public static Value Ceil(Interpreter interpreter, List<Value> arguments)
        {
            return Value.Of(Math.Ceiling(Argument(arguments, 0).AsNumber()));
        }

        public static Value Round(Interpreter interpreter, List<Value> arguments)
        {
            double value = Argument(arguments, 0).AsNumber();
            int places = arguments.Count > 1 ? (int)arguments[1].AsNumber() : 0;

            return Value.Of(Math.Round(value, Math.Max(0, Math.Min(15, places))));
        }

        public static Value Sqrt(Interpreter interpreter, List<Value> arguments)
        {
            double value = Argument(arguments, 0).AsNumber();

            if (value < 0.0)
                return Value.Of(0.0);

            return Value.Of(Math.Sqrt(value));
        }

        public static Value Pow(Interpreter interpreter, List<Value> arguments)
        {
            double left = Argument(arguments, 0).AsNumber();
            double right = Argument(arguments, 1).AsNumber();

            return Value.Of(Math.Pow(left, right));
        }

        public static Value Min(Interpreter interpreter, List<Value> arguments)
        {
            if (arguments.Count == 0)
                return Value.Nil();

            double smallest = arguments[0].AsNumber();

            for (int i = 1; i < arguments.Count; i++)
            {
                double value = arguments[i].AsNumber();

                if (value < smallest)
                    smallest = value;
            }

            return Value.Of(smallest);
        }

        public static Value Max(Interpreter interpreter, List<Value> arguments)
        {
            if (arguments.Count == 0)
                return Value.Nil();

            double largest = arguments[0].AsNumber();

            for (int i = 1; i < arguments.Count; i++)
            {
                double value = arguments[i].AsNumber();

                if (value > largest)
                    largest = value;
            }

            return Value.Of(largest);
        }

        public static Value Clamp(Interpreter interpreter, List<Value> arguments)
        {
            double value = Argument(arguments, 0).AsNumber();
            double minimum = Argument(arguments, 1).AsNumber();
            double maximum = Argument(arguments, 2).AsNumber();

            if (value < minimum)
                return Value.Of(minimum);

            if (value > maximum)
                return Value.Of(maximum);

            return Value.Of(value);
        }

        public static Value Sin(Interpreter interpreter, List<Value> arguments)
        {
            return Value.Of(Math.Sin(Argument(arguments, 0).AsNumber()));
        }

        public static Value Cos(Interpreter interpreter, List<Value> arguments)
        {
            return Value.Of(Math.Cos(Argument(arguments, 0).AsNumber()));
        }

        public static Value Tan(Interpreter interpreter, List<Value> arguments)
        {
            return Value.Of(Math.Tan(Argument(arguments, 0).AsNumber()));
        }

        public static Value Atan2(Interpreter interpreter, List<Value> arguments)
        {
            double y = Argument(arguments, 0).AsNumber();
            double x = Argument(arguments, 1).AsNumber();

            return Value.Of(Math.Atan2(y, x));
        }

        /// <summary>random() is 0 to 1; random(n) is 1 to n; random(a, b) is a to b.</summary>
        public static Value Random(Interpreter interpreter, List<Value> arguments)
        {
            Random source = new Random(unchecked(Environment.TickCount + interpreter.Steps));

            if (arguments.Count == 0)
                return Value.Of(source.NextDouble());

            if (arguments.Count == 1)
            {
                int high = (int)arguments[0].AsNumber();

                if (high < 1)
                    high = 1;

                return Value.Of((double)(source.Next(high) + 1));
            }

            int from = (int)arguments[0].AsNumber();
            int to = (int)arguments[1].AsNumber();

            if (to < from)
            {
                int swap = from;
                from = to;
                to = swap;
            }

            return Value.Of((double)(from + source.Next(to - from + 1)));
        }

        public static Value RandomSeed(Interpreter interpreter, List<Value> arguments)
        {
            // The interpreter's own stream is not reseeded; the built-in random
            // is deliberately simple, and seeding is reported as accepted.
            return Value.Of(Argument(arguments, 0).AsNumber());
        }

        // ------------------------------------------------------------------
        // Text
        // ------------------------------------------------------------------

        public static Value Upper(Interpreter interpreter, List<Value> arguments)
        {
            return Value.Of(Argument(arguments, 0).AsText().ToUpperInvariant());
        }

        public static Value Lower(Interpreter interpreter, List<Value> arguments)
        {
            return Value.Of(Argument(arguments, 0).AsText().ToLowerInvariant());
        }

        /// <summary>sub(text, from [, to]) counts from 1, as Lua does.</summary>
        public static Value Substring(Interpreter interpreter, List<Value> arguments)
        {
            string text = Argument(arguments, 0).AsText();
            int from = (int)Argument(arguments, 1).AsNumber();

            int to = arguments.Count > 2 ? (int)arguments[2].AsNumber() : text.Length;

            if (from < 1)
                from = 1;

            if (to > text.Length)
                to = text.Length;

            if (from > to || from > text.Length)
                return Value.Of(string.Empty);

            return Value.Of(text.Substring(from - 1, to - from + 1));
        }

        public static Value Find(Interpreter interpreter, List<Value> arguments)
        {
            string haystack = Argument(arguments, 0).AsText();
            string needle = Argument(arguments, 1).AsText();

            int index = haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase);

            return Value.Of((double)(index + 1));
        }

        public static Value Repeat(Interpreter interpreter, List<Value> arguments)
        {
            string text = Argument(arguments, 0).AsText();
            int times = (int)Argument(arguments, 1).AsNumber();

            if (times < 0)
                times = 0;

            if (times > 10000)
                times = 10000;

            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            for (int i = 0; i < times; i++)
                builder.Append(text);

            return Value.Of(builder.ToString());
        }

        public static Value Split(Interpreter interpreter, List<Value> arguments)
        {
            string text = Argument(arguments, 0).AsText();
            string separator = arguments.Count > 1 ? arguments[1].AsText() : ",";

            Value list = Value.Table();

            if (separator.Length == 0)
            {
                for (int i = 0; i < text.Length; i++)
                    list.Append(Value.Of(text.Substring(i, 1)));

                return list;
            }

            string[] parts = text.Split(new string[] { separator }, StringSplitOptions.None);

            for (int i = 0; i < parts.Length; i++)
                list.Append(Value.Of(parts[i]));

            return list;
        }

        public static Value Join(Interpreter interpreter, List<Value> arguments)
        {
            Value list = Argument(arguments, 0);
            string separator = arguments.Count > 1 ? arguments[1].AsText() : ",";

            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            for (int i = 0; i < list.Length; i++)
            {
                if (i > 0)
                    builder.Append(separator);

                builder.Append(list.GetIndex(i + 1).AsText());
            }

            return Value.Of(builder.ToString());
        }

        /// <summary>format(text, ...) replaces each "{}" with the next argument.</summary>
        public static Value Format(Interpreter interpreter, List<Value> arguments)
        {
            if (arguments.Count == 0)
                return Value.Of(string.Empty);

            string pattern = arguments[0].AsText();

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            int next = 1;

            for (int i = 0; i < pattern.Length; i++)
            {
                if (pattern[i] == '{' && i + 1 < pattern.Length && pattern[i + 1] == '}')
                {
                    if (next < arguments.Count)
                    {
                        builder.Append(arguments[next].AsText());
                        next++;
                    }

                    i++;
                    continue;
                }

                builder.Append(pattern[i]);
            }

            return Value.Of(builder.ToString());
        }

        // ------------------------------------------------------------------
        // Tables
        // ------------------------------------------------------------------

        public static Value Append(Interpreter interpreter, List<Value> arguments)
        {
            Value table = Argument(arguments, 0);

            if (table.IsTable || table.IsObject)
                table.Append(Argument(arguments, 1));

            return table;
        }

        public static Value Keys(Interpreter interpreter, List<Value> arguments)
        {
            Value table = Argument(arguments, 0);
            Value names = Value.Table();

            List<string> keys = table.FieldNames();

            for (int i = 0; i < keys.Count; i++)
                names.Append(Value.Of(keys[i]));

            return names;
        }

        public static Value Count(Interpreter interpreter, List<Value> arguments)
        {
            return Value.Of((double)Argument(arguments, 0).Length);
        }

        public static Value Remove(Interpreter interpreter, List<Value> arguments)
        {
            Value table = Argument(arguments, 0);
            int index = (int)Argument(arguments, 1).AsNumber();

            if (table.Items == null)
                return Value.Nil();

            int position = index - 1;

            if (position < 0 || position >= table.Items.Count)
                return Value.Nil();

            Value removed = table.Items[position];

            table.Items.RemoveAt(position);

            return removed;
        }

        public static Value Copy(Interpreter interpreter, List<Value> arguments)
        {
            return Argument(arguments, 0).ShallowCopy();
        }

        /// <summary>sort(list) sorts numbers and text in place and returns the list.</summary>
        public static Value Sort(Interpreter interpreter, List<Value> arguments)
        {
            Value table = Argument(arguments, 0);

            if (table.Items == null)
                return table;

            table.Items.Sort(delegate (Value left, Value right)
            {
                if (left.Kind == ValueKind.Number && right.Kind == ValueKind.Number)
                    return left.Number.CompareTo(right.Number);

                return string.CompareOrdinal(left.AsText(), right.AsText());
            });

            return table;
        }

        // ------------------------------------------------------------------
        // Module tables
        // ------------------------------------------------------------------

        private static Value BuildMathTable()
        {
            Value table = Value.Table();

            table.SetField("pi", Value.Of(Math.PI));
            table.SetField("e", Value.Of(Math.E));
            table.SetField("abs", Value.Native(Abs));
            table.SetField("floor", Value.Native(Floor));
            table.SetField("ceil", Value.Native(Ceil));
            table.SetField("round", Value.Native(Round));
            table.SetField("sqrt", Value.Native(Sqrt));
            table.SetField("pow", Value.Native(Pow));
            table.SetField("min", Value.Native(Min));
            table.SetField("max", Value.Native(Max));
            table.SetField("clamp", Value.Native(Clamp));
            table.SetField("sin", Value.Native(Sin));
            table.SetField("cos", Value.Native(Cos));
            table.SetField("tan", Value.Native(Tan));
            table.SetField("atan2", Value.Native(Atan2));

            return table;
        }

        private static Value BuildStringTable()
        {
            Value table = Value.Table();

            table.SetField("upper", Value.Native(Upper));
            table.SetField("lower", Value.Native(Lower));
            table.SetField("sub", Value.Native(Substring));
            table.SetField("find", Value.Native(Find));
            table.SetField("rep", Value.Native(Repeat));
            table.SetField("split", Value.Native(Split));
            table.SetField("join", Value.Native(Join));
            table.SetField("format", Value.Native(Format));

            return table;
        }

        private static Value BuildTableTable()
        {
            Value table = Value.Table();

            table.SetField("append", Value.Native(Append));
            table.SetField("keys", Value.Native(Keys));
            table.SetField("count", Value.Native(Count));
            table.SetField("remove", Value.Native(Remove));
            table.SetField("copy", Value.Native(Copy));
            table.SetField("sort", Value.Native(Sort));

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
