using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;
using STLang.DataTypes;
using QUT.Gppg;

namespace STLang.Symbols
{
    public class UndefinedFunctionBlockSymbol : ProgramOrganizationUnitSymbol
    {
        public UndefinedFunctionBlockSymbol(string name) : base(name, TypeNode.Error)
        {
            this.parameters = new List<ElementaryVariableSymbol>();
        }

        public override string TypeName { get { return "odeklarerad funktionsblock"; } }

        public override Expression MakeSyntaxTreeNode(LexLocation loc)
        {
            throw new NotImplementedException();
        }

        public override Expression MakeSyntaxTreeNode(List<Expressions.Expression> argList, LexLocation loc)
        {
            throw new NotImplementedException();
        }

        public void Add(List<ElementaryVariableSymbol> inputParams)
        {
            foreach (ElementaryVariableSymbol input in inputParams)
                parameters.Add(input);
        }

        private readonly List<ElementaryVariableSymbol> parameters;
    }
}
