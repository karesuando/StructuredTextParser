using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class FloatEqlOperator : RelationOperator
    {
        public FloatEqlOperator(Expression leftTree, Expression rightTree)
            : base(leftTree, rightTree, " = ", VirtualMachineInstruction.FJEQ)
        {

        }

        public FloatEqlOperator(Expression leftTree, Expression rightTree, bool zeroOp)
            : base(leftTree, rightTree, " = ", VirtualMachineInstruction.FJEQZ, true)
        {

        }

        public override Expression DeMorgan()
        {
            if (this.IsZeroOp)
                return new FloatNeqOperator(this.LeftOperand, this.RightOperand, true);
            else
                return new FloatNeqOperator(this.LeftOperand, this.RightOperand);
        }

        public override int Priority
        {
            get { return 4; }
        }
    }
}
