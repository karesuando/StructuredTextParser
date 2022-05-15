using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.DataTypes
{
    public class AnyMagnitudeType : AnyElementaryType
    {
        public AnyMagnitudeType(string name = "ANY_MAGNITUDE", uint size = uint.MaxValue)
            : base(name, size)
        {
        }
    }
}
