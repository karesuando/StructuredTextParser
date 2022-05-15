using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public class LRealType : ElementaryType<double>
    {
        public LRealType()
            : base("LREAL", double.MinValue, double.MaxValue, LREAL_SIZE, "R")
        {
            this.initialValue = new LRealConstant(0.0d, "LREAL#0.0", this);
        }

        public override float ConversionCost(Expression expression)
        {
            if (expression == null)
                return 0.0f;
            else if (expression.DataType == this)
                return 0.0f;
            else if (expression.IsConstant && expression.DataType == Real)
                return 0.333f;
            else if (expression.IsConstant && expression.DataType.IsIntegerType)
                return 1.0f;
            else if (expression.DataType == Error)
                return 0.0f;
            else
                return MAX_CONVERSION_COST;
        }

        public override bool IsNumericalType 
        { 
            get { return true; } 
        }
    }
}
