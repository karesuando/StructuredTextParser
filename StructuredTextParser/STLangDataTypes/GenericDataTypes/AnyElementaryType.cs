using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.DataTypes
{
    public class AnyElementaryType : GenericType
    {
        public AnyElementaryType(string name = "ANY_ELEMENTARY", uint size = uint.MaxValue)
            : base(name, size)
        {
        }
    }
}
