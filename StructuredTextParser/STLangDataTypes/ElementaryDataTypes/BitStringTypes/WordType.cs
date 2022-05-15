using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.DataTypes
{
    public class WordType : BitStringType<ushort>
    {
        public WordType(string name = "WORD")
            : base(name, WORD_SIZE, ushort.MinValue, ushort.MaxValue, "W")
        {
            this.initialValue = new WordConstant(0, this, name + "#0");
        }

        public WordType(string name, ushort lowerBound, ushort upperBound, TypeNode baseType, string typeID)
            : base(name, WORD_SIZE, lowerBound, upperBound, baseType, typeID)
        {
            string strValue = name + "#" + this.LowerBound;
            this.initialValue = new WordConstant(this.LowerBound, this, strValue);
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
                return Word;
            }
            else if (subRange is IntSubrange)
            {
                IntSubrange intSubrange = (IntSubrange)subRange;
                ushort lower = (ushort)intSubrange.LowerBound;
                ushort upper = (ushort)intSubrange.UpperBound;
                string typeID = Word.TypeID + "(" + subRange + ")";
                return new WordType(name, lower, upper, this, typeID);
            }
            else
            {
                BitStringSubrange bitStringSubrange = (BitStringSubrange)subRange;
                ushort lower = (ushort)bitStringSubrange.LowerBound;
                ushort upper = (ushort)bitStringSubrange.UpperBound;
                string typeID = Word.TypeID + "(" + subRange + ")";
                return new WordType(name, lower, upper, this, typeID);
            }
        }
    }
}
