using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public class DateType : ElementaryType<DateTime>
    {
        public DateType(string name = "DATE")
            : base(name, DateTime.MinValue, DateTime.MaxValue, DATE_SIZE, "Q")
        {
            this.initialValue = new DateConstant(new DateTime(1, 1, 1), this);
        }

        public override bool IsDateClass 
        { 
            get { return true; } 
        }

        public override bool IsDateType 
        { 
            get { return true; } 
        }

        public override bool IsDateTimeType
        {
            get { return true; }
        }
    }
}
