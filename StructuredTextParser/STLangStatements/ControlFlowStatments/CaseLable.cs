using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.DataTypes;

namespace STLang.Expressions
{
    public abstract class CaseLabel
    {
        public abstract bool IsDisjoint(SubrangeLabel label);

        public abstract bool IsDisjoint(NumericLabel label);
    }

    public class SubrangeLabel : CaseLabel
    {
        public SubrangeLabel(SubRange subRange)
        {
            this.subRange = subRange;
        }

        public override bool IsDisjoint(SubrangeLabel label)
        {
            return label.subRange.IsDisjoint(this.subRange);
        }

        public override bool IsDisjoint(NumericLabel label)
        {
            return ! this.subRange.Contains(label.Value);
        }

        public object CreateInterval()
        {
            return this.subRange.CreateInterval();
        }

        public SubRange SubRange 
        { 
            get { return this.subRange; } 
        }

        public override string ToString()
        {
            return this.subRange.ToString();
        }

        private readonly SubRange subRange;
    }

    public class NumericLabel : CaseLabel
    {
        public NumericLabel(Expression value)
        {
            this.value = value;
        }

        public override bool IsDisjoint(SubrangeLabel label)
        {
            return !label.SubRange.Contains(this.value);
        }

        public override bool IsDisjoint(NumericLabel label)
        {
            object constant1 = this.value.Evaluate();
            object constant2 = label.Value.Evaluate();
            return ! constant1.Equals(constant2);
        }

        public override string ToString()
        {
            return this.value.ToString();
        }

        public Expression Value 
        { 
            get { return this.value; } 
        }

        private readonly Expression value;
    }
}
