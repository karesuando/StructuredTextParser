using STLang.Subranges;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public class CharType : AnyCharacter<byte>
    {
        public CharType(string name = "CHAR") : base(name, sizeof(byte), byte.MinValue, byte.MaxValue, "C")
        {
            this.initialValue = new CharConstant(byte.MinValue);
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
            else if (expression.IsConstant && expression.DataType.IsSignedIntType)
                return 1.0f * (expression.DataType.Size / (float)this.Size);
            else if (!(expression.DataType.IsUnsignedIntType || expression.DataType.IsBitStringType))
                return MAX_CONVERSION_COST;
            else 
                return 0.333f * (expression.DataType.Size / (float)this.Size);
        }

        public override uint Alignment
        {
            get { return this.Size; }
        }
    }
}
