using STLang.Expressions;
using STLang.Subranges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STLang.DataTypes
{
    public class WCharType : AnyCharacter<char>
    {
        public WCharType(string name = "WCHAR") : base(name, sizeof(char), char.MinValue, char.MaxValue, "W")
        {
            this.initialValue = new WCharConstant(char.MinValue);
        }

        public override bool IsInRange(long value)
        {
            return value >= this.LowerBound && value <= this.UpperBound;
        }

        public override bool IsInRange(SubRange subRange)
        {
            return subRange.InRange(this.LowerBound, this.UpperBound);
        }

        public override SubRange GetSubrange()
        {
            return new IntSubrange(this.LowerBound, this.UpperBound, this);
        }

        public override float ConversionCost(Expression expression)
        {
            if (expression == null)
                return 0.0f;
            else if (expression.DataType == Error)
                return 0.0f;
            else if (expression.DataType == this)
                return 0.0f;
            else if (expression.IsConstant && expression.DataType.IsIntegerType)
                return expression.DataType.Size / (float)this.Size;
            else if (!expression.DataType.IsIntegerType)
                return MAX_CONVERSION_COST;
            else if (this.Size >= expression.DataType.Size)
                return 0.333f;   // promotion
            else
                return 0.333f * (expression.DataType.Size / (float)this.Size);
        }
    }
}
