using Lox;

namespace AstPrinterTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var expression = new Expr.Binary(
                new Expr.Unary(
                    new Token(TokenType.MINUS, "-", null, 1), 
                new Expr.Literal(123)),
                new Token(TokenType.STAR, "*", null, 1),
                new Expr.Grouping(new Expr.Literal(45.67))      
                );

            Console.WriteLine(new AstPrinter().Print(expression));
        }
    }
}
