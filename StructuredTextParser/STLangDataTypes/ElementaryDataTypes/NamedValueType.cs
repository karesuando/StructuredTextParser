using STLang.Expressions;
using STLang.ParserUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STLang.DataTypes
{
    public class NamedValueType : TypeNode
    {
        static NamedValueType()
        {
            typeNames = new List<string>();
        }
        public NamedValueType(string name, TypeNode baseType)
            : base(name, baseType.Size, baseType.TypeID)
        {
            this.baseType = baseType;
            this.initialValue = baseType.DefaultValue;
            this.names = new List<string>();
        }

        public NamedValueType(string name, TypeNode baseType, Expression initialValue)
            : base(name, baseType.Size, baseType.TypeID)
        {
            this.baseType = baseType;
            this.initialValue = initialValue;
            this.names = new List<string>();
        }

        public void Add(string name)
        {
            this.names.Add(name);
        }

        public static IEnumerable<string> TypeNames
        {
            get { return typeNames; }
        }

        public override TypeNode BaseType { get { return baseType; } }

        private readonly TypeNode baseType;
        private readonly List<string> names;
        private static readonly List<string> typeNames;
    }
}
