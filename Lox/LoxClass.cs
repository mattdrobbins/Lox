using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class LoxClass : ILoxCallable
    {
        public string _name;
        private Dictionary<string, LoxFunction> _methods;
        private LoxClass _superclass;

        public LoxClass(string name, LoxClass superclass, Dictionary<string, LoxFunction> methods)
        {
            _name = name;
            _methods = methods;
            _superclass = superclass;
        }

        public LoxFunction FindMethod(string name)
        {
            if (_methods.ContainsKey(name)) return _methods[name];

            if (_superclass != null) return _superclass.FindMethod(name);

            return null;
        }

        public int Arity()
        {
            LoxFunction initialiser = FindMethod("init");
            if (initialiser != null)
            {
                return initialiser.Arity();
            }    

            return 0;
        }

        public object Call(Interperater interperater, List<object> args)
        {
            LoxInstance instance = new LoxInstance(this);
            LoxFunction initialiser = FindMethod("init");

            if (initialiser != null)
            {
                initialiser.Bind(instance).Call(interperater, args);
            }

            return instance;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
