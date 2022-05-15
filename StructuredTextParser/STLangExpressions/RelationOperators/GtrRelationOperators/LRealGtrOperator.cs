using System;
using System.Collections.Generic;
using System.Linq;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class LRealGtrOperator : RelationOperator
    {
        public LRealGtrOperator(Expression arg1, Expression arg2)
            : base(arg1, arg2,  " > ", VirtualMachineInstruction.DJGT)
        {

        }

        public LRealGtrOperator(Expression arg1, Expression arg2, bool isZeroOp)
            : base(arg1, arg2,  " > ", VirtualMachineInstruction.DJGTZ, true)
        {

        }

        public override Expression DeMorgan()
        {
            if (this.IsZeroOp)
                return new LRealLeqOperator(this.LeftOperand, this.RightOperand, true);
            else
                return new LRealLeqOperator(this.LeftOperand, this.RightOperand);
        }

        public override int Priority
        {
            get { return 5; }
        }
    }
}
