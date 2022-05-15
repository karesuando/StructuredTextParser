using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class UIntSubOperator : BinaryOperator
    {
        public UIntSubOperator(Expression arg1, Expression arg2, TypeNode dataType)
            : base(arg1, arg2, dataType, " - ", GetSubInstruction(dataType))
        {

        }

        private static VirtualMachineInstruction GetSubInstruction(TypeNode dataType)
        {
            if (dataType.Size < TypeNode.ULInt.Size)
                return VirtualMachineInstruction.ISUB;
            else
                return VirtualMachineInstruction.LSUB;
        }

        public override int Priority
        {
            get { return 6; }
        }
    }
}
