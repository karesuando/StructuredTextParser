using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.ConstantTokens
{
    public class TokenDate : ConstantToken
    {
        public TokenDate(DateTime date, string strVal)
            : base(strVal)
        {
            this.date = date;
        }

        public DateTime Value 
        { 
            get { return this.date; } 
        }

        private readonly DateTime date;
    }
}
