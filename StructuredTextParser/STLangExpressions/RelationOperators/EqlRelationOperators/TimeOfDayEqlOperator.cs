using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class TimeOfDayEqlOperator : RelationOperator
    {
        public TimeOfDayEqlOperator(Expression arg1, Expression arg2)
            : base(arg1, arg2, " = ", VirtualMachineInstruction.NOOP)
        {

        }

        public override Expression DeMorgan()
        {
            return new TimeOfDayNeqOperator(this.LeftOperand, this.RightOperand);
        }

        public override int Priority
        {
            get { return 4; }
        }
    }
}
