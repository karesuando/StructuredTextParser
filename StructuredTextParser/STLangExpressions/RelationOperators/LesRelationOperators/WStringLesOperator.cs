using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class WStringLesOperator : WStringRelationOperator
    {
        public WStringLesOperator(Expression arg1, Expression arg2)
            : base(arg1, arg2, " < ", VirtualMachineInstruction.IJLTZ)
        {

        }

        public override Expression DeMorgan()
        {
            return new WStringGeqOperator(this.LeftOperand, this.RightOperand);
        }

        public override int Priority
        {
            get { return 5; }
        }
    }
}

