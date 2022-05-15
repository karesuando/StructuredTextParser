using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class BitIOrOperator : BinaryOperator
    {
        public BitIOrOperator(Expression arg1, Expression arg2, TypeNode dataType)
            : base(arg1, arg2, dataType, " OR ", GetIOrInstruction(dataType))
        {

        }

        private static VirtualMachineInstruction GetIOrInstruction(TypeNode dataType)
        {
            if (dataType.Size < TypeNode.LInt.Size)
                return VirtualMachineInstruction.IBIOR;
            else
                return VirtualMachineInstruction.LBIOR;
        }
    }
}
