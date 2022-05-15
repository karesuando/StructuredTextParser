using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.ConstantTokens
{
    public class TokenTOD : ConstantToken
    {
        public TokenTOD(int hour, int sec, int min, int millisec, string strVal)
            : base(strVal)
        {
            this.timeSpan = new TimeSpan(0, hour, min, sec, millisec);
        }

        public TimeSpan Value 
        { 
            get { return this.timeSpan; } 
        }

        private readonly TimeSpan timeSpan;
    }
}
