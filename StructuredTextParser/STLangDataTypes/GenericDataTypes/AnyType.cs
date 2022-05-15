using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public class AnyType : GenericType
    {
        public AnyType()
            : base("ANY", uint.MaxValue)
        {
        }

        public override float ConversionCost(Expression expression)
        {
            return 0.0f;
        }
    }
}
