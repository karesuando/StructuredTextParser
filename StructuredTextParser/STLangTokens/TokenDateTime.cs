using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.ConstantTokens
{
    public class TokenDateTime : ConstantToken
    {
        public TokenDateTime(DateTime dateTime, string strVal)
            : base(strVal)
        {
            this.dateTime = dateTime;
        }

        public DateTime Value 
        { 
            get { return this.dateTime; } 
        }

        private readonly DateTime dateTime;
    }
}
