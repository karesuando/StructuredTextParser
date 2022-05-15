using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.DataTypes
{
    public class ULIntType : UnsignedIntegerType<ulong>
    {
        public ULIntType(string name = "ULINT")
            : base(name, ULINT_SIZE, ulong.MinValue, ulong.MaxValue, "G")
        {
            this.initialValue = new ULIntConstant(0, this, name + "#0");
        }

        public ULIntType(string name, ulong lowerBound, ulong upperBound, TypeNode baseType, string typeID)
            : base(name, ULINT_SIZE, lowerBound, upperBound, baseType, typeID)
        {
            string strValue = name + "#" + lowerBound.ToString();
            this.initialValue = new ULIntConstant(lowerBound, TypeNode.ULInt, strValue);
        }

        public ULIntType(string name, ulong lowerBound, ulong upperBound, Expression initVal, TypeNode baseType, string typeID)
            : base(name, ULINT_SIZE, lowerBound, upperBound, baseType, typeID)
        {
            this.initialValue = initVal;
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
            return new UIntSubrange(this.LowerBound, this.UpperBound, this);
        }

        public override TypeNode MakeSubrange(string name, SubRange subRange, LexLocation loc)
        {
            if (! subRange.DataType.IsIntegerType)
            {
                Report.SemanticError(-2, subRange.ToString(), this.Name, loc);
                return Error;
            }
            else if (!this.IsInRange(subRange))
            {
                Report.SemanticError(-3, subRange.ToString(), this.Name, loc);
                return ULInt;
            }
            else if (subRange is IntSubrange)
            {
                IntSubrange intSubrange = (IntSubrange)subRange;
                ulong lower = (ulong)intSubrange.LowerBound;
                ulong upper = (ulong)intSubrange.UpperBound;
                string typeID = ULInt.TypeID + "(" + subRange + ")";
                return new ULIntType(name, lower, upper, this, typeID);
            }
            else
            {
                UIntSubrange uintSubrange = (UIntSubrange)subRange;
                ulong lower = uintSubrange.LowerBound;
                ulong upper = uintSubrange.UpperBound;
                string typeID = ULInt.TypeID + "(" + subRange + ")";
                return new ULIntType(name, lower, upper, this, typeID);
            }
        }
    }
}
