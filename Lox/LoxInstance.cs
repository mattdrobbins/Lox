using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class LoxInstance
    {
        private LoxClass _class;

        private Dictionary<string, object> _fields = new Dictionary<string, object>();
        
        public LoxInstance(LoxClass _class) 
        {
            this._class = _class;
        }

        public object Get(Token name)
        {
            if (_fields.ContainsKey(name._lexeme))
            {
                return _fields[name._lexeme];
            }

            var method = _class.FindMethod(name._lexeme);
            if (method != null) return method.Bind(this);

            throw new RuntimeException(name, "Undefined property " + name._lexeme);
        }

        public object Set(Token name, object value)
        {
            _fields[name._lexeme] = value;

            return null;
        }

        public override string ToString()
        {
            return _class._name + " instance";
        }
    }
}
