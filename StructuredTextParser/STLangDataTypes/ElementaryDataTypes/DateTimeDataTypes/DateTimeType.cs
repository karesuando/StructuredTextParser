using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public class DateAndTimeType : ElementaryType<DateTime>
    {
        public DateAndTimeType(string name = "DATE_AND_TIME")
            : base(name, DateTime.MinValue, DateTime.MaxValue, DT_SIZE, "E")
        {
            DateTime dateTime = new DateTime(1, 1, 1, 0, 0, 0);
            this.initialValue = new DateTimeConstant(dateTime, this);
        }

        public override bool IsDateClass 
        { 
            get { return true; } 
        }

        public override bool IsDateTimeType 
        { 
            get { return true; } 
        }
    }
}
