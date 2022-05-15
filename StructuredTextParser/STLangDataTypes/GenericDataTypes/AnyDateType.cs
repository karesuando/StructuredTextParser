using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public class AnyDateType : AnyElementaryType
    {
        public AnyDateType(string name = "ANY_DATE", uint size = DATE_SIZE)
            : base(name, size)
        {
        }

        public override float ConversionCost(Expression expression)
        {
            if (expression == null)
                return 0.0f;
            else if (expression.DataType.IsDateClass)
                return 0.0f;
            else if (expression.DataType == Error)
                return 0.0f;
            else
                return MAX_CONVERSION_COST;
        }
    }
}
