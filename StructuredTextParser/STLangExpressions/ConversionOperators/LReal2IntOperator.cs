using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class LReal2IntOperator : ConversionOperator
    {
        public LReal2IntOperator(Expression expr, bool isImplicitOp = true)
            : base(expr, "LREAL_TO_INT", TypeNode.Int, VirtualMachineInstruction.D2I, isImplicitOp)
        {
        }
    }
}
