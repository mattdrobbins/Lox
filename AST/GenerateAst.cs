using System;
using System.IO;
using System.Collections.Generic;

namespace CraftingInterpreters.Tool
{
    public class Program
    {                    
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Usage: generate_ast <output directory>");
                Environment.Exit(64);
            }
            string outputDir = args[0];
            
            DefineAst(outputDir, "Expr", new List<string>
            {
                "Assign   : Token name, Expr value",
                "Binary   : Expr left, Token _operator, Expr right",
                "Call     : Expr callee, Token paren, List<Expr> arguments",
                "Get      : Expr obj, Token name",
                "Grouping : Expr expression",
                "Literal  : obj value",
                "Logical  : Expr left, Token _operator, Expr right",
                "Set      : Expr obj, Token name, Expr value",
                "Super    : Token keyword, Token method",
                "This     : Token keyword",
                "Unary    : Token _operator, Expr right",
                "Variable : Token name"
            });

            DefineAst(outputDir, "Stmt", new List<string>
            {
                "Block      : List<Stmt> statements",
                "Class      : Token name, Expr.Variable superclass, List<Stmt.Function> methods",
                "Expression : Expr expression",
                "Function   : Token name, List<Token> params, List<Stmt> body",
                "If         : Expr condition, Stmt thenBranch, Stmt elseBranch",
                "Print      : Expr expression",
                "Return     : Token keyword, Expr value",
                "Var        : Token name, Expr initializer",
                "While      : Expr condition, Stmt body"
            });
        }
        
        private static void DefineAst(string outputDir, string baseName, List<string> types)
        {
            string path = Path.Combine(outputDir, baseName + ".cs");
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("namespace CraftingInterpreters.Lox");
                writer.WriteLine("{");
                writer.WriteLine("    using System.Collections.Generic;");
                writer.WriteLine();
                writer.WriteLine($"    public abstract class {baseName}");
                writer.WriteLine("    {");

                DefineVisitor(writer, baseName, types);

                // The AST classes.
                foreach (string type in types)
                {
                    string className = type.Split(':')[0].Trim();
                    string fields = type.Split(':')[1].Trim();
                    DefineType(writer, baseName, className, fields);
                }

                // The base accept() method.
                writer.WriteLine();
                writer.WriteLine("        public abstract R Accept<R>(Visitor<R> visitor);");

                writer.WriteLine("    }");
                writer.WriteLine("}");
            }
        }

        private static void DefineVisitor(StreamWriter writer, string baseName, List<string> types)
        {
            writer.WriteLine("        public interface Visitor<R>");
            writer.WriteLine("        {");

            foreach (string type in types)
            {
                string typeName = type.Split(':')[0].Trim();
                writer.WriteLine($"            R Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
            }

            writer.WriteLine("        }");
        }

        private static void DefineType(StreamWriter writer, string baseName, string className, string fieldList)
        {
            writer.WriteLine($"        public class {className} : {baseName}");
            writer.WriteLine("        {");

            // Constructor.
            writer.WriteLine($"            public {className}({fieldList})");
            writer.WriteLine("            {");

            // Store parameters in fields.
            string[] fields = fieldList.Split(", ");
            foreach (string field in fields)
            {
                string name = field.Split(' ')[1];
                writer.WriteLine($"                this.{name} = {name};");
            }

            writer.WriteLine("            }");

            // Visitor pattern.
            writer.WriteLine();
            writer.WriteLine("            public override R Accept<R>(Visitor<R> visitor)");
            writer.WriteLine("            {");
            writer.WriteLine($"                return visitor.Visit{className}{baseName}(this);");
            writer.WriteLine("            }");

            // Fields.
            writer.WriteLine();
            foreach (string field in fields)
            {
                writer.WriteLine($"            public readonly {field};");
            }

            writer.WriteLine("        }");
        }
    }

    public interface PastryVisitor
    {
        void VisitBeignet(Beignet beignet);
        void VisitCruller(Cruller cruller);
    }

    public abstract class Pastry
    {
        public abstract void Accept(PastryVisitor visitor);
    }

    public class Beignet : Pastry
    {
        public override void Accept(PastryVisitor visitor)
        {
            visitor.VisitBeignet(this);
        }
    }

    public class Cruller : Pastry
    {
        public override void Accept(PastryVisitor visitor)
        {
            visitor.VisitCruller(this);
        }
    }
}

