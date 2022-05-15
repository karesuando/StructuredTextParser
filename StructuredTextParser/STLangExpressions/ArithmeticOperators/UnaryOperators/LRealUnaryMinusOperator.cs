using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class LRealUnaryMinusOperator : UnaryOperator
    {
        public LRealUnaryMinusOperator(Expression tree)
            : base(tree, "-", TypeNode.LReal, VirtualMachineInstruction.DNEG)
        {
        }
    }
}
