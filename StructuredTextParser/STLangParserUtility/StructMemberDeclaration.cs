using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.ParserUtility
{
    public class StructMemberDeclaration
    {
        public StructMemberDeclaration(string name, TypeNode dataType, Expression initialValue, LexLocation location)
        {
            this.name = name;
            this.dataType = dataType;
            this.initValue = initialValue;
            this.location = location;
        }

        public string Name
        {
            get { return this.name; }
        }

        public Expression InitValue
        {
            get { return this.initValue; }
        }

        public TypeNode DataType
        {
            get { return this.dataType; }
        }

        public LexLocation Location
        {
            get { return this.location; }
        }

        private readonly string name;

        private readonly TypeNode dataType;

        private readonly Expression initValue;

        private readonly LexLocation location;
    }
}
