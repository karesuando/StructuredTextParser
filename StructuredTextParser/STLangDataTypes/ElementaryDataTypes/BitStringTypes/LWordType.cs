using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.DataTypes
{
    public class LWordType : BitStringType<ulong>
    {
        public LWordType(string name = "LWORD")
            : base(name, LWORD_SIZE, ulong.MinValue, ulong.MaxValue, "V")
        {
            this.initialValue = new LWordConstant(0, this, name + "#0");
        }

        public LWordType(string name, ulong lowerBound, ulong upperBound, TypeNode baseType, string typeID)
            : base(name, LWORD_SIZE, lowerBound, upperBound, baseType, typeID)
        {
            string strValue = name + "#" + lowerBound;
            this.initialValue = new LWordConstant(lowerBound, this, strValue);
        }

        public override bool IsInRange(ulong value)
        {
            return value >= this.LowerBound && value <= this.UpperBound;
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
            if (!subRange.DataType.IsBitStringType || !subRange.DataType.IsSignedIntType)
            {
                Report.SemanticError(-2, subRange.ToString(), this.Name, loc);
                return Error;
            }
            else if (!this.IsInRange(subRange))
            {
                Report.SemanticError(-3, subRange.ToString(), this.Name, loc);
                return LWord;
            }
            else if (subRange is IntSubrange)
            {
                IntSubrange intSubrange = (IntSubrange)subRange;
                ulong lower = (ulong)intSubrange.LowerBound;
                ulong upper = (ulong)intSubrange.UpperBound;
                string typeID = LWord.TypeID + "(" + subRange + ")";
                return new LWordType(name, lower, upper, this, typeID);
            }
            else
            {
                BitStringSubrange bitStringSubrange = (BitStringSubrange)subRange;
                ulong lower = (ulong)bitStringSubrange.LowerBound;
                ulong upper = (ulong)bitStringSubrange.UpperBound;
                string typeID = LWord.TypeID + "(" + subRange + ")";
                return new LWordType(name, lower, upper, this, typeID);
            }
        }
    }
}
