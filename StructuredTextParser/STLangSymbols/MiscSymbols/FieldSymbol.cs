using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;
using STLang.DataTypes;
using QUT.Gppg;

namespace STLang.Symbols
{
    public class FieldSymbol : STLangSymbol
    {
        public FieldSymbol(string name, TypeNode dataType)
            : base(name, dataType)
        {
            this.initValue = dataType.DefaultValue;
        }

        public FieldSymbol(string name, TypeNode dataType, Expression initValue) 
            : base(name, dataType)
        {
            this.initValue = initValue;
        }

        public override string TypeName 
        { 
            get { return "strukturfält"; } 
        }

        public override Expression MakeSyntaxTreeNode(LexLocation loc)
        {
            throw new NotImplementedException();
        }

        public override Expression MakeSyntaxTreeNode(List<Expression> argList, LexLocation loc)
        {
            throw new NotImplementedException();
        }

        public Expression InitialValue 
        { 
            get { return this.initValue; } 
        }

        public FieldSymbol Next 
        { 
            get; 
            set; 
        }

        private readonly Expression initValue;
    }
}
