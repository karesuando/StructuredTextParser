using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.DataTypes
{
    public class LIntType : SignedIntegerType<long>
    {
        public LIntType(string name = "LINT")
            : base(name, LINT_SIZE, long.MinValue, long.MaxValue, "L")
        {
            this.initialValue = new LIntConstant(0, this, name + "#0");
        }

        public LIntType(string name, long lowerBound, long upperBound, TypeNode baseType, string typeID)
            : base(name, LINT_SIZE, lowerBound, upperBound, baseType, typeID)
        {
            this.initialValue = new LIntConstant(lowerBound, this, name + "#" + lowerBound);
        }

        public LIntType(string name, long lowerBound, long upperBound, Expression initVal, TypeNode baseType, string typeID)
            : base(name, LINT_SIZE, lowerBound, upperBound, baseType, typeID)
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
                return Error;
            }
            else
            {
                IntSubrange intSubrange = (IntSubrange)subRange;
                long lower = intSubrange.LowerBound;
                long upper = intSubrange.UpperBound;
                string typeID = LInt.TypeID + "(" + subRange + ")";
                return new LIntType(name, lower, upper, this, typeID);
            }
        }
    }
}
