using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class LRealEqlOperator : RelationOperator
    {
        public LRealEqlOperator(Expression leftTree, Expression rightTree)
            : base(leftTree, rightTree, " = ", VirtualMachineInstruction.DJEQ)
        {

        }

        public LRealEqlOperator(Expression leftTree, Expression rightTree, bool zeroOp)
            : base(leftTree, rightTree, " = ", VirtualMachineInstruction.DJEQZ, true)
        {

        }

        public override Expression DeMorgan()
        {
            if (this.IsZeroOp)
                return new LRealNeqOperator(this.LeftOperand, this.RightOperand, true);
            else
                return new LRealNeqOperator(this.LeftOperand, this.RightOperand);
        }

        public override int Priority
        {
            get { return 4; }
        }
    }
}
