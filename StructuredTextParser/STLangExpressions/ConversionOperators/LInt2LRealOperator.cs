using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class LInt2LRealOperator : ConversionOperator
    {
        public LInt2LRealOperator(Expression expr, bool isImplicitOp = true)
            : base(expr, "LINT_TO_LREAL", TypeNode.LReal, VirtualMachineInstruction.L2D, isImplicitOp)
        {
        }
    }
}