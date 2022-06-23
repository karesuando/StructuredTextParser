using STLang.Expressions;
using STLang.ParserUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STLang.DataTypes
{
    public class NamedValueType : DerivedType
    {
        static NamedValueType()
        {
            typeNames = new List<string>();
        }

        public NamedValueType(string typeName, TypeNode baseType, Expression initialValue)
            : base(typeName, baseType, initialValue)
        {
            typeNames.Add(typeName);
            this.initialValue = initialValue;
            this.valueNames = new List<string>();
        }

        public void Add(string name)
        {
            this.valueNames.Add(name);
        }

        public static IEnumerable<string> TypeNames
        {
            get { return typeNames; }
        }

        public override bool IsNamedValueType { get { return true; } }

        private readonly List<string> valueNames;
        private static readonly List<string> typeNames;
    }
}
