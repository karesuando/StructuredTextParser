using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class UIntMulOperator : BinaryOperator
    {
        public UIntMulOperator(Expression arg1, Expression arg2, TypeNode dataType)
            : base(arg1, arg2, dataType, "*", GetMulInstruction(dataType))
        {

        }

        private static VirtualMachineInstruction GetMulInstruction(TypeNode dataType)
        {
            if (dataType.Size < TypeNode.LInt.Size)
                return VirtualMachineInstruction.IMUL;
            else
                return VirtualMachineInstruction.LMUL;
        }

        public override int Priority
        {
            get { return 7; }
        }
    }
}
