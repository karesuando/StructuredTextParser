using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using STLang.Symbols;
using STLang.Expressions;
using STLang.MemoryLayout;


namespace STLang.DataTypes
{
    public class FunctionBlockType : TypeNode
    {
        public FunctionBlockType(string name, List<InstanceSymbol> members, uint size, Expression eSize, string typeID) 
            : base(name, size, typeID)
        {
            string key;
            InstanceSymbol prevMember = null;
            this.members = new Dictionary<string, InstanceSymbol>();
            foreach (InstanceSymbol member in members)
            {
                key = member.Name.ToUpper();
                this.members[key] = member;
                if (prevMember == null)
                     this.firstMember = member;
                else
                    prevMember.Next = member;
                prevMember = member;
            }
            this.memberCount = members.Count;
            this.initialValue = new DefaultFunctionBlockInitializer(this, eSize);
        }

        public bool LookUp(string ident, out InstanceSymbol member)
        {
            string name = ident.ToUpper();
            if (this.members.ContainsKey(name))
            {
                member = this.members[name];
                return true;
            }
            else
            {
                member = null;
                InstanceSymbol undeclVarSymbol;
                MemoryLocation index = new ElementaryLocation(-1, TypeNode.Error);
                undeclVarSymbol = new ElementaryVariableSymbol(name, TypeNode.Error, STVarType.VAR_OUTPUT, 
                                        STVarQualifier.NONE, STDeclQualifier.NONE, Expression.Error, index, -1);
                this.members.Add(name, undeclVarSymbol);
                return false;
            }
        }

        public override float ConversionCost(Expression expression)
        {
            if (expression == null)
                return 0.0f;
            else if (expression.DataType == this)
                return 0.0f;
            else
                return MAX_CONVERSION_COST;
        }

        public int MemberCount
        {
            get { return this.memberCount; }
        }

        public InstanceSymbol FirstMember
        {
            get { return this.firstMember; }
        }

        public override bool IsFunctionBlockType
        {
            get { return true; }
        }

        private readonly int memberCount;

        private readonly InstanceSymbol firstMember;

        private readonly Dictionary<string, InstanceSymbol> members;
    }
}
