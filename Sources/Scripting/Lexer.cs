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
    /// What a piece of source text is made of.
    /// </summary>
    public enum TokenKind
    {
        End,
        NewLine,
        Name,
        Number,
        String,
        Keyword,
        Operator,
        /// <summary>"--" to the end of the line.</summary>
        Comment
    }

    /// <summary>One token: what it is, what it says, and where it came from.</summary>
    public sealed class Token
    {
        private TokenKind m_kind;
        private string m_text;
        private double m_number;
        private int m_line;
        private int m_column;

        public TokenKind Kind
        {
            get { return m_kind; }
        }

        /// <summary>The source text of the token.</summary>
        public string Text
        {
            get { return m_text; }
        }

        /// <summary>The value, if it is a number.</summary>
        public double Number
        {
            get { return m_number; }
        }

        public int Line
        {
            get { return m_line; }
        }

        public int Column
        {
            get { return m_column; }
        }

        public Token(TokenKind kind, string text, double number, int line, int column)
        {
            m_kind = kind;
            m_text = text;
            m_number = number;
            m_line = line;
            m_column = column;
        }

        public override string ToString()
        {
            return m_kind + " '" + m_text + "' at " + m_line + ":" + m_column;
        }
    }

    /// <summary>Raised for anything the language cannot read.</summary>
    public sealed class ScriptError : Exception
    {
        private int m_line;
        private int m_column;

        public int Line
        {
            get { return m_line; }
        }

        public int Column
        {
            get { return m_column; }
        }

        public ScriptError(string message, int line, int column)
            : base(message + " (line " + line + ", column " + column + ")")
        {
            m_line = line;
            m_column = column;
        }

        public ScriptError(string message, int line, int column, Exception inner)
            : base(message + " (line " + line + ", column " + column + ")", inner)
        {
            m_line = line;
            m_column = column;
        }
    }

    /// <summary>
    /// Turns source text into tokens.
    ///
    /// Indentation is significant, so a newline is a real token: the parser uses
    /// it to end a statement and the indentation of the following line to decide
    /// whether a block has ended.
    /// </summary>
    public sealed class Lexer
    {
        private static readonly Dictionary<string, bool> KeywordTable = BuildKeywords();

        private string m_source;
        private int m_position;
        private int m_line;
        private int m_column;
        private List<Token> m_tokens;

        /// <summary>Every word that is syntax rather than a name.</summary>
        public static IList<string> Keywords
        {
            get
            {
                List<string> words = new List<string>(KeywordTable.Keys);
                words.Sort();
                return words;
            }
        }

        private static Dictionary<string, bool> BuildKeywords()
        {
            Dictionary<string, bool> table = new Dictionary<string, bool>(StringComparer.Ordinal);

            string[] words =
            {
                "and", "break", "class", "continue", "do", "else", "elseif", "end",
                "extends", "false", "for", "from", "function", "if", "import", "in",
                "local", "new", "nil", "not", "null", "or", "print", "return",
                "self", "super", "then", "this", "true", "while"
            };

            for (int i = 0; i < words.Length; i++)
                table[words[i]] = true;

            return table;
        }

        public Lexer(string source)
        {
            m_source = source == null ? string.Empty : source;
            m_position = 0;
            m_line = 1;
            m_column = 1;
            m_tokens = new List<Token>();
        }

        /// <summary>Reads the whole source and returns the tokens, ending with End.</summary>
        public List<Token> Tokenise()
        {
            m_tokens.Clear();

            while (!AtEnd())
            {
                char letter = Peek();

                if (letter == '\r')
                {
                    Advance();
                    continue;
                }

                if (letter == '\n')
                {
                    Add(TokenKind.NewLine, "\n", 0.0);
                    Advance();
                    m_line++;
                    m_column = 1;
                    continue;
                }

                if (letter == ' ' || letter == '\t')
                {
                    Advance();
                    continue;
                }

                if (letter == '#')
                {
                    SkipComment();
                    continue;
                }

                if (letter == '-' && PeekNext() == '-')
                {
                    SkipComment();
                    continue;
                }

                if (char.IsDigit(letter) || (letter == '.' && char.IsDigit(PeekNext())))
                {
                    ReadNumber();
                    continue;
                }

                if (letter == '"' || letter == '\'')
                {
                    ReadString(letter);
                    continue;
                }

                if (char.IsLetter(letter) || letter == '_')
                {
                    ReadName();
                    continue;
                }

                ReadOperator();
            }

            m_tokens.Add(new Token(TokenKind.End, string.Empty, 0.0, m_line, m_column));

            return m_tokens;
        }

        private bool AtEnd()
        {
            return m_position >= m_source.Length;
        }

        private char Peek()
        {
            if (AtEnd())
                return '\0';

            return m_source[m_position];
        }

        private char PeekNext()
        {
            if (m_position + 1 >= m_source.Length)
                return '\0';

            return m_source[m_position + 1];
        }

        private char PeekAt(int offset)
        {
            if (m_position + offset >= m_source.Length)
                return '\0';

            return m_source[m_position + offset];
        }

        private void Advance()
        {
            m_position++;
            m_column++;
        }

        private void Add(TokenKind kind, string text, double number)
        {
            m_tokens.Add(new Token(kind, text, number, m_line, m_column));
        }

        private void SkipComment()
        {
            while (!AtEnd() && Peek() != '\n')
                Advance();
        }

        private void ReadNumber()
        {
            int startColumn = m_column;
            int start = m_position;
            bool hasDot = false;

            while (!AtEnd())
            {
                char letter = Peek();

                if (char.IsDigit(letter))
                {
                    Advance();
                    continue;
                }

                if (letter == '.' && !hasDot && char.IsDigit(PeekNext()))
                {
                    hasDot = true;
                    Advance();
                    continue;
                }

                // Exponent notation: 1e-9 and the like.
                if ((letter == 'e' || letter == 'E') &&
                    (char.IsDigit(PeekNext()) || PeekNext() == '-' || PeekNext() == '+'))
                {
                    Advance();

                    if (Peek() == '-' || Peek() == '+')
                        Advance();

                    continue;
                }

                break;
            }

            string text = m_source.Substring(start, m_position - start);
            double value;

            if (!double.TryParse(text, System.Globalization.NumberStyles.Float,
                                 System.Globalization.CultureInfo.InvariantCulture, out value))
                throw new ScriptError("Not a number: " + text, m_line, startColumn);

            m_tokens.Add(new Token(TokenKind.Number, text, value, m_line, startColumn));
        }

        private void ReadString(char quote)
        {
            int startColumn = m_column;

            Advance();

            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            while (!AtEnd() && Peek() != quote)
            {
                char letter = Peek();

                if (letter == '\\')
                {
                    Advance();
                    char escape = Peek();

                    switch (escape)
                    {
                        case 'n': builder.Append('\n'); break;
                        case 't': builder.Append('\t'); break;
                        case 'r': builder.Append('\r'); break;
                        case '\\': builder.Append('\\'); break;
                        case '"': builder.Append('"'); break;
                        case '\'': builder.Append('\''); break;
                        default: builder.Append(escape); break;
                    }

                    Advance();
                    continue;
                }

                if (letter == '\n')
                    throw new ScriptError("Unterminated string", m_line, startColumn);

                builder.Append(letter);
                Advance();
            }

            if (AtEnd())
                throw new ScriptError("Unterminated string", m_line, startColumn);

            Advance();

            m_tokens.Add(new Token(TokenKind.String, builder.ToString(), 0.0, m_line, startColumn));
        }

        private void ReadName()
        {
            int startColumn = m_column;
            int start = m_position;

            while (!AtEnd() && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
                Advance();

            string text = m_source.Substring(start, m_position - start);

            bool isKeyword = KeywordTable.ContainsKey(text);

            m_tokens.Add(new Token(isKeyword ? TokenKind.Keyword : TokenKind.Name,
                                   text, 0.0, m_line, startColumn));
        }

        private void ReadOperator()
        {
            int startColumn = m_column;
            char letter = Peek();

            // Two and three character operators first, so "==" beats "=".
            string three = letter.ToString() + PeekAt(1) + PeekAt(2);
            string two = letter.ToString() + PeekAt(1);

            string[] triples = { "..." };

            for (int i = 0; i < triples.Length; i++)
            {
                if (three == triples[i])
                {
                    Advance(); Advance(); Advance();
                    Add(TokenKind.Operator, three, 0.0);
                    return;
                }
            }

            string[] pairs = { "==", "~=", "!=", "<=", ">=", "..", "->", "+=", "-=", "*=", "/=" };

            for (int i = 0; i < pairs.Length; i++)
            {
                if (two == pairs[i])
                {
                    Advance(); Advance();
                    Add(TokenKind.Operator, two, 0.0);
                    return;
                }
            }

            string singles = "+-*/%^#=<>(){}[];:,.&|!";

            if (singles.IndexOf(letter) >= 0)
            {
                Advance();
                Add(TokenKind.Operator, letter.ToString(), 0.0);
                return;
            }

            throw new ScriptError("Unexpected character '" + letter + "'", m_line, startColumn);
        }
    }
}
