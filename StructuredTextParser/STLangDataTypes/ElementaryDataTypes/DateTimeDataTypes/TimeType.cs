using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public class TimeType : ElementaryType<TimeSpan>
    {
        public TimeType(string name = "TIME")
            : base(name, TimeSpan.MinValue, TimeSpan.MaxValue, TIME_SIZE, "P")
        {
            this.initialValue = new TimeConstant(new TimeSpan(0, 0, 0, 0), this);
        }

        public override bool IsTimeType 
        { 
            get { return true; } 
        }

        public override bool IsDateTimeType
        {
            get { return true; } 
        }
    }
}
