using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;

namespace STLang.Expressions
{
    public class DWordConstant : UInt32Constant
    {
        public DWordConstant(uint value)
            : base(TypeNode.DWord, value, value.ToString())
        {
        }

        public DWordConstant(uint value, string valueStr)
            : base(TypeNode.DWord, value, valueStr)
        {
        }

        public DWordConstant(uint value, TypeNode dataType, string valueStr)
            : base(dataType, value, valueStr)
        {
        }
    }
}
