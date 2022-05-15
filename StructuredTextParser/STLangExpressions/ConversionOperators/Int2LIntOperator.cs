using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class Int2LIntOperator : ConversionOperator
    {
        public Int2LIntOperator(Expression expr, bool isImplicitOp = true)
            : base(expr, "INT_TO_LINT", TypeNode.LInt, VirtualMachineInstruction.I2L, isImplicitOp)
        {
        }
    }
}
