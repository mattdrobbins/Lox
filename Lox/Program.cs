namespace Lox
{
    internal class Program
    {
        private static Interperater Interperater = new Interperater();

        static bool _hadError = false;
        static bool _hadRuntimeError = false;

        static void Main(string[] args)
        {
            RunFile("Program.txt");

/*            if (args.Length > 1)
            {
                Console.WriteLine("Usage Lox [script]");
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }*/
        }

        private static void RunFile(string filename)
        {
            byte[] bytes = File.ReadAllBytes(filename);
            var text = System.Text.Encoding.UTF8.GetString(bytes);
            Run(text);

            if (_hadError)
            {
                System.Environment.Exit(65);
            }
            if (_hadRuntimeError)
            {
                System.Environment.Exit(70);
            }
        }

        private static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line == "\u0004")
                {
                    break;
                }
                Run(line);
            }
        }

        private static void Run(string text)
        {
            _hadError = false;
            _hadRuntimeError = false;
            var scanner = new Scanner(text);
            var tokens = scanner.ScanTokens();

            Parser parser = new Parser(tokens);
            List<Stmt> stmts = parser.Parse();

            if (_hadError)
            {
                return;
            }

            Resolver resolver = new Resolver(Interperater);
            resolver.Resolve(stmts);

            if (_hadError) return;

            Interperater.Interperate(stmts);

            //Console.WriteLine(new AstPrinter().Print(expr));

/*            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }*/
        }

        public static void RuntimeError(RuntimeException runtimeException)
        {
            Console.WriteLine($"{runtimeException.Message} \n " +
                $"[{runtimeException.Token._line} \" ]");
            _hadRuntimeError = true;
        }

        public static void Error(int line, string message)
        {
            Report(line, " ", message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Error {where} : {message}");
            _hadError = true;
        }

        public static void Error(Token token, string message)
        {
            if (token._type == TokenType.EOF)
            {
                Report(token._line, " at end", message);
            }
            else
            {
                Report(token._line, $" at '{token._lexeme}'", message);
            }
        }
    }
}
