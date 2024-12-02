using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class LoxFunction : ILoxCallable
    {
        private readonly Stmt.Function decleration;
        private readonly Environment closure;
        private bool isInitialiser;
        public LoxFunction(Stmt.Function decleration, Environment closure, bool isInitialiser)
        {
            this.decleration = decleration;
            this.closure = closure;
            this.isInitialiser = isInitialiser;
        }

        public LoxFunction Bind(LoxInstance instance)
        {
            var environment = new Environment(closure);
            environment.Define("this", instance);
            return new LoxFunction(decleration, environment, isInitialiser);
        }

        public int Arity()
        {
            return decleration._params.Count;
        }

        public object Call(Interperater interperater, List<object> args)
        {
            var environment = new Environment(this.closure);

            for (int i = 0; i < decleration._params.Count; i++)
            {
                environment.Define(decleration._params[i]._lexeme, args[i]);          
            }

            try
            {
                interperater.ExecuteBlock(this.decleration.body, environment);

            }
            catch (Return returnValue)
            {
                if (isInitialiser) return closure.GetAt(0, "this");
                return returnValue._value;
            }

            if (isInitialiser) return closure.GetAt(0, "this");
            return null;
        }

        public override string ToString()
        {
            return $"<fn {decleration.name._lexeme} >";
        }
    }
}
