using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class LIntConstant : Int64Constant
    {
        public LIntConstant(long value)
            : base(TypeNode.LInt, value, value.ToString())
        {
          
        }

        public LIntConstant(long value, TypeNode dataType, string valueStr)
            : base(dataType, value, value.ToString())
        {
        }

        public LIntConstant(long value, string valueStr)
            : base(TypeNode.LInt, value, valueStr)
        {
        }
    }
}
