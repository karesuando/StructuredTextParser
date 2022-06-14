using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;

namespace STLang.Subranges
{
    public class IntSubrange : SubRange
    {
        public IntSubrange(Expression lower, Expression upper, TypeNode dataType) 
            : base(lower, upper, dataType)
        {
            this.lowerBound = Convert.ToInt64(lower.Evaluate());
            this.upperBound = Convert.ToInt64(upper.Evaluate());
        }

        public IntSubrange(long lower, long upper, TypeNode dataType)
            : base(lower, upper, dataType)
        {
            this.lowerBound = lower;
            this.upperBound = upper;
        }

        public override bool Contains(SubRange subRange)
        {
            if (subRange is IntSubrange)
            {
                IntSubrange intSubRange = (IntSubrange)subRange;
                long lower = intSubRange.LowerBound;
                long upper = intSubRange.UpperBound;
                return lower >= this.lowerBound && upper <= this.upperBound;
            }
            else if (subRange is UIntSubrange)
            {
                UIntSubrange uintSubRange = (UIntSubrange)subRange;
                ulong lower = uintSubRange.LowerBound;
                ulong upper = uintSubRange.UpperBound;
                if (this.lowerBound < 0)
                    return false;
                else
                    return lower >= (ulong)this.lowerBound && upper <= (ulong)this.upperBound;
            }
            return false;
        }

        public override bool IsDisjoint(SubRange subRange)
        {
            if (subRange is IntSubrange)
            {
                IntSubrange intSubRange = (IntSubrange)subRange;
                long lower = intSubRange.LowerBound;
                long upper = intSubRange.UpperBound;
                return upper < this.lowerBound || lower > this.upperBound;
            }
            else
            {
                UIntSubrange uintSubRange = (UIntSubrange)subRange;
                ulong lower = uintSubRange.LowerBound;
                ulong upper = uintSubRange.UpperBound;
                return this.lowerBound < 0 && this.upperBound < 0
                    || (upper < (ulong)this.lowerBound || lower > (ulong)this.upperBound);
                  
            }
        }

        public override bool Contains(Expression value)
        {
            if (value == null)
                return false;
            else if (!value.DataType.IsSignedIntType)
                return false;
            else
            {
                long uintValue = Convert.ToInt64(value.Evaluate());
                return uintValue >= this.lowerBound && uintValue <= this.upperBound;
            }
        }

        public override object CreateInterval()
        {
            if (this.DataType == TypeNode.SInt)
            {
                sbyte value = (sbyte)this.lowerBound;
                int range = (int)(this.upperBound - this.lowerBound + 1);
                List<sbyte> interval = new List<sbyte>();
                for (int i = range; i > 0; i--)
                    interval.Add(value++);
                return interval.ToArray();
            }
            else if (this.DataType == TypeNode.Int)
            {
                short value = (short)this.lowerBound;
                int range = (int)(this.upperBound - this.lowerBound + 1);
                List<short> interval = new List<short>();
                for (int i = range; i > 0; i--)
                    interval.Add(value++);
                return interval.ToArray();
            }
            else if (this.DataType == TypeNode.DInt)
            {
                int value = (int)this.lowerBound;
                int range = (int)(this.upperBound - this.lowerBound + 1);
                List<int> interval = new List<int>();
                for (int i = range; i > 0; i--)
                    interval.Add(value++);
                return interval.ToArray();
            }
            else
            {
                long value = this.lowerBound;
                long range = this.upperBound - this.lowerBound + 1;
                List<long> interval = new List<long>();
                for (long i = range; i > 0; i--)
                    interval.Add(value++);
                return interval.ToArray();
            }
        }

        public override bool InRange(long lower, long upper)
        {
            return this.lowerBound >= lower && this.upperBound <= upper;
        }

        public long LowerBound 
        {
            get { return this.lowerBound; }
        }

        public long UpperBound
        {
            get { return this.upperBound; }
        }

        public long Range
        {
            get { return this.upperBound - this.lowerBound + 1; }
        }

        private readonly long lowerBound;

        private readonly long upperBound;
    }
}
