using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class UIntAddOperator : BinaryOperator
    {
        public UIntAddOperator(Expression arg1, Expression arg2, TypeNode dataType)
            : base(arg1, arg2, dataType, " + ", GetAddInstruction(dataType))
        {

        }

        private static VirtualMachineInstruction GetAddInstruction(TypeNode dataType)
        {
            if (dataType.Size < TypeNode.ULInt.Size)
                return VirtualMachineInstruction.IADD;
            else
                return VirtualMachineInstruction.LADD;
        }

        public override int Priority
        {
            get { return 6; }
        }
    }
}
