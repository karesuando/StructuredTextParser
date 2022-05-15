using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.ErrorManager;
using QUT.Gppg;


namespace STLang.Symbols
{
    public class StructInstanceSymbol : CompoundInstanceSymbol
    {
        public StructInstanceSymbol(string name, TypeNode dataType, STVarType varType,
                          STVarQualifier varQual, STDeclQualifier edgeQual, Expression initValue,
                          Dictionary<string, InstanceSymbol> members, InstanceSymbol firstMem, int position)
            : base(name, dataType, varType, varQual, edgeQual, initValue, members, firstMem, position)
        {
        }

        public override bool IsStructInstance
        {
            get { return true; }
        }
    }
}
