using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox.Callables
{
    public class ClockCallable : ILoxCallable
    {
        public int Arity() => 0;

        public object Call(Interperater interperater, List<object> args)
        {
            return (double)DateTime.Now.Ticks / 10e7;
        }

        public override string ToString()
        {
            return "<native fn>";
        }
    }
}
