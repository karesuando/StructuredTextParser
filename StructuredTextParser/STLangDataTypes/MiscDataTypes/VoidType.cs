using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public class VoidType : TypeNode
    {
        public VoidType() : base("VOID", 0, "C")
        {

        }
    }
}
