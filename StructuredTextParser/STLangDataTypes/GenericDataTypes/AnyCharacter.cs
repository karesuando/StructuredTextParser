using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public abstract class AnyCharacter<T> : OrdinalType<T> where T : struct, IComparable<T>
    {
        public AnyCharacter(string name, uint size, T lower, T upper, string typeID)
          : base(name, size, lower, upper, typeID)
        {

        }

        public AnyCharacter(string name, uint size, T lower, T upper, TypeNode baseType, string typeID)
            : base(name, size, lower, upper, baseType, typeID)
        {

        }

    }
}
