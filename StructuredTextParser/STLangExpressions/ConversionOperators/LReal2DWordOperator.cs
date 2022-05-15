using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class LReal2DWordOperator : ConversionOperator
    {
        public LReal2DWordOperator(Expression expr, bool isImplicitOp = true)
            : base(expr, "LREAL_TO_DWORD", TypeNode.DWord, VirtualMachineInstruction.D2I, isImplicitOp)
        {
        }
    }
}
