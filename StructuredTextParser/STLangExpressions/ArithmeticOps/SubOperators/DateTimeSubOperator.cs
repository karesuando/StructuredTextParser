using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class DateTimeSubOperator : DateTimeOperator
    {
        public DateTimeSubOperator(Expression arg1, Expression arg2)
            : base(arg1, arg2, TypeNode.Time, " - ", StandardLibraryFunction.DTSUB)
        {

        }

        public override int Priority
        {
            get { return 6; }
        }

    }
}
