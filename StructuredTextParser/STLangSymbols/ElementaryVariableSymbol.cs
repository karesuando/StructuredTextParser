using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.MemoryLayout;
using QUT.Gppg;

namespace STLang.Symbols
{
    public class ElementaryVariableSymbol : InstanceSymbol
    {
        public ElementaryVariableSymbol(string name, TypeNode dataType, STVarType varType,
                              STVarQualifier varQual, STDeclQualifier edgeQual, 
                              Expression initValue, MemoryLocation index, int position)
            : base(name, dataType, varType, varQual, edgeQual, initValue, index, position)
        {
            this.IsForLoopCtrlVar = false;
        }

        public ElementaryVariableSymbol(string name, TypeNode dataType, STVarType varType,
                              STVarQualifier varQual, STDeclQualifier edgeQual, Expression initValue, int position)
            : base(name, dataType, varType, varQual, edgeQual, initValue, position)
        {
            this.IsForLoopCtrlVar = false;
        }

        public override Expression MakeSyntaxTreeNode(LexLocation loc)
        {
            return new MemoryObject(this.Name, this.Location, this.DataType, this);
        }

        public override Expression MakeSyntaxTreeNode(List<Expression> argList, LexLocation loc)
        {
            Report.SemanticError(5, this.TypeName, this.Name, loc);
            return new MemoryObject(this.Name, this.Location, this.DataType, this);
        }

        public override bool IsForLoopCtrlVar
        {
            get;
            set;
        }
    }
}
