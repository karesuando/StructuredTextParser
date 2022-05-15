using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class Int2RealOperator : ConversionOperator
    {
        public Int2RealOperator(Expression expr, bool isImplicitOp = true)
            : base(expr, "INT_TO_REAL", TypeNode.Real, VirtualMachineInstruction.I2F, isImplicitOp)
        {
        }
    }
}
