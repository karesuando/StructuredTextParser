using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class UInt2LRealOperator : ConversionOperator
    {
        public UInt2LRealOperator(Expression expr, bool isImplicitOp = true)
            : base(expr, "UINT_TO_LREAL", TypeNode.LReal, VirtualMachineInstruction.I2D, isImplicitOp)
        {
        }
    }
}
