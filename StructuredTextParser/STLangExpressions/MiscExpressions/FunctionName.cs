using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.DataTypes;
using STLang.ErrorManager;

namespace STLang.Expressions
{
    public class FunctionName : Expression
    {
        public FunctionName(STLangSymbol symbol)
            : base(symbol.DataType, symbol.Name)
        {
            this.functionSymbol = symbol;
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            throw new STLangCompilerError("Illegal usage of function name.");
        }

        public STLangSymbol FunctionSymbol
        {
            get { return this.functionSymbol; }
        }

        private readonly STLangSymbol functionSymbol;
    }
}
