using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.ConstantTokens;
using QUT.Gppg;
using STLang.MemoryLayout;

namespace STLang.Symbols
{
    public class DirectRepVarSymbol : InstanceSymbol
    {
        public DirectRepVarSymbol(TokenDirectVar directVar) 
            : base(directVar.ToString(), TypeNode.Error, STVarType.VAR, STVarQualifier.NONE, 
                   STDeclQualifier.NONE, Expression.Error, -1)
        {
            this.location = char.ToUpper(directVar.Location);
            this.size = directVar.Size;
            this.address = directVar.Address;
        }

        public DirectRepVarSymbol(string name, TypeNode dataType, STVarType varType, STVarQualifier varQual,
                                      STDeclQualifier edgeQual, char location, char size)
            : base(name, dataType, varType, varQual, edgeQual, dataType.DefaultValue, -1)
        {
            this.location = char.ToUpper(location);
            this.size = char.ToUpper(size);
            this.address = new ushort[4] { 0, 0, 0, 0 };
        }

        public DirectRepVarSymbol(string name, TypeNode dataType, STVarType varType, STVarQualifier varQual,
                                  STDeclQualifier edgeQual, char location, char size, ushort[] address)
            : base(name, dataType, varType, varQual, edgeQual, dataType.DefaultValue, -1)
        {
            this.location = char.ToUpper(location);
            this.size = char.ToUpper(size);
            this.address = address;
        }

        public override string TypeName 
        { 
            get { return "direkt representerad variabel"; } 
        }

        public void SetAddress(ushort address, int level)
        {
            this.address[level] = address;
        }

        public override Expression MakeSyntaxTreeNode(LexLocation loc)
        {
            throw new NotImplementedException();
        }

        public override Expression MakeSyntaxTreeNode(List<Expression> argList, LexLocation loc)
        {
            throw new NotImplementedException();
        }

        public char Location2 
        { 
            get { return this.location; } 
        }

        public char Size 
        { 
            get { return this.size; } 
        }

        public IEnumerable<ushort> Address 
        { 
            get { return this.address; } 
        }

        private readonly char location;

        private readonly char size;

        private readonly ushort[] address;
    }
}
