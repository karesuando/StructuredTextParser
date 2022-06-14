using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;

namespace STLang.Subranges
{
    public class BitStringSubrange : SubRange
    {
        public BitStringSubrange(Expression lower, Expression upper, TypeNode dataType) 
            : base(lower, upper, dataType)
        {
            this.lowerBound = Convert.ToUInt64(lower.Evaluate());
            this.upperBound = Convert.ToUInt64(upper.Evaluate());
        }

        public BitStringSubrange(ulong lower, ulong upper, TypeNode dataType)
            : base(lower, upper, dataType)
        {
            this.lowerBound = lower;
            this.upperBound = upper;
        }

        public override bool Contains(SubRange subRange)
        {
            if (!(subRange is BitStringSubrange))
                return false;
            else
            {
                BitStringSubrange bitStringSubrange = (BitStringSubrange)subRange;
                ulong lower = bitStringSubrange.LowerBound;
                ulong upper = bitStringSubrange.UpperBound;
                return lower >= this.lowerBound && upper <= this.upperBound;
            }
        }

        public override bool IsDisjoint(SubRange subRange)
        {
            if (!(subRange is BitStringSubrange))
                return false;
            else
            {
                BitStringSubrange bitStringSubrange = (BitStringSubrange)subRange;
                ulong lower = bitStringSubrange.LowerBound;
                ulong upper = bitStringSubrange.UpperBound;
                return upper < this.lowerBound || lower > this.upperBound;
            }
        }

        public override bool Contains(Expression value)
        {
            if (value == null)
                return false;
            else if (!value.IsConstant)
                return false;
            else if (!value.DataType.IsBitStringType && !value.DataType.IsSignedIntType)
                return false;
            else {
                ulong uintValue = Convert.ToUInt64(value.Evaluate());
                return uintValue >= this.lowerBound && uintValue <= this.upperBound;
            }
        }

        public override object CreateInterval()
        {
            if (this.DataType == TypeNode.Byte)
            {
                byte value = (byte)this.lowerBound;
                int range = (int)(this.upperBound - this.lowerBound + 1);
                List<byte> interval = new List<byte>();
                for (int i = range; i > 0; i--)
                    interval.Add(value++);
                return interval.ToArray();
            }
            else if (this.DataType == TypeNode.Word)
            {
                ushort value = (ushort)this.lowerBound;
                int range = (int)(this.upperBound - this.lowerBound + 1);
                List<ushort> interval = new List<ushort>();
                for (int i = range; i > 0; i--)
                    interval.Add(value++);
                return interval.ToArray();
            }
            else if (this.DataType == TypeNode.DWord)
            {
                uint value = (uint)this.lowerBound;
                uint range = (uint)(this.upperBound - this.lowerBound + 1);
                List<uint> interval = new List<uint>();
                for (uint i = range; i > 0; i--)
                    interval.Add(value++);
                return interval.ToArray();
            }
            else
            {
                ulong value = this.lowerBound;
                ulong range = this.upperBound - this.lowerBound + 1;
                List<ulong> interval = new List<ulong>();
                for (ulong i = range; i > 0; i--)
                    interval.Add(value++);
                return interval.ToArray();
            }
        }

        public override bool InRange(ulong lower, ulong upper)
        {
            return lower >= this.lowerBound && upper <= this.upperBound;
        }

        public ulong LowerBound
        {
            get { return this.lowerBound; }
        }

        public ulong UpperBound
        {
            get { return this.upperBound; }
        }

        private readonly ulong lowerBound;

        private readonly ulong upperBound;
    }
}
