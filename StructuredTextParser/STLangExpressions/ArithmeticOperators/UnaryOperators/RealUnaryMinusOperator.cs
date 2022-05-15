using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class RealUnaryMinusOperator : UnaryOperator
    {
        public RealUnaryMinusOperator(Expression tree)
            : base(tree,  "-", VirtualMachineInstruction.FNEG)
        {
        }
    }
}
