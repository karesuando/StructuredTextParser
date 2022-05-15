using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.MemoryLayout;

namespace STLang.ParserUtility
{
    public class IndexedVariable
    {
        public IndexedVariable()
        {
            this.Offset = MemoryLocation.UndefinedLocation;
            this.DataType = TypeNode.Error;
            this.VariablePart = Expression.Error;
            this.ConstantPart = int.MinValue;
            this.Symbol = null;
            this.Name = "";
            this.Length = 0;
        }

        public IndexedVariable(MemoryLocation offset, TypeNode dataType, Expression varPart, int constPart, InstanceSymbol symbol, string name, int length)
        {
            this.Offset = offset;
            this.DataType = dataType;
            this.VariablePart = varPart;
            this.ConstantPart = constPart;
            this.Symbol = symbol;
            this.Length = length;
            this.Name = name;
        }

        public MemoryLocation Offset
        {
            get;
            private set;
        }

        public TypeNode DataType
        {
            get;
            private set;
        }

        public Expression VariablePart
        {
            get;
            private set;
        }

        public int ConstantPart
        {
            get;
            private set;
        }

        public InstanceSymbol Symbol
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public int Length
        {
            get;
            private set;
        }
    }
}
