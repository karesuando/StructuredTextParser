using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.ConstantTokens
{
    public class TokenTime : ConstantToken
    {
        public TokenTime(TimeSpan duration, string strVal)
            : base(strVal)
        {
            this.time = duration;
        }

        public TimeSpan Value 
        { 
            get { return this.time; } 
        }

        private readonly TimeSpan time;
    }
}
