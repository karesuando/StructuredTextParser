using STLang.DataTypes;
using STLang.Expressions;
using System.Collections.Generic;

namespace STLang.ParserUtility
{
    public class NamedValueList
    {
        public NamedValueList(TypeNode dataType)
        {
            this.DataType = dataType;
            this.nameValueDict = new Dictionary<string, Expression>();
        }
        public NamedValueList(string name, Expression value, TypeNode dataType)
        {
            this.DataType = dataType;
            this.InitialValue = value;
            this.nameValueDict = new Dictionary<string, Expression>();
            this.nameValueDict.Add(name, value);

        }
        public TypeNode DataType { get; private set; }
        public Expression InitialValue { get; private set; }
        public IEnumerable<KeyValuePair<string, Expression>> NamedValues { 
            get {
                foreach (KeyValuePair<string, Expression> keyValuePair in this.nameValueDict)
                    yield return keyValuePair;
            } 
        }
        public bool Add(string name, Expression value)
        {
            if (nameValueDict.ContainsKey(name))
                return false;
            else {
                this.nameValueDict.Add(name, value);
                return true;
            }
        }

        private readonly Dictionary<string, Expression> nameValueDict;
    }
}
