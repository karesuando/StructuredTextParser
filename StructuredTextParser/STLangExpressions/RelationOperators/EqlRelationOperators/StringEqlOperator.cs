using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class StringEqlOperator : StringRelationOperator
    {
        public StringEqlOperator(Expression arg1, Expression arg2)
            : base(arg1, arg2, " = ", VirtualMachineInstruction.IJEQZ)
        {

        }

        public override Expression DeMorgan()
        {
            return new StringNeqOperator(this.LeftOperand, this.RightOperand);
        }

        public override int Priority
        {
            get { return 4; }
        }
    }
}
