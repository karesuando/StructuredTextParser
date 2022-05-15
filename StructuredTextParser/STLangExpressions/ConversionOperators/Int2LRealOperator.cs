using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class Int2LRealOperator : ConversionOperator
    {
        public Int2LRealOperator(Expression expr, bool isImplicitOp = true)
            : base(expr, "INT_TO_LREAL", TypeNode.LReal, VirtualMachineInstruction.I2D, isImplicitOp)
        {
        }
    }
}
