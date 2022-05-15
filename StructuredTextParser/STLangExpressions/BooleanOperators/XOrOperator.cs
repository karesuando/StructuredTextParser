using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class XOrOperator : BinaryOperator
    {
        public XOrOperator(Expression arg1, Expression arg2)
            : base(arg1, arg2, TypeNode.Bool, " XOR ", VirtualMachineInstruction.IBXOR)
        {

        }

        public override void GenerateLoad(List<int> trueBranch = null, List<int> falseBranch = null)
        {
            throw new NotImplementedException();
        }

        private static VirtualMachineInstruction GetEqlInstruction(TypeNode dataType)
        {
            if (dataType.Size < TypeNode.LInt.Size)
                return VirtualMachineInstruction.IBXOR;
            else
                return VirtualMachineInstruction.LBXOR;
        }

        public override int Priority
        {
            get { return 2; }
        }
    }
}
