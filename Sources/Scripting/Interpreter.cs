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
    /// <summary>Raised by "return", caught by the call that is waiting for it.</summary>
    public sealed class ReturnSignal : Exception
    {
        private Value m_value;

        public Value Value
        {
            get { return m_value; }
        }

        public ReturnSignal(Value value)
            : base("return")
        {
            m_value = value;
        }
    }

    /// <summary>Raised by "break" and "continue", caught by the nearest loop.</summary>
    public sealed class LoopSignal : Exception
    {
        private bool m_isBreak;

        public bool IsBreak
        {
            get { return m_isBreak; }
        }

        public LoopSignal(bool isBreak)
            : base(isBreak ? "break" : "continue")
        {
            m_isBreak = isBreak;
        }
    }

    /// <summary>
    /// Walks a parse tree and does what it says.
    ///
    /// The interpreter owns the module globals and the class table. Everything
    /// else — a function's locals, the object a method was called on — lives in
    /// a <see cref="Scope"/> that is created and discarded as the walk descends.
    /// </summary>
    public sealed class Interpreter
    {
        private Scope m_globals;
        private Dictionary<string, ScriptClass> m_classes;
        private Dictionary<string, Value> m_modules;
        private List<string> m_output;
        private int m_steps;
        private int m_stepLimit;
        private string m_sourceName;
        private Random m_random;
        private Value m_currentSelf;
        private ScriptClass m_currentClass;

        /// <summary>Written to by print and anything else that reports.</summary>
        public IList<string> Output
        {
            get { return m_output; }
        }

        /// <summary>The module globals.</summary>
        public Scope Globals
        {
            get { return m_globals; }
        }

        /// <summary>Every class declared so far.</summary>
        public Dictionary<string, ScriptClass> Classes
        {
            get { return m_classes; }
        }

        /// <summary>
        /// A ceiling on the number of statements a script may execute. A runaway
        /// loop in a script should not hang the program, so it throws instead.
        /// </summary>
        public int StepLimit
        {
            get { return m_stepLimit; }
            set { m_stepLimit = value < 1000 ? 1000 : value; }
        }

        public int Steps
        {
            get { return m_steps; }
        }

        public Interpreter()
        {
            m_globals = new Scope(null);
            m_classes = new Dictionary<string, ScriptClass>(StringComparer.Ordinal);
            m_modules = new Dictionary<string, Value>(StringComparer.Ordinal);
            m_output = new List<string>();
            m_steps = 0;
            m_stepLimit = 2000000;
            m_sourceName = "script";
            m_random = new Random(20261007);

            StandardLibrary.Install(this);
        }

        /// <summary>Empties the output buffer.</summary>
        public void ClearOutput()
        {
            m_output.Clear();
        }

        /// <summary>Everything printed since the last clear, joined by newlines.</summary>
        public string TakeOutput()
        {
            return string.Join("\n", m_output.ToArray());
        }

        // ------------------------------------------------------------------
        // Running
        // ------------------------------------------------------------------

        /// <summary>Reads and runs a script from text.</summary>
        public Value Run(string source, string sourceName)
        {
            m_sourceName = sourceName ?? "script";
            m_steps = 0;

            Lexer lexer = new Lexer(source);
            List<Token> tokens = lexer.Tokenise();

            Parser parser = new Parser(tokens, m_sourceName);
            Node program = parser.ParseProgram();

            return Execute(program, m_globals);
        }

        /// <summary>Reads and runs a script, reporting the errors it found.</summary>
        public bool TryRun(string source, string sourceName, out string error)
        {
            error = string.Empty;

            try
            {
                Run(source, sourceName);
                return true;
            }
            catch (ScriptError scriptError)
            {
                error = (sourceName ?? "script") + ": " + scriptError.Message;
                return false;
            }
            catch (Exception other)
            {
                error = (sourceName ?? "script") + ": " + other.Message;
                return false;
            }
        }

        /// <summary>Calls a global function by name with the given arguments.</summary>
        public Value Call(string functionName, params Value[] arguments)
        {
            Value function = m_globals.Get(functionName);

            List<Value> list = new List<Value>();

            if (arguments != null)
                list.AddRange(arguments);

            return CallValue(function, list, m_globals);
        }

        /// <summary>Calls any callable value.</summary>
        public Value CallValue(Value function, List<Value> arguments, Scope callingScope)
        {
            if (function == null || !function.IsCallable)
                throw new ScriptError("That is not a function", 0, 0);

            if (function.Kind == ValueKind.Native)
            {
                NativeFunction native = function.Host as NativeFunction;

                if (native == null)
                    throw new ScriptError("Broken native function", 0, 0);

                return native(this, arguments ?? new List<Value>());
            }

            Node body = function.Host as Node;

            if (body == null)
                throw new ScriptError("Broken script function", 0, 0);

            Scope callScope = new Scope(m_globals);
            callScope.Self = m_currentSelf;

            // Bind the parameters to the arguments.
            Node parameters = body.Children.Count > 1 ? body.Children[1] : null;

            if (parameters != null)
            {
                for (int i = 0; i < parameters.Body.Count; i++)
                {
                    Value argument = Value.Nil();

                    if (arguments != null && i < arguments.Count)
                        argument = arguments[i];

                    callScope.Declare(parameters.Body[i].Name, argument);
                }
            }

            try
            {
                for (int i = 0; i < body.Body.Count; i++)
                    Execute(body.Body[i], callScope);
            }
            catch (ReturnSignal signal)
            {
                return signal.Value;
            }

            return Value.Nil();

        }

        // ------------------------------------------------------------------
        // Statements
        // ------------------------------------------------------------------

        private Value Execute(Node node, Scope scope)
        {
            if (node == null)
                return Value.Nil();

            m_steps++;

            if (m_steps > m_stepLimit)
                throw new ScriptError("The script ran too long and was stopped", node.Line, 0);

            switch (node.Kind)
            {
                case NodeKind.Block:
                    for (int i = 0; i < node.Body.Count; i++)
                        Execute(node.Body[i], scope);

                    return Value.Nil();

                case NodeKind.Assign:
                    return ExecuteAssign(node, scope);

                case NodeKind.If:
                    return ExecuteIf(node, scope);

                case NodeKind.While:
                    return ExecuteWhile(node, scope);

                case NodeKind.For:
                    return ExecuteFor(node, scope);

                case NodeKind.Function:
                    return ExecuteFunction(node, scope);

                case NodeKind.Class:
                    return ExecuteClass(node, scope);

                case NodeKind.Return:
                    throw new ReturnSignal(node.Children.Count > 0
                        ? Evaluate(node.Children[0], scope)
                        : Value.Nil());

                case NodeKind.Break:
                    throw new LoopSignal(true);

                case NodeKind.Continue:
                    throw new LoopSignal(false);

                case NodeKind.Import:
                    return ExecuteImport(node, scope);

                default:
                    // Anything else is an expression used as a statement.
                    Evaluate(node, scope);
                    return Value.Nil();
            }
        }

        private Value ExecuteAssign(Node node, Scope scope)
        {
            if (node.Children.Count < 2)
                return Value.Nil();

            Node target = node.Children[0];
            Value value = Evaluate(node.Children[1], scope);

            bool isLocal = node.Name == "local";

            switch (target.Kind)
            {
                case NodeKind.Name:
                    if (isLocal)
                        scope.Declare(target.Name, value);
                    else
                        scope.Assign(target.Name, value);
                    break;

                case NodeKind.Field:
                    AssignField(target, value, scope);
                    break;

                case NodeKind.Index:
                    {
                        Value container = Evaluate(target.Children[0], scope);
                        Value index = Evaluate(target.Children[1], scope);

                        AssignIndex(container, index, value);
                        break;
                    }

                case NodeKind.Self:
                    // self.x = value
                    if (scope.Self != null)
                        scope.Self.SetField(target.Name, value);
                    break;

                default:
                    throw new ScriptError("Cannot assign to that", node.Line, 0);
            }

            return value;
        }

        private void AssignField(Node target, Value value, Scope scope)
        {
            if (target.Children.Count == 0)
                return;

            Node container_node = target.Children[0];

            // super.name = value writes on the current object, so a subclass can
            // set a field the parent declared.
            if (container_node.Kind == NodeKind.Super)
            {
                if (scope.Self != null)
                    scope.Self.SetField(target.Name, value);

                return;
            }

            Value container = Evaluate(container_node, scope);

            if (container == null || container.IsNil)
                throw new ScriptError("Cannot set '" + target.Name + "' on nil", target.Line, 0);

            container.SetField(target.Name, value);
        }

        private void AssignIndex(Value container, Value index, Value value)
        {
            if (container == null || container.IsNil)
                return;

            if (index.Kind == ValueKind.Number)
            {
                container.SetIndex((int)index.Number, value);
                return;
            }

            container.SetField(index.AsText(), value);
        }

        private Value ExecuteIf(Node node, Scope scope)
        {
            if (Evaluate(node.Condition, scope).IsTruthy())
                return ExecuteBlock(node.Body, scope);

            for (int i = 0; i < node.Children.Count; i++)
            {
                Node branch = node.Children[i];

                if (branch.Kind == NodeKind.If)
                {
                    if (Evaluate(branch.Condition, scope).IsTruthy())
                        return ExecuteBlock(branch.Body, scope);
                }
                else
                {
                    // The trailing else block.
                    return ExecuteBlock(branch.Body, scope);
                }
            }

            return Value.Nil();
        }

        private Value ExecuteBlock(List<Node> body, Scope scope)
        {
            for (int i = 0; i < body.Count; i++)
                Execute(body[i], scope);

            return Value.Nil();
        }

        private Value ExecuteWhile(Node node, Scope scope)
        {
            while (Evaluate(node.Condition, scope).IsTruthy())
            {
                m_steps++;

                if (m_steps > m_stepLimit)
                    throw new ScriptError("The loop ran too long and was stopped", node.Line, 0);

                try
                {
                    ExecuteBlock(node.Body, scope);
                }
                catch (LoopSignal signal)
                {
                    if (signal.IsBreak)
                        break;

                    continue;
                }
            }

            return Value.Nil();
        }

        private Value ExecuteFor(Node node, Scope scope)
        {
            Value iterable = Evaluate(node.Initial, scope);

            // Only the "for x in table" form is iterated; the numeric form is
            // detected by the condition being present without an iterable table.
            if (iterable.IsTable || iterable.IsObject)
            {
                Scope loopScope = new Scope(scope);

                for (int i = 0; i < iterable.Length; i++)
                {
                    m_steps++;

                    if (m_steps > m_stepLimit)
                        throw new ScriptError("The loop ran too long and was stopped", node.Line, 0);

                    loopScope.Declare(node.Name, iterable.GetIndex(i + 1));

                    try
                    {
                        ExecuteBlock(node.Body, loopScope);
                    }
                    catch (LoopSignal signal)
                    {
                        if (signal.IsBreak)
                            break;

                        continue;
                    }
                }

                return Value.Nil();
            }

            // Numeric form: for i = from, to [, step] do ... end
            double from = iterable.AsNumber();
            double to = Evaluate(node.Condition, scope).AsNumber();
            double step = node.Step != null ? Evaluate(node.Step, scope).AsNumber() : 1.0;

            if (step == 0.0)
                step = 1.0;

            Scope counterScope = new Scope(scope);

            if (step > 0.0)
            {
                for (double value = from; value <= to; value += step)
                {
                    m_steps++;

                    if (m_steps > m_stepLimit)
                        throw new ScriptError("The loop ran too long and was stopped", node.Line, 0);

                    counterScope.Declare(node.Name, Value.Of(value));

                    try
                    {
                        ExecuteBlock(node.Body, counterScope);
                    }
                    catch (LoopSignal signal)
                    {
                        if (signal.IsBreak)
                            break;

                        continue;
                    }
                }
            }
            else
            {
                for (double value = from; value >= to; value += step)
                {
                    m_steps++;

                    if (m_steps > m_stepLimit)
                        throw new ScriptError("The loop ran too long and was stopped", node.Line, 0);

                    counterScope.Declare(node.Name, Value.Of(value));

                    try
                    {
                        ExecuteBlock(node.Body, counterScope);
                    }
                    catch (LoopSignal signal)
                    {
                        if (signal.IsBreak)
                            break;

                        continue;
                    }
                }
            }

            return Value.Nil();
        }

        private Value ExecuteFunction(Node node, Scope scope)
        {
            if (node.Children.Count == 0)
                return Value.Nil();

            Node target = node.Children[0];
            Value function = Value.Function(node.Name, node);

            switch (target.Kind)
            {
                case NodeKind.Name:
                    scope.Assign(target.Name, function);
                    break;

                case NodeKind.Field:
                    {
                        // function obj.name() ... end is a method on an object.
                        Value container = Evaluate(target.Children[0], scope);

                        container.SetField(target.Name, function);
                        break;
                    }

                case NodeKind.Index:
                    {
                        Value container = Evaluate(target.Children[0], scope);
                        Value index = Evaluate(target.Children[1], scope);

                        AssignIndex(container, index, function);
                        break;
                    }
            }

            return function;
        }

        private Value ExecuteClass(Node node, Scope scope)
        {
            ScriptClass parent = null;

            if (node.Base != null)
            {
                ScriptClass found;

                if (m_classes.TryGetValue(node.Base.Name, out found))
                    parent = found;
            }

            ScriptClass declared = new ScriptClass(node.ClassName, parent);

            m_classes[node.ClassName] = declared;

            Scope classScope = new Scope(scope);

            for (int i = 0; i < node.Body.Count; i++)
            {
                Node member = node.Body[i];

                if (member.Kind == NodeKind.Function)
                {
                    declared.Members[member.Name] = Value.Function(member.Name, member);
                    continue;
                }

                if (member.Kind == NodeKind.Assign && member.ClassName == "field")
                {
                    declared.Members[member.Name] = Evaluate(member.Children[0], classScope);
                    continue;
                }

                Execute(member, classScope);
            }

            // The class itself is a value in the module, so it can be passed on.
            Value classValue = Value.Table();

            classValue.SetField("__class", Value.Of(true));
            classValue.SetField("name", Value.Of(node.ClassName));
            classValue.Host = declared;

            scope.Assign(node.ClassName, classValue);

            return classValue;
        }

        private Value ExecuteImport(Node node, Scope scope)
        {
            Value module;

            if (m_modules.TryGetValue(node.Name, out module))
            {
                scope.Assign(node.Name, module);
                return module;
            }

            return Value.Nil();
        }

        // ------------------------------------------------------------------
        // Expressions
        // ------------------------------------------------------------------

        /// <summary>Evaluates any node as a value.</summary>
        public Value Evaluate(Node node, Scope scope)
        {
            if (node == null)
                return Value.Nil();

            switch (node.Kind)
            {
                case NodeKind.Literal:
                    return node.Literal;

                case NodeKind.Name:
                    return scope.Get(node.Name);

                case NodeKind.Self:
                    return scope.Self == null ? Value.Nil() : scope.Self;

                case NodeKind.Super:
                    return EvaluateSuper(node, scope);

                case NodeKind.New:
                    return Instantiate(node, scope);

                case NodeKind.Binary:
                    return EvaluateBinary(node, scope);

                case NodeKind.Unary:
                    return EvaluateUnary(node, scope);

                case NodeKind.Call:
                    return EvaluateCall(node, scope);

                case NodeKind.MethodCall:
                    return EvaluateMethodCall(node, scope);

                case NodeKind.Field:
                    return EvaluateField(node, scope);

                case NodeKind.Index:
                    return EvaluateIndex(node, scope);

                case NodeKind.Function:
                    return Value.Function(node.Name, node);

                case NodeKind.TableConstructor:
                    return EvaluateTable(node, scope);

                case NodeKind.Block:
                    return ExecuteBlock(node.Body, scope);

                default:
                    return Execute(node, scope);
            }
        }

        private Value EvaluateSuper(Node node, Scope scope)
        {
            if (scope.Self == null || m_currentClass == null)
                return Value.Nil();

            ScriptClass parent = m_currentClass.Parent;

            if (parent == null)
                return Value.Nil();

            if (string.IsNullOrEmpty(node.Name))
            {
                Value self = scope.Self;
                return self;
            }

            Value member = parent.Lookup(node.Name);

            // A super method call binds the current object, so it still sees the
            // subclass's fields.
            if (member.IsCallable)
                return member;

            return member;
        }

        private Value Instantiate(Node node, Scope scope)
        {
            ScriptClass declared;

            if (!m_classes.TryGetValue(node.Name, out declared))
                throw new ScriptError("There is no class called '" + node.Name + "'", node.Line, 0);

            Value instance = Value.Object(node.Name);

            instance.Host = declared;

            // Copy the class's own fields, and the parents', onto the instance.
            List<ScriptClass> chain = declared.GetChain();

            for (int c = chain.Count - 1; c >= 0; c--)
            {
                foreach (KeyValuePair<string, Value> pair in chain[c].Members)
                {
                    if (pair.Value.IsCallable)
                        continue;

                    instance.SetField(pair.Key, pair.Value.ShallowCopy());
                }
            }

            // Run the initialiser, if there is one.
            Value initialiser = declared.Lookup("init");

            if (initialiser.IsCallable)
            {
                List<Value> arguments = new List<Value>();

                for (int i = 0; i < node.Children.Count; i++)
                    arguments.Add(Evaluate(node.Children[i], scope));

                CallWithSelf(initialiser, instance, declared, arguments, scope);
            }

            return instance;
        }

        /// <summary>Calls a function with an object bound as self.</summary>
        private Value CallWithSelf(Value function, Value self, ScriptClass owner,
                                   List<Value> arguments, Scope scope)
        {
            Value previousSelf = m_currentSelf;
            ScriptClass previousClass = m_currentClass;

            m_currentSelf = self;
            m_currentClass = owner;

            try
            {
                Scope callScope = new Scope(m_globals);

                callScope.Self = self;

                Node body = function.Host as Node;

                if (body != null)
                {
                    Node parameters = body.Children.Count > 1 ? body.Children[1] : null;

                    if (parameters != null)
                    {
                        for (int i = 0; i < parameters.Body.Count; i++)
                        {
                            Value argument = i < arguments.Count ? arguments[i] : Value.Nil();

                            callScope.Declare(parameters.Body[i].Name, argument);
                        }
                    }

                    try
                    {
                        for (int i = 0; i < body.Body.Count; i++)
                            Execute(body.Body[i], callScope);
                    }
                    catch (ReturnSignal signal)
                    {
                        return signal.Value;
                    }

                    return Value.Nil();
                }

                if (function.Kind == ValueKind.Native)
                {
                    NativeFunction native = function.Host as NativeFunction;

                    if (native != null)
                        return native(this, arguments);
                }

                return Value.Nil();
            }
            finally
            {
                m_currentSelf = previousSelf;
                m_currentClass = previousClass;
            }
        }

        private Value EvaluateBinary(Node node, Scope scope)
        {
            string op = node.Operator;

            // Short circuit, so the right side is not evaluated when it need not be.
            if (op == "and")
            {
                Value left = Evaluate(node.Children[0], scope);

                if (!left.IsTruthy())
                    return left;

                return Evaluate(node.Children[1], scope);
            }

            if (op == "or")
            {
                Value left = Evaluate(node.Children[0], scope);

                if (left.IsTruthy())
                    return left;

                return Evaluate(node.Children[1], scope);
            }

            Value a = Evaluate(node.Children[0], scope);
            Value b = Evaluate(node.Children[1], scope);

            switch (op)
            {
                case "..":
                    return Value.Of(a.AsText() + b.AsText());

                case "==":
                    return Value.Of(AreEqual(a, b));

                case "~=":
                case "!=":
                    return Value.Of(!AreEqual(a, b));

                case "<": return Value.Of(a.AsNumber() < b.AsNumber());
                case ">": return Value.Of(a.AsNumber() > b.AsNumber());
                case "<=": return Value.Of(a.AsNumber() <= b.AsNumber());
                case ">=": return Value.Of(a.AsNumber() >= b.AsNumber());
                case "+": return Value.Of(a.AsNumber() + b.AsNumber());
                case "-": return Value.Of(a.AsNumber() - b.AsNumber());
                case "*": return Value.Of(a.AsNumber() * b.AsNumber());

                case "/":
                    {
                        double divisor = b.AsNumber();

                        if (divisor == 0.0)
                            return Value.Of(0.0);

                        return Value.Of(a.AsNumber() / divisor);
                    }

                case "%":
                    {
                        double divisor = b.AsNumber();

                        if (divisor == 0.0)
                            return Value.Of(0.0);

                        return Value.Of(a.AsNumber() % divisor);
                    }

                case "^":
                    return Value.Of(Math.Pow(a.AsNumber(), b.AsNumber()));

                default:
                    throw new ScriptError("Unknown operator '" + op + "'", node.Line, 0);
            }
        }

        private bool AreEqual(Value a, Value b)
        {
            if (a == null || b == null)
                return false;

            if (a.Kind != b.Kind)
            {
                // Numbers and numeric text compare by value, which is what a
                // script author expects from a language with one number type.
                if (a.Kind == ValueKind.Number || b.Kind == ValueKind.Number)
                {
                    try
                    {
                        return Math.Abs(a.AsNumber() - b.AsNumber()) < 1e-12;
                    }
                    catch (ScriptError)
                    {
                        return false;
                    }
                }

                return false;
            }

            switch (a.Kind)
            {
                case ValueKind.Nil: return true;
                case ValueKind.Bool: return a.Boolean == b.Boolean;
                case ValueKind.Number: return a.Number == b.Number;
                case ValueKind.Text: return string.Equals(a.Text, b.Text, StringComparison.Ordinal);
                default: return object.ReferenceEquals(a, b) || ReferenceEquals(a.Host, b.Host);
            }
        }

        private Value EvaluateUnary(Node node, Scope scope)
        {
            Value operand = Evaluate(node.Children[0], scope);

            switch (node.Operator)
            {
                case "not":
                    return Value.Of(!operand.IsTruthy());

                case "-":
                    return Value.Of(-operand.AsNumber());

                case "+":
                    return Value.Of(operand.AsNumber());

                case "#":
                    return Value.Of(operand.Length);

                default:
                    throw new ScriptError("Unknown operator '" + node.Operator + "'", node.Line, 0);
            }
        }

        private Value EvaluateCall(Node node, Scope scope)
        {
            Value function = Evaluate(node.Children[0], scope);

            List<Value> arguments = new List<Value>();

            for (int i = 1; i < node.Children.Count; i++)
                arguments.Add(Evaluate(node.Children[i], scope));

            // A call on a class constructor runs init with the object bound.
            if (function.IsTable && function.HasField("__class"))
            {
                ScriptClass declared = function.Host as ScriptClass;

                if (declared != null)
                {
                    Value instance = Value.Object(declared.Name);

                    instance.Host = declared;

                    List<ScriptClass> chain = declared.GetChain();

                    for (int c = chain.Count - 1; c >= 0; c--)
                    {
                        foreach (KeyValuePair<string, Value> pair in chain[c].Members)
                        {
                            if (pair.Value.IsCallable)
                                continue;

                            instance.SetField(pair.Key, pair.Value.ShallowCopy());
                        }
                    }

                    Value initialiser = declared.Lookup("init");

                    if (initialiser.IsCallable)
                        CallWithSelf(initialiser, instance, declared, arguments, scope);

                    return instance;
                }
            }

            return CallValue(function, arguments, scope);
        }

        private Value EvaluateMethodCall(Node node, Scope scope)
        {
            Value receiver = Evaluate(node.Children[0], scope);

            ScriptClass owner = receiver.Host as ScriptClass;

            Value method = receiver.GetField(node.Name);

            // A method is looked up on the object, then on its class chain.
            if (method.IsNil && owner != null)
                method = owner.Lookup(node.Name);

            if (!method.IsCallable)
                throw new ScriptError("There is no method '" + node.Name + "'", node.Line, 0);

            List<Value> arguments = new List<Value>();

            for (int i = 1; i < node.Children.Count; i++)
                arguments.Add(Evaluate(node.Children[i], scope));

            return CallWithSelf(method, receiver, owner, arguments, scope);
        }

        private Value EvaluateField(Node node, Scope scope)
        {
            Value container = Evaluate(node.Children[0], scope);

            if (container.IsNil)
                return Value.Nil();

            Value field = container.GetField(node.Name);

            if (!field.IsNil)
                return field;

            // Fall back to the class, so methods and shared fields are visible.
            ScriptClass declared = container.Host as ScriptClass;

            if (declared != null)
                return declared.Lookup(node.Name);

            return Value.Nil();
        }

        private Value EvaluateIndex(Node node, Scope scope)
        {
            Value container = Evaluate(node.Children[0], scope);
            Value index = Evaluate(node.Children[1], scope);

            if (container.IsNil)
                return Value.Nil();

            if (index.Kind == ValueKind.Number)
                return container.GetIndex((int)index.Number);

            return container.GetField(index.AsText());
        }

        private Value EvaluateTable(Node node, Scope scope)
        {
            Value table = Value.Table();

            for (int i = 0; i < node.Body.Count; i++)
            {
                Node entry = node.Body[i];

                if (entry.Kind == NodeKind.Assign)
                {
                    if (entry.Children.Count == 2)
                    {
                        // [key] = value
                        Value key = Evaluate(entry.Children[0], scope);
                        Value value = Evaluate(entry.Children[1], scope);

                        if (key.Kind == ValueKind.Number)
                            table.SetIndex((int)key.Number, value);
                        else
                            table.SetField(key.AsText(), value);
                    }
                    else if (entry.Children.Count == 1)
                    {
                        // name = value
                        table.SetField(entry.Name, Evaluate(entry.Children[0], scope));
                    }

                    continue;
                }

                table.Append(Evaluate(entry, scope));
            }

            return table;
        }

        public override string ToString()
        {
            return "Interpreter (" + m_classes.Count + " classes)";
        }
    }
}
