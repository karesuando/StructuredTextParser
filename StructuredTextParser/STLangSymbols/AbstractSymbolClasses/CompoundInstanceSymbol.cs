using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;

namespace STLang.Symbols
{
    public abstract class CompoundInstanceSymbol : InstanceSymbol
    {
        public CompoundInstanceSymbol(string name, TypeNode dataType, STVarType varType,
                                STVarQualifier varQual, STDeclQualifier edgeQual, Expression initValue,
                                Dictionary<string, InstanceSymbol> members, InstanceSymbol firstMem, int position)
            : base(name, dataType, varType, varQual, edgeQual, initValue, position)
        {
            this.firstMember = firstMem;
            this.members = members;
        }

        public InstanceSymbol FirstMember
        {
            get { return this.firstMember; }
        }

        public override bool IsCompoundInstanceSymbol
        {
            get { return true; }
        }

        public bool LookUp(string name, out InstanceSymbol member)
        {
            string key = name.ToUpper();

            if (this.members.ContainsKey(key))
            {
                member = this.members[key];
                return true;
            }
            else
            {
                member = null;
                return false;
            }
        }

        protected readonly InstanceSymbol firstMember;

        protected readonly Dictionary<string, InstanceSymbol> members;
    }
}
