using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.DataTypes
{
    public class DIntType : SignedIntegerType<int>
    {
        public DIntType(string name = "DINT")
            : base(name, DINT_SIZE, int.MinValue, int.MaxValue, "D")
        {
            this.initialValue = new DIntConstant(0, name, this);
        }

        public DIntType(string name, int lowerBound, int upperBound, TypeNode baseType, string typeID)
            : base(name, DINT_SIZE, lowerBound, upperBound, baseType, typeID)
        {
            string initValStr = name + "#" + lowerBound.ToString();
            this.initialValue = new DIntConstant(lowerBound, initValStr);
        }

        public DIntType(string name, int lowerBound, int upperBound, TypeNode baseType, string typeID, Expression initValue)
            : base(name, DINT_SIZE, lowerBound, upperBound, baseType, typeID)
        {
            this.initialValue = initValue;
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
                return DInt;
            }
            else
            {
                IntSubrange intSubRange = (IntSubrange)subRange;
                int lower = (int)intSubRange.LowerBound;
                int upper = (int)intSubRange.UpperBound;
                string typeID = DInt.TypeID + "(" + subRange + ")";
                return new DIntType(name, lower, upper, this, typeID);
             }
        }
    }
}
