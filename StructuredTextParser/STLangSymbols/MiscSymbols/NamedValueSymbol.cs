using QUT.Gppg;
using STLang.DataTypes;
using STLang.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STLang.Symbols
{
    public class NamedValueSymbol : STLangSymbol
    {
        public NamedValueSymbol(string name, TypeNode dataType, Expression value) : base(name, dataType)
        {
            this.Value = value;
        }

        public override string TypeName
        {
            get { return "namngiven konstant"; }
        }

        public Expression Value { get; private set; }

        public override Expression MakeSyntaxTreeNode(LexLocation loc = null)
        {
            return this.Value;
        }

        public override Expression MakeSyntaxTreeNode(List<Expression> argList, LexLocation loc = null)
        {
            throw new NotImplementedException();
        }
    }
}
