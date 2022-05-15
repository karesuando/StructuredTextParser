using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public class TimeOfDayType : ElementaryType<TimeSpan>
    {
        public TimeOfDayType(string name = "TIME_OF_DAY")
            : base(name, new TimeSpan(0, 0, 0), new TimeSpan(23, 59, 59), DINT_SIZE, "J")
        {
            this.initialValue = new TimeOfDayConstant(new TimeSpan(0, 0, 0), this);
        }

        public override bool IsDateClass 
        { 
            get { return true; } 
        }

        public override bool IsTimeOfDayType 
        { 
            get { return true; } 
        }
    }
}
