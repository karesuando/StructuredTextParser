using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public class RealType : ElementaryType<float>
    {
        public RealType(string name = "REAL")
            : base(name, float.MinValue, float.MaxValue, REAL_SIZE, "F")
        {
            this.initialValue = new RealConstant(0.0f, "REAL#0.0", this);
        }

        public override bool IsInRange(SubRange subRange)
        {
            return true;
        }

        public override float ConversionCost(Expression expression)
        {
            if (expression == null)
                return 0.0f;
            else if (expression.DataType == this)
                return 0.0f;
            else if (expression.IsConstant && expression.DataType == LReal)
                return 0.5f;
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
