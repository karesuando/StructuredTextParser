using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.DataTypes
{
    public class AnyDerivedType : GenericType
    {
        public AnyDerivedType()
            : base("ANY_DERIVED", uint.MaxValue)
        {
        }
    }
}
