using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class Return : RuntimeException
    {
        public object _value;

        public Return(object value) : base(null, null)
        {
            _value = value;
        }
    }
}
