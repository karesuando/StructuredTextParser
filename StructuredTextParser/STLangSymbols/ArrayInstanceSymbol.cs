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
    public class ArrayInstanceSymbol : InstanceSymbol
    {
        public ArrayInstanceSymbol(string name, TypeNode dataType, STVarType varType,
                          STVarQualifier varQual, STDeclQualifier edgeQual, 
                          Expression initValue, InstanceSymbol elemSym, int position)
            : base(name, dataType, varType, varQual, edgeQual, initValue, position)
        {
            this.elementSymbol = elemSym;
        }

        public override Expression MakeSyntaxTreeNode(LexLocation loc = null)
        {
            return new MemoryObject(this.Name, this.Location, this.DataType, this);
        }

        public override Expression MakeSyntaxTreeNode(List<Expression> argList, LexLocation loc = null)
        {
            Report.SemanticError(5, this.TypeName, this.Name, loc);
            return new MemoryObject(this.Name, this.Location, this.DataType, this);
        }

        public InstanceSymbol ElementSymbol
        {
            get { return this.elementSymbol; }
        }

        public override bool IsArrayInstance
        {
            get { return true; }
        }

        public override MemoryLocation Location
        {
            get { return elementSymbol.Location; }
        } 

        private readonly InstanceSymbol elementSymbol;
    }
}
