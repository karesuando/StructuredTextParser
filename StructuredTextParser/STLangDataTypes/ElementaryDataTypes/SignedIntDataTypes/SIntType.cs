using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.DataTypes
{
    public class SIntType : SignedIntegerType<sbyte>
    {
        public SIntType(string name = "SINT")
            : base(name, SINT_SIZE, sbyte.MinValue, sbyte.MaxValue, "S")
        {
            this.initialValue = new SIntConstant(0, this, name + "#0");
        }

        public SIntType(string name, sbyte lowerBound, sbyte upperBound, TypeNode baseType, string typeID)
            : base(name, SINT_SIZE, lowerBound, upperBound, baseType, typeID)
        {
            this.initialValue = new SIntConstant(lowerBound, this, name + "#" + lowerBound);
        }

        public SIntType(string name, sbyte lowerBound, sbyte upperBound, Expression initVal, TypeNode baseType, string typeID)
            : base(name, SINT_SIZE, lowerBound, upperBound, baseType, typeID)
        {
            this.initialValue = initVal;
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

        public override TypeNode MakeSubrange(string name, SubRange subRange, LexLocation loc)
        {
            if (! (subRange is IntSubrange))
            {
                Report.SemanticError(-2, subRange.ToString(), this.Name, loc);
                return Error;
            }
            else if (!this.IsInRange(subRange))
            {
                Report.SemanticError(-3, subRange.ToString(), this.Name, loc);
                return SInt;
            }
            else
            {
                IntSubrange intSubrange = (IntSubrange)subRange;
                sbyte lower = (sbyte)intSubrange.LowerBound;
                sbyte upper = (sbyte)intSubrange.UpperBound;
                string typeID = SInt.TypeID + "(" + subRange + ")";
                return new SIntType(name, lower, upper, this, typeID);
            }
          
        }
    }
}
