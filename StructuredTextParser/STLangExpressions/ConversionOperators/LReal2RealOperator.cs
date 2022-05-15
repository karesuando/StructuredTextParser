using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class LReal2RealOperator : ConversionOperator
    {
        public LReal2RealOperator(Expression expr, bool isImplicitOp = true)
            : base(expr, "LREAL_TO_REAL", TypeNode.Real, VirtualMachineInstruction.D2F, isImplicitOp)
        {
        }
    }
}
