using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;

namespace STLang.Subranges
{
    public class EnumSubrange : SubRange
    {
        public EnumSubrange(Expression lower, Expression upper, TypeNode dataType) 
            : base(lower, upper, dataType)
        {
            this.lowerBound = Convert.ToUInt16(lower.Evaluate());
            this.upperBound = Convert.ToUInt16(upper.Evaluate());
        }

        public EnumSubrange(ushort lower, ushort upper, TypeNode dataType)
            : base(lower, upper, dataType)
        {
            this.lowerBound = lower;
            this.upperBound = upper;
        }

        public override bool Contains(SubRange subRange)
        {
            if (!(subRange is EnumSubrange))
                return false;
            else if (!subRange.DataType.Equals(this.DataType))
                return false;
            else {
                EnumSubrange enumSubrange = (EnumSubrange)subRange;
                ushort lower = enumSubrange.LowerBound;
                ushort upper = enumSubrange.UpperBound;
                return lower >= this.lowerBound && upper <= this.upperBound;
            }
        }

        public override bool IsDisjoint(SubRange subRange)
        {
            if (!(subRange is EnumSubrange))
                return false;
            else if (! subRange.DataType.Equals(this.DataType))
                return false;
            else {
                EnumSubrange enumSubrange = (EnumSubrange)subRange;
                ushort lower = enumSubrange.LowerBound;
                ushort upper = enumSubrange.UpperBound;
                return upper < this.lowerBound || lower > this.upperBound;
            }
        }

        public override bool Contains(Expression value)
        {
            if (value == null)
                return false;
            else if (!value.IsConstant)
                return false;
            else if (!value.DataType.IsEnumeratedType)
                return false;
            else if (value.DataType == this.DataType)
            {
                ushort enumValue = Convert.ToUInt16(value.Evaluate());
                return enumValue >= this.lowerBound && enumValue <= this.upperBound;
            }
            else if (this.DataType.IsSubrangeType)
            {
                EnumeratedType enumType = (EnumeratedType)this.DataType;
                TypeNode baseType = enumType.BaseType;
                if (baseType == value.DataType)
                {
                    ushort enumValue = Convert.ToUInt16(value.Evaluate());
                    return enumValue >= this.lowerBound && enumValue <= this.upperBound;
                }
            }
            return false;
        }

        public override object CreateInterval()
        {
            ushort value = this.lowerBound;
            int range = (int)(this.upperBound - this.lowerBound + 1);
            List<ushort> interval = new List<ushort>();
            for (int i = range; i > 0; i--)
                interval.Add(value++);
            return interval.ToArray();
        }

        public override bool InRange(ulong lower, ulong upper)
        {
            return lower >= this.lowerBound && upper <= this.upperBound;
        }

        public ushort LowerBound
        {
            get { return this.lowerBound; }
        }

        public ushort UpperBound
        {
            get { return this.upperBound; }
        }

        private readonly ushort lowerBound;

        private readonly ushort upperBound;
    }
}
