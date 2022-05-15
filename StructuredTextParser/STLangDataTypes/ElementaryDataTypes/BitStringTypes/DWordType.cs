using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.DataTypes
{
    public class DWordType : BitStringType<uint>
    {
        public DWordType(string name = "DWORD")
            : base(name, DWORD_SIZE, uint.MinValue, uint.MaxValue, "H")
        {
            this.initialValue = new DWordConstant(0, this, name + "#0"); 
        }

        public DWordType(string name, uint lowerBound, uint upperBound, TypeNode baseType, string typeID)
            : base(name, DWORD_SIZE, lowerBound, upperBound, baseType, typeID)
        {
            this.initialValue = new DWordConstant(lowerBound, this, name + "#" + lowerBound); 
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
            if (!subRange.DataType.IsBitStringType || !subRange.DataType.IsSignedIntType)
            {
                Report.SemanticError(-2, subRange.ToString(), this.Name, loc);
                return Error;
            }
            else if (!this.IsInRange(subRange))
            {
                Report.SemanticError(-3, subRange.ToString(), this.Name, loc);
                return DWord;
            }
            else if (subRange is IntSubrange)
            {
                IntSubrange intSubrange = (IntSubrange)subRange;
                uint lower = (uint)intSubrange.LowerBound;
                uint upper = (uint)intSubrange.UpperBound;
                string typeID = DWord.TypeID + "(" + subRange + ")";
                return new DWordType(name, lower, upper, this, typeID);
            }
            else
            {
                BitStringSubrange bitStringSubrange = (BitStringSubrange)subRange;
                uint lower = (uint)bitStringSubrange.LowerBound;
                uint upper = (uint)bitStringSubrange.UpperBound;
                string typeID = DWord.TypeID + "(" + subRange + ")";
                return new DWordType(name, lower, upper, this, typeID);
            }
        }
    }
}
