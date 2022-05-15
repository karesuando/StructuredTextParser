using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class FloatGeqOperator : RelationOperator
    {
        public FloatGeqOperator(Expression leftTree, Expression rightTree)
            : base(leftTree, rightTree, " >= ", VirtualMachineInstruction.FJGE)
        {

        }

        public FloatGeqOperator(Expression leftTree, Expression rightTree, bool isZeroOp)
            : base(leftTree, rightTree, " >= ", VirtualMachineInstruction.FJGEZ, true)
        {

        }

        public override Expression DeMorgan()
        {
            if (this.IsZeroOp)
                return new FloatLesOperator(this.LeftOperand, this.RightOperand, true);
            else
                return new FloatLesOperator(this.LeftOperand, this.RightOperand);
        }

        public override int Priority
        {
            get { return 5; }
        }
    }
}
