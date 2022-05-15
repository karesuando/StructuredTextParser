using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class IntGtrOperator : RelationOperator
    {
        public IntGtrOperator(Expression arg1, Expression arg2)
            : base(arg1, arg2,  " > ", VirtualMachineInstruction.IJGT)
        {

        }

        public IntGtrOperator(Expression arg1, Expression arg2, bool isZeroOp)
            : base(arg1, arg2,  " > ", VirtualMachineInstruction.IJGTZ, true)
        {

        }

        public override Expression DeMorgan()
        {
            if (this.IsZeroOp)
                return new IntLeqOperator(this.LeftOperand, this.RightOperand, true);
            else
                return new IntLeqOperator(this.LeftOperand, this.RightOperand);
        }

        public override int Priority
        {
            get { return 5; }
        }
    }
}
