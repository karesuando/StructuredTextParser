using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class DIntConstant : Int32Constant
    {
        public DIntConstant(int value)
            : base(TypeNode.DInt, value, value.ToString())
        {
        }

        public DIntConstant(int value, string valueStr)
            : base(TypeNode.DInt, value, valueStr)
        {
        }

        public DIntConstant(int value, string valueStr, TypeNode dataType)
            : base(dataType, value, valueStr)
        {
        }
    }
}
