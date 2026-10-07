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
    /// <summary>What a value is.</summary>
    public enum ValueKind
    {
        Nil,
        Bool,
        Number,
        Text,
        Table,
        /// <summary>A built-in function implemented in C#.</summary>
        Native,
        /// <summary>A function written in the script.</summary>
        Function,
        /// <summary>An instance of a class.</summary>
        Object
    }

    /// <summary>
    /// Anything a script can hold: a number, some text, a table, a function or
    /// an object.
    ///
    /// A table is an ordered list that can also be keyed by name, which is how
    /// the language gets both arrays and records out of one type.
    /// </summary>
    public sealed class Value
    {
        private ValueKind m_kind;
        private double m_number;
        private bool m_boolean;
        private string m_text;
        private List<Value> m_items;
        private Dictionary<string, Value> m_fields;
        private object m_host;

        public ValueKind Kind
        {
            get { return m_kind; }
        }

        /// <summary>The number, or 0 if this is not one.</summary>
        public double Number
        {
            get { return m_number; }
            set { m_number = value; m_kind = ValueKind.Number; }
        }

        public bool Boolean
        {
            get { return m_boolean; }
            set { m_boolean = value; m_kind = ValueKind.Bool; }
        }

        public string Text
        {
            get { return m_text; }
            set { m_text = value; m_kind = ValueKind.Text; }
        }

        /// <summary>The ordered items, for a table or an object's own list.</summary>
        public List<Value> Items
        {
            get { return m_items; }
        }

        /// <summary>The named members, for a table or an object.</summary>
        public Dictionary<string, Value> Fields
        {
            get { return m_fields; }
        }

        /// <summary>The C# object behind a native value, host object or function.</summary>
        public object Host
        {
            get { return m_host; }
            set { m_host = value; }
        }

        public bool IsNil
        {
            get { return m_kind == ValueKind.Nil; }
        }

        public bool IsTable
        {
            get { return m_kind == ValueKind.Table; }
        }

        public bool IsObject
        {
            get { return m_kind == ValueKind.Object; }
        }

        public bool IsCallable
        {
            get { return m_kind == ValueKind.Function || m_kind == ValueKind.Native; }
        }

        /// <summary>A value with no contents.</summary>
        public static Value Nil()
        {
            Value value = new Value();
            value.m_kind = ValueKind.Nil;
            return value;
        }

        public static Value Of(double number)
        {
            Value value = new Value();
            value.m_number = number;
            value.m_kind = ValueKind.Number;
            return value;
        }

        public static Value Of(bool boolean)
        {
            Value value = new Value();
            value.m_boolean = boolean;
            value.m_kind = ValueKind.Bool;
            return value;
        }

        public static Value Of(string text)
        {
            Value value = new Value();
            value.m_text = text;
            value.m_kind = ValueKind.Text;
            return value;
        }

        /// <summary>A new, empty table.</summary>
        public static Value Table()
        {
            Value value = new Value();
            value.m_kind = ValueKind.Table;
            value.m_items = new List<Value>();
            value.m_fields = new Dictionary<string, Value>(StringComparer.Ordinal);
            return value;
        }

        /// <summary>A table holding a list of values, numbered from 1.</summary>
        public static Value List(IEnumerable<Value> items)
        {
            Value table = Table();

            if (items == null)
                return table;

            foreach (Value item in items)
                table.m_items.Add(item);

            return table;
        }

        /// <summary>A built-in function.</summary>
        public static Value Native(NativeFunction function)
        {
            Value value = new Value();
            value.m_kind = ValueKind.Native;
            value.m_host = function;
            return value;
        }

        /// <summary>A function written in the script.</summary>
        public static Value Function(string name, object body)
        {
            Value value = new Value();
            value.m_kind = ValueKind.Function;
            value.m_text = name;
            value.m_host = body;
            return value;
        }

        /// <summary>An instance of a class.</summary>
        public static Value Object(string className)
        {
            Value value = Table();
            value.m_kind = ValueKind.Object;
            value.m_text = className;
            value.m_fields["__class"] = Of(className);
            return value;
        }

        // ------------------------------------------------------------------
        // Tables and objects
        // ------------------------------------------------------------------

        /// <summary>A numbered item, counting from 1 the way the language does.</summary>
        public Value GetIndex(int index)
        {
            if (m_items == null)
                return Nil();

            int position = index - 1;

            if (position < 0 || position >= m_items.Count)
                return Nil();

            return m_items[position];
        }

        /// <summary>Sets a numbered item, growing the list as needed.</summary>
        public void SetIndex(int index, Value value)
        {
            if (m_items == null)
                return;

            int position = index - 1;

            if (position < 0)
                return;

            while (m_items.Count <= position)
                m_items.Add(Nil());

            m_items[position] = value;
        }

        /// <summary>Appends to the numbered part of the table.</summary>
        public void Append(Value value)
        {
            if (m_items == null)
                return;

            m_items.Add(value);
        }

        /// <summary>How many numbered items there are.</summary>
        public int Length
        {
            get { return m_items == null ? 0 : m_items.Count; }
        }

        /// <summary>A named member, or nil.</summary>
        public Value GetField(string name)
        {
            if (m_fields == null || name == null)
                return Nil();

            Value found;

            if (m_fields.TryGetValue(name, out found))
                return found;

            return Nil();
        }

        /// <summary>Sets a named member. Setting one to nil removes it.</summary>
        public void SetField(string name, Value value)
        {
            if (m_fields == null || name == null)
                return;

            if (value == null || value.IsNil)
            {
                m_fields.Remove(name);
                return;
            }

            m_fields[name] = value;
        }

        public bool HasField(string name)
        {
            return m_fields != null && name != null && m_fields.ContainsKey(name);
        }

        /// <summary>Every named member, for a debug view.</summary>
        public List<string> FieldNames()
        {
            List<string> names = new List<string>();

            if (m_fields != null)
                names.AddRange(m_fields.Keys);

            return names;
        }

        // ------------------------------------------------------------------
        // Conversion
        // ------------------------------------------------------------------

        /// <summary>
        /// Truthiness: everything is true except nil and false. Zero and the
        /// empty string are true, as they are in Lua.
        /// </summary>
        public bool IsTruthy()
        {
            if (m_kind == ValueKind.Nil)
                return false;

            if (m_kind == ValueKind.Bool)
                return m_boolean;

            return true;
        }

        /// <summary>Reads the value as a number, or throws if it is not one.</summary>
        public double AsNumber()
        {
            if (m_kind == ValueKind.Number)
                return m_number;

            if (m_kind == ValueKind.Bool)
                return m_boolean ? 1.0 : 0.0;

            if (m_kind == ValueKind.Text)
            {
                double parsed;

                if (double.TryParse(m_text, System.Globalization.NumberStyles.Float,
                                    System.Globalization.CultureInfo.InvariantCulture, out parsed))
                    return parsed;
            }

            if (m_kind == ValueKind.Nil)
                return 0.0;

            throw new ScriptError("Expected a number, found " + m_kind, 0, 0);
        }

        /// <summary>Reads the value as text, converting numbers the obvious way.</summary>
        public string AsText()
        {
            switch (m_kind)
            {
                case ValueKind.Text:
                    return m_text;

                case ValueKind.Number:
                    return FormatNumber(m_number);

                case ValueKind.Bool:
                    return m_boolean ? "true" : "false";

                case ValueKind.Nil:
                    return "nil";

                case ValueKind.Object:
                    return m_text + " instance";

                case ValueKind.Table:
                    return "table (" + Length + " items)";

                case ValueKind.Function:
                case ValueKind.Native:
                    return "function " + (m_text ?? "?");

                default:
                    return string.Empty;
            }
        }

        /// <summary>Whole numbers print without a decimal point.</summary>
        public static string FormatNumber(double number)
        {
            if (number == Math.Floor(number) && Math.Abs(number) < 1e15)
                return ((long)number).ToString(System.Globalization.CultureInfo.InvariantCulture);

            return number.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>A shallow copy, so a table can be passed without being shared.</summary>
        public Value ShallowCopy()
        {
            Value copy = new Value();
            copy.m_kind = m_kind;
            copy.m_number = m_number;
            copy.m_boolean = m_boolean;
            copy.m_text = m_text;
            copy.m_host = m_host;

            if (m_items != null)
                copy.m_items = new List<Value>(m_items);

            if (m_fields != null)
                copy.m_fields = new Dictionary<string, Value>(m_fields, StringComparer.Ordinal);

            return copy;
        }

        public override string ToString()
        {
            return AsText();
        }
    }

    /// <summary>A function implemented in C# and called from a script.</summary>
    public delegate Value NativeFunction(Interpreter interpreter, List<Value> arguments);

    /// <summary>
    /// A class written in the script. Fields and methods are looked up on the
    /// class, then up the chain of parents, which is how inheritance works here.
    /// </summary>
    public sealed class ScriptClass
    {
        private string m_name;
        private ScriptClass m_parent;
        private Dictionary<string, Value> m_members;

        public string Name
        {
            get { return m_name; }
        }

        /// <summary>The class this one extends, or null.</summary>
        public ScriptClass Parent
        {
            get { return m_parent; }
        }

        public Dictionary<string, Value> Members
        {
            get { return m_members; }
        }

        public ScriptClass(string name, ScriptClass parent)
        {
            m_name = name;
            m_parent = parent;
            m_members = new Dictionary<string, Value>(StringComparer.Ordinal);
        }

        /// <summary>
        /// Finds a member on this class, or on the nearest parent that has it.
        /// </summary>
        public Value Lookup(string name)
        {
            Value found;

            if (m_members.TryGetValue(name, out found))
                return found;

            if (m_parent != null)
                return m_parent.Lookup(name);

            return Value.Nil();
        }

        /// <summary>True if this class, or a parent, declares the member.</summary>
        public bool Has(string name)
        {
            return !Lookup(name).IsNil;
        }

        /// <summary>Every class in the chain, this one first.</summary>
        public List<ScriptClass> GetChain()
        {
            List<ScriptClass> chain = new List<ScriptClass>();

            ScriptClass current = this;

            while (current != null)
            {
                chain.Add(current);
                current = current.m_parent;
            }

            return chain;
        }

        /// <summary>True if this class is, or derives from, the given one.</summary>
        public bool IsA(ScriptClass other)
        {
            if (other == null)
                return false;

            ScriptClass current = this;

            while (current != null)
            {
                if (current == other)
                    return true;

                current = current.m_parent;
            }

            return false;
        }

        public override string ToString()
        {
            if (m_parent == null)
                return "class " + m_name;

            return "class " + m_name + " extends " + m_parent.Name;
        }
    }

    /// <summary>
    /// One call frame: the local names of a function, the module's globals and
    /// the object a method was called on.
    /// </summary>
    public sealed class Scope
    {
        private Scope m_parent;
        private Dictionary<string, Value> m_names;
        private Value m_self;

        public Scope Parent
        {
            get { return m_parent; }
        }

        public Value Self
        {
            get { return m_self; }
            set { m_self = value; }
        }

        public Scope(Scope parent)
        {
            m_parent = parent;
            m_names = new Dictionary<string, Value>(StringComparer.Ordinal);
        }

        /// <summary>Declares a name in this frame.</summary>
        public void Declare(string name, Value value)
        {
            if (name == null)
                return;

            m_names[name] = value == null ? Value.Nil() : value;
        }

        /// <summary>Looks a name up here, then outward through the parents.</summary>
        public Value Get(string name)
        {
            Value found;

            if (m_names.TryGetValue(name, out found))
                return found;

            if (m_parent != null)
                return m_parent.Get(name);

            return Value.Nil();
        }

        /// <summary>
        /// Assigns to a name. If it exists in an outer frame, that frame is
        /// written to; otherwise a new local is made here. This is what makes a
        /// module's globals assignable from inside a function.
        /// </summary>
        public void Assign(string name, Value value)
        {
            if (name == null)
                return;

            Scope owner = FindOwner(name);

            if (owner != null)
            {
                owner.m_names[name] = value == null ? Value.Nil() : value;
                return;
            }

            Declare(name, value);
        }

        public bool Has(string name)
        {
            return FindOwner(name) != null;
        }

        private Scope FindOwner(string name)
        {
            Scope current = this;

            while (current != null)
            {
                if (current.m_names.ContainsKey(name))
                    return current;

                current = current.m_parent;
            }

            return null;
        }

        /// <summary>Every name visible from here, innermost first.</summary>
        public List<string> VisibleNames()
        {
            List<string> names = new List<string>(m_names.Keys);

            if (m_parent != null)
                names.AddRange(m_parent.VisibleNames());

            return names;
        }

        public override string ToString()
        {
            return "scope (" + m_names.Count + " names)";
        }
    }
}
