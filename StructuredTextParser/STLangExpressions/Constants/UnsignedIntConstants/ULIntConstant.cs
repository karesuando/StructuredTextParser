using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class ULIntConstant : UInt64Constant
    {
        public ULIntConstant(ulong value)
            : base(TypeNode.ULInt, value, value.ToString())
        {
        }

        public ULIntConstant(ulong value, TypeNode dataType, string valueStr)
            : base(dataType, value, value.ToString())
        {
        }

        public ULIntConstant(ulong value, string valueStr)
            : base(TypeNode.ULInt, value, valueStr)
        {
        }
    }
}
