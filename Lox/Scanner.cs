using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    internal class Scanner
    {
        private int start = 0;
        private int current = 0;
        private int line = 1;

        private string _source;
        private List<Token> _tokens = new List<Token>();

        private static Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
        {
            { "and", TokenType.AND },
            { "class", TokenType.CLASS },
            { "else", TokenType.ELSE },
            { "false", TokenType.FALSE },
            { "for", TokenType.FOR },
            { "fun", TokenType.FUN },
            { "if", TokenType.IF },
            { "nil", TokenType.NIL },
            { "or", TokenType.OR },
            { "print", TokenType.PRINT },
            { "return", TokenType.RETURN },
            { "super", TokenType.SUPER },
            { "this", TokenType.THIS },
            { "true", TokenType.TRUE },
            { "var", TokenType.VAR },
            { "while", TokenType.WHILE },
        };

        public Scanner(string source)
        {
            _source = source;
        }

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                start = current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, line));
            return _tokens;
        }

        private void ScanToken()
        {
            var c = Advance();
            switch (c)
            {
                case '(':
                    AddToken(TokenType.LEFT_PAREN); break;
                case ')':
                    AddToken(TokenType.RIGHT_PAREN); break;
                case '{':
                    AddToken(TokenType.LEFT_BRACE); break;
                case '}':
                    AddToken(TokenType.RIGHT_BRACE); break;
                case ',':
                    AddToken(TokenType.COMMA); break;
                case '.':
                    AddToken(TokenType.DOT); break;
                case '-':
                    AddToken(TokenType.MINUS); break;
                case '+':
                    AddToken(TokenType.PLUS); break;
                case ';':
                    AddToken(TokenType.SEMICOLON); break;
                case '*':
                    AddToken(TokenType.STAR); break;
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '/':
                    if (Match('/'))
                    {
                        while (Peek() != '\n' && IsAtEnd()) Advance();
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case ' ':
                case '\t':
                case '\r':
                    break;
                case '\n':
                    line++;
                    break;
                case '"': String(); break;
                default:
                    if (IsDigit(c)) {
                        Number();
                    }
                    else if (IsAlpha(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        Program.Error(line, "Unexpected Charactor");
                    }
                    break;
            }
        }

        private bool IsAtEnd() => current >= _source.Length;

        private char Advance() {
           return _source[current++];
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }
        
        private void Identifier()
        {
            while (IsAlphaNumeric(Peek()))
            {
                Advance();
            }

            TokenType? type = null;

            var text = _source.Substring(start, current - start);
            if (Keywords.ContainsKey(text))
            {
               type = Keywords[text];
            }
            else
            {
                type = TokenType.IDENTIFIER;
            }
            
            AddToken(type.Value);            
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z')
                || (c >= 'A' && c <= 'Z')
                || c == '-';
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private void Number()
        {
            while (IsDigit(Peek()))
            {
                Advance();
            }

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();
                while (IsDigit(Peek())) Advance();
            }

            AddToken(TokenType.NUMBER, double.Parse(_source.Substring(start, current - start)));
        }

        private void String()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') line++;
                Advance();
            }
            if (IsAtEnd())
            {
                Program.Error(line, "Unterminated String");
                return;
            }

            Advance();

            var value = _source.Substring(start + 1, current - start - 2);
            AddToken(TokenType.STRING, value);
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return _source[current];
        }

        private char PeekNext()
        {
            if (current + 1 >= _source.Length) return '\0';
            return _source[current + 1];
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[current] != expected) return false;
            current++;
            return true;
        }

        private void AddToken(TokenType type, object literal)
        {
            var text = _source.Substring(start, current - start);
            _tokens.Add(new Token(type, text, literal, line));
        }
    }
}
