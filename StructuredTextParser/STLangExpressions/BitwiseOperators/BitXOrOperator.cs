using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class BitXOrOperator : BinaryOperator
    {
        public BitXOrOperator(Expression arg1, Expression arg2, TypeNode dataType)
            : base(arg1, arg2, dataType, " XOR ", GetXOrInstruction(dataType))
        {

        }

        private static VirtualMachineInstruction GetXOrInstruction(TypeNode dataType)
        {
            if (dataType.Size < TypeNode.LInt.Size)
                return VirtualMachineInstruction.IBXOR;
            else
                return VirtualMachineInstruction.LBXOR;
        }
    }
}
