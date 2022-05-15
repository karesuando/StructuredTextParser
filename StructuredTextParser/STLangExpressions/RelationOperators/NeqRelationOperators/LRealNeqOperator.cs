using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class LRealNeqOperator : RelationOperator
    {
        public LRealNeqOperator(Expression arg1, Expression arg2)
            : base(arg1, arg2, " <> ", VirtualMachineInstruction.DJNE)
        {

        }

        public LRealNeqOperator(Expression arg1, Expression arg2, bool isZeroOp)
            : base(arg1, arg2, " <> ", VirtualMachineInstruction.DJNEZ, true)
        {

        }

        public override Expression DeMorgan()
        {
            if (this.IsZeroOp)
                return new LRealEqlOperator(this.LeftOperand, this.RightOperand, true);
            else
                return new LRealEqlOperator(this.LeftOperand, this.RightOperand);
        }

        public override int Priority
        {
            get { return 4; }
        }
    }
}
