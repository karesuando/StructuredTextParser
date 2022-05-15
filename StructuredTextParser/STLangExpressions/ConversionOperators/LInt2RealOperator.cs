using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class LInt2RealOperator : ConversionOperator
    {
        public LInt2RealOperator(Expression expr, bool isImplicitOp = true)
            : base(expr, "LINT_TO_REAL", TypeNode.Real, VirtualMachineInstruction.L2F, isImplicitOp)
        {
        }
    }
}