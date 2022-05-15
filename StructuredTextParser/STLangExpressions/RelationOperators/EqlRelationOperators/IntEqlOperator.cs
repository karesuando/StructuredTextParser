using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class IntEqlOperator : RelationOperator
    {
        public IntEqlOperator(Expression leftTree, Expression rightTree)
            : base(leftTree, rightTree, " = ", VirtualMachineInstruction.IJEQ)
        {
        }

        public IntEqlOperator(Expression leftTree, Expression rightTree, bool isZeroOp)
            : base(leftTree, rightTree, " = ", VirtualMachineInstruction.IJEQZ, true)
        {
        }

        public override Expression DeMorgan()
        {
            if (this.IsZeroOp)
                return new IntNeqOperator(this.LeftOperand, this.RightOperand, true);
            else
                return new IntNeqOperator(this.LeftOperand, this.RightOperand);
        }

        private static VirtualMachineInstruction GetEqlInstruction(TypeNode dataType)
        {
            if (dataType.Size < TypeNode.LInt.Size)
                return VirtualMachineInstruction.IJEQ;
            else
                return VirtualMachineInstruction.LJEQ;
        }

        public override int Priority
        {
            get { return 4; }
        }
    }
}