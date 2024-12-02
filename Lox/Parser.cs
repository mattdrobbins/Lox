using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lox
{
    public class Parser
    {
        private class ParseException : Exception
        {

        }

        private List<Token> _tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public List<Stmt> Parse()
        {
            List<Stmt> statements = [];

            while (!IsAtEnd())
            {
                statements.Add(Decleration());
            }

            return statements;
        }

        private Expr Expression()
        {
            return Assignment();
        }

        private Stmt Decleration()
        {
            try
            {
                if (Match(TokenType.CLASS)) return ClassDecleration();
                if (Match(TokenType.FUN)) return Function("function");
                if (Match(TokenType.VAR)) return VarDecleration();
                return Statement();
            }
            catch (ParseException e)
            {
                Synchronise();
                return null;
            }
        }

        private Stmt ClassDecleration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect class name");
            Expr.Variable superclass = null;

            if (Match(TokenType.LESS))
            {
                Consume(TokenType.IDENTIFIER, "Expect superclass name");
                superclass = new Expr.Variable(Previous());
            }
            
            Consume(TokenType.LEFT_BRACE, "Expect { before class body");

            List<Stmt.Function> methods = new List<Stmt.Function>();

            while(!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                methods.Add(Function("method"));
            }

            Consume(TokenType.RIGHT_BRACE, "Expect } after class body");

            return new Stmt.Class(name, superclass, methods);
        }

        private Stmt Statement()
        {
            if (Match(TokenType.FOR))
            {

            }
            if (Match(TokenType.IF))
            {
                return IfStatement();
            }
            if (Match(TokenType.PRINT))
            {
                return PrintStatement();
            }
            if (Match(TokenType.RETURN))
            {
                return ReturnStatement();
            }
            if (Match(TokenType.WHILE))
            {
                return WhileStatement();
            }
            if (Match(TokenType.LEFT_BRACE))
            {
                return new Stmt.Block(Block());
            }

            return ExpressionStatment();
        }

        private Stmt ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expected ( after for");

            Stmt initialiser;
            if (Match(TokenType.SEMICOLON))
            {
                initialiser = null;
            }
            else if (Match(TokenType.VAR))
            {
                initialiser = VarDecleration();
            }
            else
            {
                initialiser = ExpressionStatment();
            }
            Expr condition = null;
            if (!Check(TokenType.SEMICOLON))
            {
                condition = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expect ; after loop");

            Expr increment = null;
            if (!Check(TokenType.SEMICOLON))
            {
                increment = Expression();
            }

            Consume(TokenType.RIGHT_PAREN, "Expect ) after clauses");

            var body = Statement();

            if (increment != null)
            {
                body = new Stmt.Block(new List<Stmt> { body, new Stmt.Expression(increment) });
            }

            if (condition == null)
            {
                condition = new Expr.Literal(true);
            }
            body = new Stmt.While(condition, body);

            if (initialiser != null)
            {
                body = new Stmt.Block(new List<Stmt> { initialiser, body });
            }

            return body;
        }

        private Stmt IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect ( after if");
            var condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ) after if");

            var thenBranch = Statement();
            Stmt elseBranch = null;
            if (Match(TokenType.ELSE))
            {
                elseBranch = Statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private Stmt ExpressionStatment()
        {
            var value = Expression();
            Consume(TokenType.SEMICOLON, "Expect \';\' after expression.");
            return new Stmt.Expression(value);
        }

        private Stmt.Function Function(string kind)
        {
            Token name = Consume(TokenType.IDENTIFIER, $"expect {kind} name ");
            Consume(TokenType.LEFT_PAREN, $"expect ( after {kind} name");
            var _params = new List<Token>();
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (_params.Count >= 255)
                    {
                        Error(Peek(), "Can't have more than 255 parameters");
                    }

                    _params.Add(Consume(TokenType.IDENTIFIER, "expect parameter name"));
                }
                while (Match(TokenType.COMMA));
            }

            Consume(TokenType.RIGHT_PAREN, "Expect ) after parameters");
            Consume(TokenType.LEFT_BRACE, "Expect { before " + $"{kind}" + " body.");

            List<Stmt> body = Block();

            return new Stmt.Function(name, _params, body);
        }

        private List<Stmt> Block()
        {
            List<Stmt> stmts = new List<Stmt>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                stmts.Add(Decleration());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect } after block");
            return stmts;
        }

        private Expr Assignment()
        {
            Expr exp = Equality();

            if (Match(TokenType.EQUAL))
            {
                Token equals = Previous();
                Expr value = Assignment();

                if (exp is Expr.Variable v)
                {
                    Token name = v.name;
                    return new Expr.Assign(name, exp);
                } else if (exp is  Expr.Get g)
                {
                    return new Expr.Set(g.obj, g.name, value);
                }

                Error(equals, "Invalid assignment target");
            }

            return exp;
        }

        private Expr Or()
        {
            Expr expr = And();

            while (Match(TokenType.OR))
            {
                var _operator = Previous();
                var right = Equality();
                expr = new Expr.Logical(expr, _operator, right);
            }

            return expr;
        }

        private Expr And()
        {
            var expr = Equality();

            while (Match(TokenType.AND))
            {
                var _operator = Previous();
                var right = Equality();
                expr = new Expr.Logical(expr, _operator, right);
            }

            return expr;
        }

        private Stmt PrintStatement()
        {
            var value = Expression();
            Consume(TokenType.SEMICOLON, "Expect \';\' after expression.");
            return new Stmt.Print(value);
        }

        private Stmt ReturnStatement()
        {
            Token keyword = Previous();
            Expr value = null;
            if (!Check(TokenType.SEMICOLON))
            {
                value = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ; after return");
            return new Stmt.Return(keyword, value);
        }
        private Stmt VarDecleration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name");

            Expr initialiser = null;
            if (Match(TokenType.EQUAL))
            {
                initialiser = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect semicolon after variable decleration");
            return new Stmt.Var(name, initialiser);
        }

        private Stmt WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect ( after while");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ) after condition");
            var body = Statement();

            return new Stmt.While(condition, body);
        }

        // equality -> comparison ( ( "!=" | "==" ) comparison *); 
        private Expr Equality() {
            Expr expr = Comparison();
            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token _operator = Previous();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, _operator, right); 
            }

            return expr;
        }

        private Expr Comparison()
        {
            Expr expr = Term();

            while (Match(TokenType.GREATER, 
                TokenType.GREATER_EQUAL, 
                TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token _operator = Previous();
                Expr right = Term();
                expr = new Expr.Binary(expr, _operator, right);

            }

            return expr;
        }

        private Expr Term()
        {
            var expr = Factor();

            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                Token _operator = Previous();
                Expr right = Factor();
                expr = new Expr.Binary(expr, _operator, right);
            }

            return expr;
        }

        private Expr Factor()
        {
            Expr expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                Token _operator = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, _operator, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                var _operator = Previous();
                Expr right = Unary();
                return new Expr.Unary(_operator, right);
            }

            return Call();
        }

        private Expr Call()
        {
            var expr = Primary();

            while (true)
            {
                if (Match(TokenType.LEFT_PAREN))
                {
                    expr = FinishCall(expr);
                } 
                else if (Match(TokenType.DOT))
                {
                    var name = Consume(TokenType.IDENTIFIER, "Expect property name after .");
                    expr = new Expr.Get(expr, name);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            var arguments = new List<Expr>();

            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count >= 255)
                    {
                        Error(Peek(), "Can't have more than 255 arguments");
                    }

                    arguments.Add(Expression());
                }
                while (Match(TokenType.COMMA));
            }

            var parem = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments");

            return new Expr.Call(callee, parem, arguments);
        }

        private Expr Primary()
        {
            if (Match(TokenType.FALSE)) return new Expr.Literal(false);
            if (Match(TokenType.TRUE)) return new Expr.Literal(true);
            if (Match(TokenType.NIL)) return new Expr.Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(Previous()._literal);
            }

            if (Match(TokenType.SUPER))
            {
                Token keyword = Previous();
                Consume(TokenType.DOT, "Expect . agter super");
                var method = Consume(TokenType.IDENTIFIER, "expect superclass method name");

                return new Expr.Super(keyword, method);
            }

            if (Match(TokenType.THIS))
            {
                return new Expr.This(Previous());
            }

            if (Match(TokenType.IDENTIFIER))
            {
                return new Expr.Variable(Previous());
            }

            if (Match(TokenType.LEFT_PAREN))
            {
                Expr exp = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression");
                return new Expr.Grouping(exp);
            }

            throw Error(Peek(), "Expect expression.");
        }        

        private bool Match(params TokenType[] tokenTypes)
        {
            foreach (TokenType tokenType in tokenTypes)
            {
                if (Check(tokenType))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private Token Consume(TokenType tokenType, string message)
        {
            if (Check(tokenType))
            {
                return Advance();
            }

            throw Error(Peek(), message);
        }

        private ParseException Error(Token token, String message)
        {
            Program.Error(token, message);
            return new ParseException();
        }

        private void Synchronise()
        {
            Advance();
            while (!IsAtEnd())
            {
                if (Previous()._type == TokenType.SEMICOLON)
                {
                    return;
                }

                switch (Peek()._type)
                {
                    case TokenType.CLASS:
                    case TokenType.FOR:
                    case TokenType.FUN:
                    case TokenType.IF:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                    case TokenType.VAR:
                    case TokenType.WHILE:
                        return;
                }

                Advance();
            }
        }

        private bool Check(TokenType tokenType)
        {
            if (IsAtEnd()) return false;
            return Peek()._type == tokenType;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) current++;
            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek()._type == TokenType.EOF;
        }

        private Token Peek()
        {
            return _tokens[current];
        }

        private Token Previous()
        {
            return _tokens[current - 1];
        }
    }
}
