using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class LeftShiftOperator : UnaryOperator
    {
        public LeftShiftOperator(Expression expression, int bits)
            : base(expression, expression.ToString(), GetSHLInstruction(expression.DataType))
        {
            this.bits = bits;
            this.operand = expression;
        }

        public override void GenerateLoad(List<int> trueBranch = null, List<int> falseBranch = null)
        {
            this.operand.GenerateLoad();
            this.StoreInstruction(this.OperationCode, this.bits);
        }

        private static VirtualMachineInstruction GetSHLInstruction(TypeNode dataType)
        {
            if (dataType.Size < TypeNode.LWord.Size)
                return VirtualMachineInstruction.IBSHL;
            else
                return VirtualMachineInstruction.LBSHL;
        }

        private readonly int bits;

        private readonly Expression operand;
    }
}
