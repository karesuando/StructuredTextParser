using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public class AnyRealType : GenericType
    {
        public AnyRealType()
            : base("ANY_REAL", uint.MaxValue)
        {
        }

        public override float ConversionCost(Expression expression)
        {
            if (expression == null)
                return 0.0f;
            else if (expression.DataType == TypeNode.Real)
                return 0.0f;
            else if (expression.DataType == TypeNode.LReal)
                return 0.0f;
            else if (expression.IsConstant && expression.DataType.IsIntegerType)
                return 0.5f;
            else if (expression.DataType == Error)
                return 0.0f;
            else
                return MAX_CONVERSION_COST;
        }
    }
}
