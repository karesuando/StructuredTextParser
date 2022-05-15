using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.DataTypes;
using STLang.Expressions;


namespace STLang.ParserUtility
{
    public class DataTypeSpec
    {
        public DataTypeSpec(TypeNode dataType, STDeclQualifier declQualifier, Expression initVal)
        {
            this.dataType = dataType;
            this.initialValue = initVal;
            this.declQualifier = declQualifier;
        }

        public TypeNode DataType { get { return this.dataType; } }

        public Expression InitialValue { get { return this.initialValue; } }

        public STDeclQualifier DeclQualifier { get { return this.declQualifier; } }

        private readonly TypeNode dataType;

        private readonly Expression initialValue;

        private readonly STDeclQualifier declQualifier;
    }
}
