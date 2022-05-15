using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class IntNeqOperator : RelationOperator
    {
        public IntNeqOperator(Expression arg1, Expression arg2)
            : base(arg1, arg2, " <> ", VirtualMachineInstruction.IJNE)
        {

        }

        public IntNeqOperator(Expression arg1, Expression arg2, bool isZeroOp)
            : base(arg1, arg2, " <> ", VirtualMachineInstruction.IJNEZ, true)
        {

        }

        public override Expression DeMorgan()
        {
            if (this.IsZeroOp)
                return new IntEqlOperator(this.LeftOperand, this.RightOperand, true);
            else
                return new IntEqlOperator(this.LeftOperand, this.RightOperand);
        }

        public override int Priority
        {
            get { return 4; }
        }
    }
}
