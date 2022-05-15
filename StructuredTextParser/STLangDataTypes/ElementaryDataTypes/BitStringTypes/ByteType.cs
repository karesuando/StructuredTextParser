using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.DataTypes
{
    public class ByteType : BitStringType<byte>
    {
        public ByteType(string name = "BYTE")
            : base(name, BYTE_SIZE, byte.MinValue, byte.MaxValue, "Z")
        {
            this.initialValue = new ByteConstant(0, this, name + "#0");
        }

        public ByteType(string name, byte lowerBound, byte upperBound, TypeNode baseType, string typeID)
            : base(name, BYTE_SIZE, lowerBound, upperBound, baseType, typeID)
        {
            this.initialValue = new ByteConstant(lowerBound, this, name + "#" + lowerBound);
        }

        public override bool IsInRange(ulong value)
        {
            return value <= this.UpperBound;
        }

        public override bool IsInRange(SubRange subRange)
        {
            return subRange.InRange(this.LowerBound, this.UpperBound);
        }

        public override SubRange GetSubrange()
        {
            return new BitStringSubrange(this.LowerBound, this.UpperBound, this);
        }

        public override TypeNode MakeSubrange(string name, SubRange subRange, LexLocation loc)
        {
            if (!subRange.DataType.IsBitStringType && !subRange.DataType.IsSignedIntType)
            {
                Report.SemanticError(-2, subRange.ToString(), this.Name, loc);
                return Error;
            }
            else if (!this.IsInRange(subRange))
            {
                Report.SemanticError(-3, subRange.ToString(), this.Name, loc);
                return Byte;
            }
            else if (subRange is IntSubrange)
            {
                IntSubrange intSubrange = (IntSubrange)subRange;
                byte lower = (byte)intSubrange.LowerBound;
                byte upper = (byte)intSubrange.UpperBound;
                string typeID = Byte.TypeID + "(" + subRange + ")";
                return new ByteType(name, lower, upper, this, typeID);
            }
            else
            {
                BitStringSubrange bitStringSubrange = (BitStringSubrange)subRange;
                byte lower = (byte)bitStringSubrange.LowerBound;
                byte upper = (byte)bitStringSubrange.UpperBound;
                string typeID = Byte.TypeID + "(" + subRange + ")";
                return new ByteType(name, lower, upper, this, typeID);
            }
        }
    }
}
