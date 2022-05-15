using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class TimeSubOperator : DateTimeOperator
    {
        public TimeSubOperator(Expression arg1, Expression arg2)
            : base(arg1, arg2, TypeNode.Time, " - ", StandardLibraryFunction.TSUB)
        {

        }

        public override int Priority
        {
            get { return 6; }
        }
    }
}
