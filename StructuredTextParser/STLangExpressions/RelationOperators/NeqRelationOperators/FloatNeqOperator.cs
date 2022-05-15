using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class FloatNeqOperator : RelationOperator
    {
        public FloatNeqOperator(Expression arg1, Expression arg2)
            : base(arg1, arg2, " <> ", VirtualMachineInstruction.FJNE)
        {

        }

        public FloatNeqOperator(Expression arg1, Expression arg2, bool isZeroOp)
            : base(arg1, arg2, " <> ", VirtualMachineInstruction.FJNEZ, true)
        {

        }

        public override Expression DeMorgan()
        {
            if (this.IsZeroOp)
                return new FloatEqlOperator(this.LeftOperand, this.RightOperand, true);
            else
                return new FloatEqlOperator(this.LeftOperand, this.RightOperand);
        }

        public override int Priority
        {
            get { return 4; }
        }
    }
}
