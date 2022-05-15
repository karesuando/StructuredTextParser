using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class UDIntConstant : UInt32Constant
    {
        public UDIntConstant(uint value)
            : base(TypeNode.UDInt, value, value.ToString())
        {
        }

        public UDIntConstant(uint value, string valueStr)
            : base(TypeNode.UDInt, value, valueStr)
        {
        }

        public UDIntConstant(uint value, TypeNode dataType, string valueStr)
            : base(dataType, value, valueStr)
        {
        }
    }
}
