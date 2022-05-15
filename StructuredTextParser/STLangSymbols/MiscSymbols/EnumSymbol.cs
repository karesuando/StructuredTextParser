using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.Symbols
{
    public class EnumSymbol : STLangSymbol
    {
        public EnumSymbol(string name, TypeNode dataType, ushort enumValue) : base(name, dataType)
        {
            this.enumValue = enumValue;
        }

        public override string TypeName 
        { 
            get { return "uppräknad konstant"; } 
        }

        public override bool IsEnumeratedConst
        {
            get { return true; }
        }

        public override Expression MakeSyntaxTreeNode(LexLocation loc)
        {
            return new EnumConstant(this.enumValue, this.DataType, this.Name);
        }

        public override Expression MakeSyntaxTreeNode(List<Expression> argList, LexLocation loc)
        {
            Report.SemanticError(5, "uppräknad konstant.", this.Name, loc);
            return new EnumConstant(this.enumValue, this.DataType, this.Name);
        }

        private readonly ushort enumValue;
    }
}
