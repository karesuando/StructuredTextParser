using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.Symbols
{
    public class UndefinedFunctionSymbol : ProgramOrganizationUnitSymbol
    {
        public UndefinedFunctionSymbol(string name) 
            : base(name, TypeNode.Error)
        {
        }

        public override Expression MakeSyntaxTreeNode(LexLocation loc = null)
        {
            return Expression.Error;
        }

        public override Expression MakeSyntaxTreeNode(List<Expression> argList, LexLocation loc = null)
        {
            return Expression.Error;
        }

        public override string TypeName
        {
            get { return "funktion"; }
        }
        public override POUType POUType
        {
            get { return POUType.FUNCTION; }
        }
    }
}
