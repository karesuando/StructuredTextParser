using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public class UndefinedType : TypeNode
    {
        public UndefinedType(string name) : base(name, 0, "#" + name + "#")
        {
            this.symbols = new List<STLangSymbol>();
            this.initialValue = Expression.Error;
        }

        public void Add(List<STLangSymbol> symbolList)
        {
            this.symbols.AddRange(symbolList);
        }

        public void Add(STLangSymbol symbol)
        {
            this.symbols.Add(symbol);
        }

        public override bool IsUndefinedType
        {
            get { return true; }
        }

        private readonly List<STLangSymbol> symbols;
    }
}
