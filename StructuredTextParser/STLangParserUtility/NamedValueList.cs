using STLang.DataTypes;
using System.Collections.Generic;

namespace STLang.ParserUtility
{
    public class NamedValueList
    {
        public NamedValueList(NamedValue namedValue, TypeNode dataType)
        {
            this.DataType = dataType;
            this.namedValueList = new List<NamedValue>() { namedValue };
        }

        public TypeNode DataType { get; private set; }
        public IEnumerable<NamedValue> NamedValues { get { return this.namedValueList; } }
        public void Add(NamedValue namedValue)
        {
            this.namedValueList.Add(namedValue);
        }

        private readonly List<NamedValue> namedValueList;
    }
}
