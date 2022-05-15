using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class FloatGtrOperator : RelationOperator
    {
        public FloatGtrOperator(Expression leftTree, Expression rightTree)
            : base(leftTree, rightTree,  " > ", VirtualMachineInstruction.FJGT)
        {

        }

        public FloatGtrOperator(Expression leftTree, Expression rightTree, bool isZeroOp)
            : base(leftTree, rightTree,  " > ", VirtualMachineInstruction.FJGTZ, true)
        {

        }

        public override Expression DeMorgan()
        {
            if (this.IsZeroOp)
                return new FloatLeqOperator(this.LeftOperand, this.RightOperand, true);
            else
                return new FloatLeqOperator(this.LeftOperand, this.RightOperand);
        }

        public override int Priority
        {
            get { return 5; }
        }
    }
}
