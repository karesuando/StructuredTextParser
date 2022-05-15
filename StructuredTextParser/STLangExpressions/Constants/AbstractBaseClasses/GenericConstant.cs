using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.SymbolTable;
using STLang.MemoryLayout;
using STLang.ConstantTokens;

namespace STLang.Expressions
{
    public abstract class GenericConstant : Expression
    {
        public GenericConstant(TypeNode dataType, string exprStr)
            : base(dataType, exprStr)
        {
        }

        protected MemoryLocation GetConstantIndex(string stringValue = null)
        {
            return ConstMemoryManager.GenerateIndex(this.DataType, stringValue);
        }


        public static ROMemoryLayoutManager ConstMemoryManager
        {
            get;
            set;
        }
    }
}
