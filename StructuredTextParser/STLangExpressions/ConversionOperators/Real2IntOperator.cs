using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class Real2IntOperator : ConversionOperator
    {
        public Real2IntOperator(Expression expr, bool isImplicitOp = true)
            : base(expr, "REAL_TO_INT", TypeNode.DInt, VirtualMachineInstruction.F2I, isImplicitOp)
        {
        }
    }
}
