using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.Statements
{
    public class EmptyStatement : Statement
    {
        public override void GenerateCode(List<int> exitList)
        {
        }
    }
}
