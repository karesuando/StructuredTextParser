﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class DateTimeLesOperator : RelationOperator
    {
        public DateTimeLesOperator(Expression arg1, Expression arg2)
            : base(arg1, arg2, " < ", VirtualMachineInstruction.NOOP)
        {

        }

        public override Expression DeMorgan()
        {
            return new DateTimeGeqOperator(this.LeftOperand, this.RightOperand);
        }

        public override int Priority
        {
            get { return 5; }
        }
    }
}
