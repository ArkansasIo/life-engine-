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
    /// <summary>The kinds of node a parse tree can contain.</summary>
    public enum NodeKind
    {
        Block,
        Literal,
        Name,
        Self,
        Assign,
        Binary,
        Unary,
        Call,
        MethodCall,
        Index,
        Field,
        If,
        While,
        For,
        Repeat,
        Function,
        Return,
        Break,
        Continue,
        Class,
        New,
        Super,
        Import,
        TableConstructor
    }

    /// <summary>
    /// One node of the parse tree. A single node type with a kind, rather than a
    /// class per construct: it keeps the tree walk in one place and makes the
    /// whole grammar visible in one file.
    /// </summary>
    public sealed class Node
    {
        private NodeKind m_kind;
        private int m_line;
        private string m_name;
        private string m_operator;
        private Value m_literal;
        private List<Node> m_children;
        private List<Node> m_body;
        private Node m_condition;
        private Node m_step;
        private Node m_initial;
        private Node m_base;
        private string m_className;

        public NodeKind Kind
        {
            get { return m_kind; }
        }

        public int Line
        {
            get { return m_line; }
        }

        /// <summary>A name: an identifier, a field, or a parameter.</summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>The operator, for a binary or unary node.</summary>
        public string Operator
        {
            get { return m_operator; }
        }

        /// <summary>The value, for a literal.</summary>
        public Value Literal
        {
            get { return m_literal; }
        }

        /// <summary>Operands: target and value, or left and right.</summary>
        public List<Node> Children
        {
            get { return m_children; }
        }

        /// <summary>The statements of a body.</summary>
        public List<Node> Body
        {
            get { return m_body; }
        }

        public Node Condition
        {
            get { return m_condition; }
            set { m_condition = value; }
        }

        /// <summary>The increment of a numeric for loop.</summary>
        public Node Step
        {
            get { return m_step; }
            set { m_step = value; }
        }

        /// <summary>The initial value of a numeric for loop.</summary>
        public Node Initial
        {
            get { return m_initial; }
            set { m_initial = value; }
        }

        /// <summary>The class a declaration extends.</summary>
        public Node Base
        {
            get { return m_base; }
            set { m_base = value; }
        }

        /// <summary>The class name of a declaration.</summary>
        public string ClassName
        {
            get { return m_className; }
            set { m_className = value; }
        }

        public Node(NodeKind kind, int line)
        {
            m_kind = kind;
            m_line = line;
            m_children = new List<Node>();
            m_body = new List<Node>();
            m_name = string.Empty;
            m_operator = string.Empty;
            m_className = string.Empty;
        }

        public Node Add(Node child)
        {
            if (child != null)
                m_children.Add(child);

            return this;
        }

        public Node SetOperator(string op)
        {
            m_operator = op;
            return this;
        }

        public Node SetName(string name)
        {
            m_name = name;
            return this;
        }

        public Node SetLiteral(Value value)
        {
            m_literal = value;
            return this;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(m_operator))
                return m_kind + "(" + m_operator + ")";

            if (!string.IsNullOrEmpty(m_name))
                return m_kind + "(" + m_name + ")";

            if (m_literal != null)
                return m_kind + "(" + m_literal.AsText() + ")";

            return m_kind.ToString();
        }
    }

    /// <summary>
    /// Reads tokens into a parse tree.
    ///
    /// The grammar is Lua's, with the additions that make the object layer
    /// readable: "class ... extends ... end", "new", "self", "super" and
    /// "import". Indentation is not part of the grammar — blocks are closed by
    /// "end", the way they are in Lua — so a script can be laid out however the
    /// author likes.
    /// </summary>
    public sealed class Parser
    {
        private List<Token> m_tokens;
        private int m_position;
        private string m_sourceName;

        public Parser(List<Token> tokens, string sourceName)
        {
            m_tokens = tokens;
            m_position = 0;
            m_sourceName = sourceName ?? "script";
        }

        /// <summary>Parses a whole script into a block node.</summary>
        public Node ParseProgram()
        {
            return ParseBlock();
        }

        // ------------------------------------------------------------------
        // Token handling
        // ------------------------------------------------------------------

        /// <summary>
        /// The current token, with leading newlines skipped.
        ///
        /// This is a true peek: it does NOT move the position. An earlier version
        /// called SkipNewLines here, which meant the newline that ends a
        /// statement was consumed before ExpectEndOfStatement could see it, and
        /// every multi-line script failed with "unexpected token after the
        /// statement".
        /// </summary>
        private Token Current()
        {
            int index = m_position;

            while (index < m_tokens.Count - 1 && m_tokens[index].Kind == TokenKind.NewLine)
                index++;

            return m_tokens[index];
        }

        /// <summary>The token at the position, without skipping anything.</summary>
        private Token Raw()
        {
            return m_tokens[m_position];
        }

        private Token Look(int ahead)
        {
            int index = m_position;

            while (index < m_tokens.Count - 1 && m_tokens[index].Kind == TokenKind.NewLine)
                index++;

            index += ahead;

            if (index >= m_tokens.Count)
                return m_tokens[m_tokens.Count - 1];

            return m_tokens[index];
        }

        private void SkipNewLines()
        {
            while (m_position < m_tokens.Count - 1 && m_tokens[m_position].Kind == TokenKind.NewLine)
                m_position++;
        }

        private bool AtEnd()
        {
            return m_tokens[m_position].Kind == TokenKind.End;
        }

        /// <summary>
        /// Reads the current token and steps past it.
        ///
        /// Newlines BEFORE the token are skipped, because a statement may be
        /// broken across lines. The newline AFTER the token is deliberately left
        /// in place: it is the statement terminator, and
        /// <see cref="ExpectEndOfStatement"/> has to be able to see it. Skipping
        /// it here was the bug that made every multi-line script fail with
        /// "unexpected token after the statement".
        /// </summary>
        /// <summary>
        /// Reads the token at the current position and steps past it.
        ///
        /// Leading newlines are skipped first, so a statement may be broken
        /// across lines. The newline that FOLLOWS the token is left in place for
        /// <see cref="ExpectEndOfStatement"/> to see — that is the statement
        /// terminator, and consuming it here was the bug that made every
        /// multi-line script fail with "unexpected token after the statement".
        /// </summary>
        private Token Advance()
        {
            Token token = m_tokens[m_position];

            if (m_position < m_tokens.Count - 1)
                m_position++;

            return token;
        }

        private bool Check(TokenKind kind, string text)
        {
            Token token = Current();

            if (token.Kind != kind)
                return false;

            if (text != null && token.Text != text)
                return false;

            return true;
        }

        private bool CheckKeyword(string word)
        {
            return Check(TokenKind.Keyword, word);
        }

        private bool CheckOperator(string op)
        {
            return Check(TokenKind.Operator, op);
        }

        private bool MatchKeyword(string word)
        {
            if (CheckKeyword(word))
            {
                Advance();
                return true;
            }

            return false;
        }

        /// <summary>
        /// True if the IMMEDIATE next token is the given operator, and consumes
        /// it.
        ///
        /// This reads the raw position rather than the next meaningful token,
        /// because an operator belongs to the line it is on: a newline between
        /// the name and the '=' means the statement ended, not that the '=' is
        /// still coming.
        /// </summary>
        private bool MatchOperator(string op)
        {
            Token raw = Raw();

            if (raw.Kind == TokenKind.Operator && raw.Text == op)
            {
                Advance();
                return true;
            }

            return false;
        }

        private Token Expect(TokenKind kind, string text)
        {
            Token token = Current();

            if (token.Kind != kind || (text != null && token.Text != text))
            {
                string wanted = text ?? kind.ToString();

                throw new ScriptError("Expected " + wanted + " but found '" + token.Text + "'",
                                      token.Line, token.Column);
            }

            return Advance();
        }

        private void ExpectKeyword(string word)
        {
            Token token = Current();

            if (token.Kind != TokenKind.Keyword || token.Text != word)
                throw new ScriptError("Expected '" + word + "' but found '" + token.Text + "'",
                                      token.Line, token.Column);

            Advance();
        }

        private void ExpectOperator(string op)
        {
            Token token = Current();

            if (token.Kind != TokenKind.Operator || token.Text != op)
                throw new ScriptError("Expected '" + op + "' but found '" + token.Text + "'",
                                      token.Line, token.Column);

            Advance();
        }

        /// <summary>
        /// Checks that a statement has ended, and consumes the terminator.
        ///
        /// This reads the RAW token rather than the next meaningful one: the
        /// newline IS the terminator, so skipping it would mean never seeing it.
        /// A statement may also end at a closing keyword, which is what makes
        /// "x = 1 end" legal without a newline before the end.
        /// </summary>
        private void ExpectEndOfStatement()
        {
            Token raw = Raw();

            if (raw.Kind == TokenKind.NewLine)
            {
                SkipNewLines();
                return;
            }

            if (raw.Kind == TokenKind.End)
                return;

            if (raw.Kind == TokenKind.Operator && raw.Text == ";")
            {
                Advance();
                SkipNewLines();
                return;
            }

            if (raw.Kind == TokenKind.Keyword &&
                (raw.Text == "end" || raw.Text == "else" || raw.Text == "elseif"))
                return;

            throw new ScriptError("Unexpected '" + raw.Text + "' after the statement",
                                  raw.Line, raw.Column);
        }

        // ------------------------------------------------------------------
        // Blocks and statements
        // ------------------------------------------------------------------

        private bool IsBlockEnd()
        {
            Token token = Current();

            if (token.Kind == TokenKind.End)
                return true;

            if (token.Kind != TokenKind.Keyword)
                return false;

            return token.Text == "end" || token.Text == "else" || token.Text == "elseif";
        }

        private Node ParseBlock()
        {
            Node block = new Node(NodeKind.Block, Current().Line);

            while (!AtEnd() && !IsBlockEnd())
            {
                // Newlines must be consumed physically: Current() skips them
                // virtually, so testing the token it returns for NewLine can
                // never succeed. Leaving m_position on a newline made Advance()
                // eat the newline instead of the first token of the statement,
                // which is why a statement beginning with 'self' failed.
                SkipNewLines();

                if (AtEnd() || IsBlockEnd())
                    break;

                Node statement = ParseStatement();

                if (statement != null)
                    block.Body.Add(statement);
            }

            return block;
        }

        private Node ParseStatement()
        {
            Token token = Current();
            int line = token.Line;

            if (token.Kind == TokenKind.Operator && token.Text == ";")
            {
                Advance();
                return null;
            }

            if (token.Kind == TokenKind.Keyword)
            {
                switch (token.Text)
                {
                    case "if": return ParseIf();
                    case "while": return ParseWhile();
                    case "for": return ParseFor();
                    case "function": return ParseFunctionStatement();
                    case "class": return ParseClass();
                    case "return": return ParseReturn();
                    case "break":
                        Advance();
                        ExpectEndOfStatement();
                        return new Node(NodeKind.Break, line);
                    case "continue":
                        Advance();
                        ExpectEndOfStatement();
                        return new Node(NodeKind.Continue, line);
                    case "import": return ParseImport();
                    case "local":
                        Advance();
                        return ParseAssignment(true);
                    case "print":
                        break;
                }
            }

            return ParseAssignment(false);
        }

        private Node ParseIf()
        {
            Token start = Expect(TokenKind.Keyword, "if");
            Node node = new Node(NodeKind.If, start.Line);

            node.Condition = ParseExpression();
            ExpectKeyword("then");

            node.Body.Add(ParseBlock());

            Node current = node;

            while (CheckKeyword("elseif"))
            {
                Token token = Advance();

                Node branch = new Node(NodeKind.If, token.Line);

                branch.Condition = ParseExpression();
                ExpectKeyword("then");
                branch.Body.Add(ParseBlock());

                current.Children.Add(branch);
                current = branch;
            }

            if (MatchKeyword("else"))
            {
                Node otherwise = new Node(NodeKind.Block, start.Line);

                otherwise.Body.Add(ParseBlock());

                current.Children.Add(otherwise);
            }

            ExpectKeyword("end");

            return node;
        }

        private Node ParseWhile()
        {
            Token start = Expect(TokenKind.Keyword, "while");
            Node node = new Node(NodeKind.While, start.Line);

            node.Condition = ParseExpression();
            ExpectKeyword("do");
            node.Body.Add(ParseBlock());
            ExpectKeyword("end");

            return node;
        }

        private Node ParseFor()
        {
            Token start = Expect(TokenKind.Keyword, "for");
            Node node = new Node(NodeKind.For, start.Line);

            Token name = Expect(TokenKind.Name, null);

            node.Name = name.Text;

            if (MatchKeyword("in"))
            {
                // Iterator form: for name in expression do ... end
                node.Initial = ParseExpression();
                ExpectKeyword("do");
                node.Body.Add(ParseBlock());
                ExpectKeyword("end");

                return node;
            }

            ExpectOperator("=");

            node.Initial = ParseExpression();
            ExpectOperator(",");

            node.Condition = ParseExpression();

            if (MatchOperator(","))
                node.Step = ParseExpression();

            ExpectKeyword("do");
            node.Body.Add(ParseBlock());
            ExpectKeyword("end");

            return node;
        }

        private Node ParseFunctionStatement()
        {
            Token start = Expect(TokenKind.Keyword, "function");

            Node node = new Node(NodeKind.Function, start.Line);

            Node target = ParseTarget();

            node.Children.Add(target);
            node.Name = target.Name;

            ParseParameters(node);
            node.Body.Add(ParseBlock());

            ExpectKeyword("end");

            return node;
        }

        private void ParseParameters(Node function)
        {
            ExpectOperator("(");

            // The parameter names are stored as name nodes in a synthetic list.
            Node parameters = new Node(NodeKind.Block, 0);

            if (!CheckOperator(")"))
            {
                do
                {
                    Token name = Expect(TokenKind.Name, null);

                    parameters.Body.Add(new Node(NodeKind.Name, name.Line).SetName(name.Text));
                }
                while (MatchOperator(","));
            }

            ExpectOperator(")");

            function.Children.Add(parameters);
        }

        private Node ParseClass()
        {
            Token start = Expect(TokenKind.Keyword, "class");
            Node node = new Node(NodeKind.Class, start.Line);

            Token name = Expect(TokenKind.Name, null);
            node.ClassName = name.Text;

            if (MatchKeyword("extends"))
            {
                Token parent = Expect(TokenKind.Name, null);

                node.Base = new Node(NodeKind.Name, parent.Line).SetName(parent.Text);
            }

            // Skip to the first statement of the body. Newlines must be consumed
            // physically here: Current() skips them virtually, so testing for a
            // NewLine token after calling it can never succeed, and the class
            // body's first line was left un-advanced — which made 'value = 0'
            // fail with "Expected '=' after the field name".
            SkipNewLines();

            while (!CheckKeyword("end") && !AtEnd())
            {
                Node member = ParseClassMember();

                if (member != null)
                    node.Body.Add(member);

                SkipNewLines();
            }

            ExpectKeyword("end");

            return node;
        }

        private Node ParseClassMember()
        {
            Token token = Current();

            if (token.Kind == TokenKind.Keyword && token.Text == "function")
            {
                Advance();

                Node function = new Node(NodeKind.Function, token.Line);
                Token name = Expect(TokenKind.Name, null);

                function.Name = name.Text;
                function.ClassName = "method";

                // A method has no written target, but the call paths expect the
                // parameters at Children[1] (a free function has its target at
                // Children[0]). A self node goes in that slot so a method and a
                // function have the same shape — without it the parameters were
                // never bound and every method argument read as nil.
                function.Children.Add(new Node(NodeKind.Self, token.Line));

                ParseParameters(function);
                function.Body.Add(ParseBlock());
                ExpectKeyword("end");

                return function;
            }

            if (token.Kind == TokenKind.Name)
            {
                // A field: name = expression
                Advance();

                if (!MatchOperator("="))
                    throw new ScriptError("Expected '=' after the field name '" + token.Text + "'",
                                          token.Line, token.Column);

                Node assignment = new Node(NodeKind.Assign, token.Line);
                assignment.ClassName = "field";
                assignment.Name = token.Text;
                assignment.Add(ParseExpression());

                ExpectEndOfStatement();

                return assignment;
            }

            if (token.Kind == TokenKind.NewLine)
            {
                Advance();
                return null;
            }

            throw new ScriptError("Unexpected '" + token.Text + "' in a class body",
                                  token.Line, token.Column);
        }

        private Node ParseReturn()
        {
            Token start = Expect(TokenKind.Keyword, "return");
            Node node = new Node(NodeKind.Return, start.Line);

            if (!IsStatementEnd())
                node.Add(ParseExpression());

            ExpectEndOfStatement();

            return node;
        }

        private bool IsStatementEnd()
        {
            Token token = Current();

            if (token.Kind == TokenKind.NewLine || token.Kind == TokenKind.End)
                return true;

            if (token.Kind == TokenKind.Operator && token.Text == ";")
                return true;

            if (token.Kind == TokenKind.Keyword &&
                (token.Text == "end" || token.Text == "else" || token.Text == "elseif"))
                return true;

            return false;
        }

        private Node ParseImport()
        {
            Token start = Expect(TokenKind.Keyword, "import");

            Node node = new Node(NodeKind.Import, start.Line);
            Token name = Expect(TokenKind.Name, null);

            node.Name = name.Text;
            ExpectEndOfStatement();

            return node;
        }

        private Node ParseAssignment(bool isLocal)
        {
            Token start = Current();
            Node target = ParseTarget();

            // '=' is consumed by MatchOperator, so it must be tested first and
            // remembered. Reading Current() afterwards to detect a compound
            // operator read the VALUE ('2' in "x = 2 + 3"), which then became the
            // operator of a bogus binary node — the "Unknown operator '2'" bug.
            string compound = null;

            if (MatchOperator("="))
            {
                compound = "=";
            }
            else
            {
                Token op = Current();

                if (op.Kind == TokenKind.Operator &&
                    (op.Text == "+=" || op.Text == "-=" || op.Text == "*=" || op.Text == "/="))
                {
                    compound = op.Text;
                    Advance();
                }
            }

            if (compound == null)
            {
                // Not an assignment: it must be a call on its own.
                if (target.Kind != NodeKind.Call && target.Kind != NodeKind.MethodCall)
                    throw new ScriptError("Unexpected '" + Current().Text + "'", start.Line, start.Column);

                ExpectEndOfStatement();

                return target;
            }

            Node assignment = new Node(NodeKind.Assign, start.Line);

            assignment.Name = isLocal ? "local" : "";
            assignment.Children.Add(target);

            Node value = ParseExpression();

            // a += b is rewritten as a = a + b, so the evaluator sees one form.
            if (compound != "=")
            {
                Node combined = new Node(NodeKind.Binary, start.Line);
                Node copy = new Node(target.Kind, start.Line).SetName(target.Name);

                for (int i = 0; i < target.Children.Count; i++)
                    copy.Children.Add(target.Children[i]);

                if (target.Kind == NodeKind.Field)
                    copy.ClassName = target.ClassName;

                combined.SetOperator(compound.Substring(0, 1));
                combined.Add(copy);
                combined.Add(value);

                value = combined;
            }

            assignment.Children.Add(value);
            ExpectEndOfStatement();

            return assignment;
        }

        /// <summary>
        /// Something that can be assigned to: a name, a field or an index.
        /// </summary>
        private Node ParseTarget()
        {
            Token start = Current();

            if (start.Kind == TokenKind.Keyword && start.Text == "self")
            {
                Advance();
                return ParseSuffixes(new Node(NodeKind.Self, start.Line));
            }

            // 'print' is a keyword but acts as a global function, so a statement
            // that begins with it is a call, not an assignment target.
            if (start.Kind == TokenKind.Keyword && start.Text == "print")
            {
                Advance();
                return ParseSuffixes(new Node(NodeKind.Name, start.Line).SetName(start.Text));
            }

            if (start.Kind != TokenKind.Name)
                throw new ScriptError("Expected a name but found '" + start.Text + "'",
                                      start.Line, start.Column);

            Advance();

            Node node = new Node(NodeKind.Name, start.Line).SetName(start.Text);

            return ParseSuffixes(node);
        }

        /// <summary>
        /// Applies .field, [index] and (call) to a base expression, left to
        /// right, so a.b.c[i](x) becomes a chain of nodes.
        /// </summary>
        private Node ParseSuffixes(Node node)
        {
            while (true)
            {
                Token token = Current();

                if (token.Kind == TokenKind.Operator && token.Text == ".")
                {
                    Advance();

                    Token name = Expect(TokenKind.Name, null);

                    Node field = new Node(NodeKind.Field, token.Line);

                    field.Name = name.Text;
                    field.Add(node);

                    node = field;
                    continue;
                }

                if (token.Kind == TokenKind.Operator && token.Text == "[")
                {
                    Advance();

                    Node index = new Node(NodeKind.Index, token.Line);

                    index.Add(node);
                    index.Add(ParseExpression());
                    ExpectOperator("]");

                    node = index;
                    continue;
                }

                if (token.Kind == TokenKind.Operator && token.Text == ":")
                {
                    Advance();

                    Token name = Expect(TokenKind.Name, null);

                    Node method = new Node(NodeKind.MethodCall, token.Line);

                    method.Name = name.Text;
                    method.Add(node);
                    ParseArguments(method);

                    node = method;
                    continue;
                }

                if (token.Kind == TokenKind.Operator && token.Text == "(")
                {
                    Node call = new Node(NodeKind.Call, token.Line);

                    call.Add(node);
                    ParseArguments(call);

                    node = call;
                    continue;
                }

                break;
            }

            return node;
        }

        private void ParseArguments(Node call)
        {
            ExpectOperator("(");

            if (!CheckOperator(")"))
            {
                do
                {
                    call.Add(ParseExpression());
                }
                while (MatchOperator(","));
            }

            ExpectOperator(")");
        }

        // ------------------------------------------------------------------
        // Expressions, by precedence
        // ------------------------------------------------------------------

        private Node ParseExpression()
        {
            return ParseOr();
        }

        private Node ParseOr()
        {
            Node left = ParseAnd();

            while (CheckKeyword("or"))
            {
                Token token = Advance();

                Node node = new Node(NodeKind.Binary, token.Line);

                node.SetOperator("or");
                node.Add(left);
                node.Add(ParseAnd());

                left = node;
            }

            return left;
        }

        private Node ParseAnd()
        {
            Node left = ParseComparison();

            while (CheckKeyword("and"))
            {
                Token token = Advance();

                Node node = new Node(NodeKind.Binary, token.Line);

                node.SetOperator("and");
                node.Add(left);
                node.Add(ParseComparison());

                left = node;
            }

            return left;
        }

        private Node ParseComparison()
        {
            Node left = ParseConcat();

            while (true)
            {
                Token token = Current();

                if (token.Kind != TokenKind.Operator)
                    break;

                if (token.Text != "==" && token.Text != "~=" && token.Text != "!=" &&
                    token.Text != "<" && token.Text != ">" &&
                    token.Text != "<=" && token.Text != ">=")
                    break;

                Advance();

                Node node = new Node(NodeKind.Binary, token.Line);

                node.SetOperator(token.Text);
                node.Add(left);
                node.Add(ParseConcat());

                left = node;
            }

            return left;
        }

        private Node ParseConcat()
        {
            Node left = ParseAdditive();

            while (CheckOperator(".."))
            {
                Token token = Advance();

                Node node = new Node(NodeKind.Binary, token.Line);

                node.SetOperator("..");
                node.Add(left);
                node.Add(ParseAdditive());

                left = node;
            }

            return left;
        }

        private Node ParseAdditive()
        {
            Node left = ParseMultiplicative();

            while (true)
            {
                Token token = Current();

                if (token.Kind != TokenKind.Operator || (token.Text != "+" && token.Text != "-"))
                    break;

                Advance();

                Node node = new Node(NodeKind.Binary, token.Line);

                node.SetOperator(token.Text);
                node.Add(left);
                node.Add(ParseMultiplicative());

                left = node;
            }

            return left;
        }

        private Node ParseMultiplicative()
        {
            Node left = ParseUnary();

            while (true)
            {
                Token token = Current();

                if (token.Kind != TokenKind.Operator)
                    break;

                if (token.Text != "*" && token.Text != "/" && token.Text != "%")
                    break;

                Advance();

                Node node = new Node(NodeKind.Binary, token.Line);

                node.SetOperator(token.Text);
                node.Add(left);
                node.Add(ParseUnary());

                left = node;
            }

            return left;
        }

        private Node ParseUnary()
        {
            Token token = Current();

            if (token.Kind == TokenKind.Keyword && token.Text == "not")
            {
                Advance();

                Node node = new Node(NodeKind.Unary, token.Line);

                node.SetOperator("not");
                node.Add(ParseUnary());

                return node;
            }

            if (token.Kind == TokenKind.Operator && token.Text == "#")
            {
                Advance();

                Node node = new Node(NodeKind.Unary, token.Line);

                node.SetOperator("#");
                node.Add(ParseUnary());

                return node;
            }

            if (token.Kind == TokenKind.Operator && (token.Text == "-" || token.Text == "+"))
            {
                Advance();

                Node node = new Node(NodeKind.Unary, token.Line);

                node.SetOperator(token.Text);
                node.Add(ParseUnary());

                return node;
            }

            if (token.Kind == TokenKind.Keyword && token.Text == "new")
            {
                Advance();

                Token name = Expect(TokenKind.Name, null);

                Node created = new Node(NodeKind.New, token.Line);

                created.Name = name.Text;

                Node call = new Node(NodeKind.New, token.Line);

                call.Name = name.Text;
                ParseArguments(call);

                for (int i = 0; i < call.Children.Count; i++)
                    created.Add(call.Children[i]);

                return ParseSuffixes(created);
            }

            return ParsePower();
        }

        private Node ParsePower()
        {
            Node left = ParsePrimary();

            if (CheckOperator("^"))
            {
                Token token = Advance();

                Node node = new Node(NodeKind.Binary, token.Line);

                node.SetOperator("^");
                node.Add(left);

                // Right associative, so 2^3^2 is 2^(3^2).
                node.Add(ParseUnary());

                return node;
            }

            return left;
        }

        private Node ParsePrimary()
        {
            Token token = Current();

            switch (token.Kind)
            {
                case TokenKind.Number:
                    Advance();
                    return new Node(NodeKind.Literal, token.Line).SetLiteral(Value.Of(token.Number));

                case TokenKind.String:
                    Advance();
                    return new Node(NodeKind.Literal, token.Line).SetLiteral(Value.Of(token.Text));

                case TokenKind.Keyword:
                    if (token.Text == "true")
                    {
                        Advance();
                        return new Node(NodeKind.Literal, token.Line).SetLiteral(Value.Of(true));
                    }

                    if (token.Text == "false")
                    {
                        Advance();
                        return new Node(NodeKind.Literal, token.Line).SetLiteral(Value.Of(false));
                    }

                    if (token.Text == "nil" || token.Text == "null")
                    {
                        Advance();
                        return new Node(NodeKind.Literal, token.Line).SetLiteral(Value.Nil());
                    }

                    if (token.Text == "self")
                    {
                        Advance();
                        return ParseSuffixes(new Node(NodeKind.Self, token.Line));
                    }

                    if (token.Text == "print")
                    {
                        // 'print' is a keyword in the lexer but behaves as a
                        // global function: print(x) is a call, not syntax.
                        Advance();
                        return ParseSuffixes(new Node(NodeKind.Name, token.Line).SetName(token.Text));
                    }

                    if (token.Text == "super")
                    {
                        Advance();

                        Node node = new Node(NodeKind.Super, token.Line);

                        if (CheckOperator(".") || CheckOperator(":"))
                        {
                            Advance();

                            Token name = Expect(TokenKind.Name, null);
                            node.Name = name.Text;
                        }

                        return node;
                    }

                    if (token.Text == "function")
                    {
                        Advance();

                        Node function = new Node(NodeKind.Function, token.Line);

                        function.Name = "anonymous";
                        ParseParameters(function);
                        function.Body.Add(ParseBlock());
                        ExpectKeyword("end");

                        return function;
                    }
                    break;

                case TokenKind.Name:
                    Advance();
                    return ParseSuffixes(new Node(NodeKind.Name, token.Line).SetName(token.Text));

                case TokenKind.Operator:
                    if (token.Text == "(")
                    {
                        Advance();

                        Node inner = ParseExpression();

                        ExpectOperator(")");

                        return ParseSuffixes(inner);
                    }

                    if (token.Text == "{")
                        return ParseTable();

                    if (token.Text == "[")
                    {
                        // A lone [index] is only valid with a base, so this is
                        // really a table with one unnamed entry.
                        return ParseTable();
                    }
                    break;
            }

            throw new ScriptError("Unexpected '" + token.Text + "' in an expression",
                                  token.Line, token.Column);
        }

        private Node ParseTable()
        {
            Token start = Expect(TokenKind.Operator, "{");

            Node table = new Node(NodeKind.TableConstructor, start.Line);

            while (!CheckOperator("}") && !AtEnd())
            {
                if (CheckOperator(","))
                {
                    Advance();
                    continue;
                }

                Token token = Current();

                if (token.Kind == TokenKind.Name && Look(1).Kind == TokenKind.Operator &&
                    Look(1).Text == "=")
                {
                    Advance();
                    Advance();

                    Node entry = new Node(NodeKind.Assign, token.Line);

                    entry.Name = token.Text;
                    entry.Add(ParseExpression());
                    table.Body.Add(entry);
                }
                else if (token.Kind == TokenKind.Operator && token.Text == "[")
                {
                    Advance();

                    Node key = ParseExpression();

                    ExpectOperator("]");
                    ExpectOperator("=");

                    Node entry = new Node(NodeKind.Assign, token.Line);

                    entry.Add(key);
                    entry.Add(ParseExpression());
                    table.Body.Add(entry);
                }
                else
                {
                    table.Body.Add(ParseExpression());
                }

                if (!CheckOperator(",") && !CheckOperator("}"))
                    break;
            }

            ExpectOperator("}");

            return table;
        }

        public override string ToString()
        {
            return "Parser for " + m_sourceName;
        }
    }
}
