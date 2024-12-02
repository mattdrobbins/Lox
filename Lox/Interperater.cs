using Lox.Callables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class Interperater : Expr.Visitor<object>, Stmt.Visitor<object>
    {
        private Environment env = new Environment();        
        
        public Environment Globals { get; private set; } = new Environment();
        private Dictionary<Expr, int> _locals = new();


        public Interperater()
        {
            Globals.Define("clock", new ClockCallable());    
        }

        public void Interperate(List<Stmt> statments)
        {
            try
            {
                foreach (Stmt statment in statments)
                {
                    Execute(statment);
                }
            }

            catch (RuntimeException e)
            {
                Program.RuntimeError(e);
            }
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        public void Resolve(Expr expr, int depth)
        {
            _locals[expr] = depth;
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            Object value = Evaluate(expr.value);

            if (_locals.ContainsKey(expr))
            {
                env.AssignAt(_locals[expr], expr.name, value);
            }
            else
            {
                Globals.Assign(expr.name, value);
            }

            return value;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch (expr._operator._type)
            {
                case TokenType.GREATER:
                    CheckNumberOperands(expr._operator, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr._operator, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr._operator, left, right);
                    return (double)left <= (double)right;
                case TokenType.LESS:
                    CheckNumberOperands(expr._operator, left, right);
                    return (double)left < (double)right;
                case TokenType.BANG_EQUAL: return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL: return IsEqual(left, right);
                case TokenType.MINUS:
                    CheckNumberOperands(expr._operator, left, right);
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    CheckNumberOperands(expr._operator, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(expr._operator, left, right);
                    return (double)left * (double)right;
                case TokenType.PLUS:
                    if (left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }
                    if (left is string && right is string)
                    {
                        return (string)left + (string)right;
                    }

                    throw new RuntimeException(expr._operator,
                        "Operands must be two numbers or two strings");
            }

            //unreachable
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            var callee = Evaluate(expr.callee);
            
            List<object> args = new List<object>();

            foreach (var arg in expr.arguments)
            {
                args.Add(Evaluate(arg));
            }

            if (!(callee is ILoxCallable))
            {
                throw new RuntimeException(expr.paren, "Call only call functions and classes");
            }

            ILoxCallable function = (ILoxCallable)callee;

            if (args.Count != function.Arity())
            {
                throw new RuntimeException(expr.paren, $"Expected {function.Arity()} arguments but got " +
                    $"{args.Count}");
            }

            return function.Call(this,  args);
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            var _object = Evaluate(expr.obj);
            if (_object is LoxInstance)
            {
                return ((LoxInstance)_object).Get(expr.name);
            }

            throw new RuntimeException(expr.name, "Only instances have properties");
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.left);

            if (expr._operator._type == TokenType.OR)
            {
                if (IsTruthy(left)) return left;
            }
            else if (!IsTruthy(left)) return left;

            return Evaluate(expr.right);
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            var _object = Evaluate(expr.obj);

            if (_object is not LoxInstance)
            {
                throw new RuntimeException(expr.name, "Only instances have fields");
            }

            var value = Evaluate(expr.value);
            ((LoxInstance)_object).Set(expr.name, value);
            return value;
        }

        public object VisitSuperExpr(Expr.Super expr)
        {
            int distance = _locals[expr];
            LoxClass superclass = ((LoxClass)(env.GetAt(distance, "super")));
            LoxInstance _object = (LoxInstance)env.GetAt(distance - 1, "this");

            LoxFunction method = superclass.FindMethod(expr.method._lexeme);

            if (method == null)
            {
                throw new RuntimeException(expr.method, "undefined property " + expr.method._lexeme);
            }

            return method.Bind(_object);
        }

        public object VisitThisExpr(Expr.This expr)
        {
            return LookUpVariable(expr.keyword, expr);
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch (expr._operator._type)
            {
                case TokenType.BANG:
                    return !IsTruthy(right);
                case TokenType.MINUS:
                    CheckNumberOperand(expr._operator, right);
                    return -(double)right;
            }

            // unreachable
            return null;
        }

        private void CheckNumberOperand(Token _operator, object operand)
        {
            if (operand is double)
            {
                return;
            }

            throw new RuntimeException(_operator, "Operand must be a number");
        }

        private void CheckNumberOperands(Token _operator, object left, object right)
        {
            if (left is double && right is double)
            {
                return;
            }

            throw new RuntimeException(_operator, "Operands must be numbers");
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return LookUpVariable(expr.name, expr);
        }

        private object LookUpVariable(Token name, Expr expr)
        {
            if (_locals.ContainsKey(expr))
            {
                return env.GetAt(_locals[expr], name._lexeme);
            }
            else
            {
                return Globals.Get(name);
            }
        }

        private bool IsTruthy(object value)
        {
            if (value == null)
            {
                return false;
            }

            if (value is bool)
            {
                return (bool)value;
            }

            return true;
        }

        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null)
            {
                return true;
            }

            if (a == null)
            {
                return false;
            }

            return a.Equals(b);
        }

        private string Stringify(object value)
        {
            if (value == null) 
            {
                return "nil";
            }

            if (value is double d)
            {
                var text = d.ToString();

                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }

                return text;
            }

            return value.ToString();
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(env));
            return null;
        }

        public void ExecuteBlock(List<Stmt> stmts, Environment env)
        {
            var previous = this.env;
            try
            {
                this.env = env;
                foreach (var stmt in stmts)
                {
                    Execute(stmt);
                }
            } finally
            {
                this.env = previous;
            }
        }

        public object VisitClassStmt(Stmt.Class stmt)
        {
            object superclass = null;
            if (stmt.superclass != null) {
                superclass = Evaluate(stmt.superclass);
                if (superclass is not LoxClass)
                {
                    throw new RuntimeException(stmt.superclass.name, "superclass must be class");
                }
            }

            env.Define(stmt.name._lexeme, null);

            if (stmt.superclass != null)
            {
                env = new Environment(env);
                env.Define("super", superclass);
            }

            var methods = new Dictionary<string, LoxFunction>();
            foreach (var method in stmt.methods)
            {
                LoxFunction func = new LoxFunction(method, env, method.name._lexeme.Equals(""));
                methods[method.name._lexeme] = func;
            }

            LoxClass _class = new LoxClass(stmt.name._lexeme, (LoxClass)superclass, methods);

            if (superclass != null)
            {
                env = env.enclosing;
            }

            env.Assign(stmt.name, _class);

            return null;
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            LoxFunction function = new LoxFunction(stmt, env, false);
            env.Define(stmt.name._lexeme, function);
            return null;
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            } else if (stmt.thenBranch != null)
            {
                Execute(stmt.thenBranch);
            }

            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            var value = (object?)null;
            if (stmt.value != null)
            {
                value = Evaluate(stmt.value);
            }

            throw new Return(value);
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }

            env.Define(stmt.name._lexeme, value);
            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            while(IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.body);
            }

            return null;
        }
    }
}
