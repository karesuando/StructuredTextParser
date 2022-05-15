using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public class AnyBitType : AnyElementaryType
    {
        public AnyBitType(string name = "ANY_BIT", uint size = uint.MaxValue)
            : base(name, size)
        {
        }

        public override float ConversionCost(Expression expression)
        {
            if (expression == null)
                return 0.0f;
            else if (expression.DataType.IsBitStringType)
                return 0.0f;
            else if (expression.IsConstant && expression.DataType.IsIntegerType)
                return 0.333f;
            else if (expression.DataType == Error)
                return 0.0f;
            else
                return MAX_CONVERSION_COST;
        }
    }
}
