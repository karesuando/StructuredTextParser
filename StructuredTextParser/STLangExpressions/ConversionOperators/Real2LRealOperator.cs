using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class Real2LRealOperator : ConversionOperator
    {
        public Real2LRealOperator(Expression expr, bool isImplicitOp = true)
            : base(expr, "REAL_TO_LREAL", TypeNode.LReal, VirtualMachineInstruction.F2D, isImplicitOp)
        {
        }
    }
}
