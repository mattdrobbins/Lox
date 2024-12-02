using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class Environment
    {
        public Environment enclosing;

        public Environment()
        {           
        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        private Dictionary<string, object> values = new Dictionary<string, object>();
    
        public void Define(string name, object value)
        {
            values.Add(name, value);
        }

        public object GetAt(int distance, string name)
        {
            return Ancestor(distance).values[name];
        }

        Environment Ancestor(int distance)
        {
            Environment environment = this;
            for (int i = 0; i < distance; i++)
            {
                environment = environment.enclosing;
            }

            return environment;
        }

        public object Get(Token name)
        {
            if (values.ContainsKey(name._lexeme))
            {
                return values[name._lexeme];
            }

            if (this.enclosing != null)
            {
                return this.enclosing.Get(name);
            }

            throw new RuntimeException(name, $"Undefined variable {name._lexeme}");
        }
        public void Assign(Token name, object value)
        {
            if (values.ContainsKey(name._lexeme))
            {
                values[name._lexeme] = value;
                return;
            }

            if (this.enclosing != null)
            {
                this.enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeException(name, $"undefined variable {name._lexeme}");
        }

        public void AssignAt(int distance, Token name, Object value)
        {
            Ancestor(distance).values[name._lexeme] = value;
        }


    }
}
