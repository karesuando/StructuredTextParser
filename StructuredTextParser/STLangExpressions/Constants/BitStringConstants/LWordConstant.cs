using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;

namespace STLang.Expressions
{
    public class LWordConstant : UInt64Constant
    {
        public LWordConstant(ulong value)
            : base(TypeNode.LWord, value, value.ToString())
        {
        }

        public LWordConstant(ulong value, string valueStr)
            : base(TypeNode.LWord, value, valueStr)
        {
        }

        public LWordConstant(ulong value, TypeNode dataType, string valueStr)
            : base(dataType, value, valueStr)
        {
        }
    }
}

