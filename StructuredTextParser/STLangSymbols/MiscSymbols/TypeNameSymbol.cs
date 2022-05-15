using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;
using QUT.Gppg
;
namespace STLang.Symbols
{
    public class TypeNameSymbol : STLangSymbol
    {
        public TypeNameSymbol(string name, TypeNode dataType)
            : base(name,  dataType)
        {
        }

        public override Expression MakeSyntaxTreeNode(LexLocation loc)
        {
            Report.SemanticError(5, "typnamnet", this.Name, loc);
            return Expression.Error;
        }

        public override Expression MakeSyntaxTreeNode(List<Expression> argList, LexLocation loc)
        {
            Report.SemanticError(5, "typnamnet", this.Name, loc);
            return Expression.Error;
        }

        public override bool IsDerivedType
        {
            get { return true; }
        }

        public override string TypeName 
        { 
            get { return "typnamn"; } 
        }
    }
}
