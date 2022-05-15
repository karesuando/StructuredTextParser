using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public abstract class GenericType : TypeNode
    {
        public GenericType(string name, uint size) : base(name, size)
        {

        }
    }
}
