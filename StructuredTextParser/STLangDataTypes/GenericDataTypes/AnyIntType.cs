using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public class AnyIntType : AnyNumType
    {
        public AnyIntType()
            : base("ANY_INT", uint.MaxValue)
        {
        }

        public override float ConversionCost(Expression expression)
        {
            if (expression == null)
                return 0.0f;
            else if (expression.DataType.IsIntegerType)
                return 0.0f;
            else if (expression.DataType == Error)
                return 0.0f;
            else
                return MAX_CONVERSION_COST;
        }
    }
}
