using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.ErrorManager;
using System.Collections;
using STLang.Parser;
using QUT.Gppg;
using STLang.Symbols;
using STLang.ParserUtility;

namespace STLang.Expressions
{
    public class FunctionBlockInitializer : InitializerList
    {
        public FunctionBlockInitializer(TypeNode dataType, Expression size) : base(dataType, size) 
        {
            if (!dataType.IsFunctionBlockType)
            {
                string errorMsg;
                errorMsg = "FunctionBlockInitializer expects function block data type.";
                this.memberCount = 0;
                this.functionBlock = null;
                this.initializers = null;
                throw new STLangCompilerError(errorMsg);
            }
            else
            {
                this.initializers = new Dictionary<string, Expression>();
                this.memberCount = ((FunctionBlockType)dataType).MemberCount;
                this.functionBlock = (FunctionBlockType)dataType;
            }
        }

        public override void Add(Expression memberInit, LexLocation location)
        {
            if (this.DataType.Size == 0)
                return; // Error struct type
            else if (memberInit == null)
                return;
            else if (!(memberInit is StructMemberInit))
                Report.SyntaxError(143, location);
            else
            {
                StructMemberInit member;
                member = (StructMemberInit)memberInit;
                string key = member.Member.ToUpper();
                Expression initialValue = member.InitValue;
                if (this.initializers.ContainsKey(key))
                    Report.Warning(24, member.Member, location);
                else
                    this.initializers[key] = initialValue;
                if (this.initializerString.Length > 0)
                    this.initializerString += ",";
                this.initializerString += member.Member + " := " + initialValue;
                if (!initialValue.IsConstant)
                    this.isConstant = false;
                if (!initialValue.IsZero)
                    this.isZero = false;
            }
        }

        public override InitializerList Expand(int factor, ErrorHandler report)
        {
            throw new NotImplementedException("FunctionBlockInitializer.Expand()");
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            string key;
            Expression initialValue;
            FunctionBlockType functionBlock;
            functionBlock = (FunctionBlockType)this.DataType.BaseType;
            InstanceSymbol member = functionBlock.FirstMember;

            while (member != null)
            {
                key = member.Name.ToUpper();
                if (!this.initializers.ContainsKey(key))
                {
                    string msg = "FunctionBlockInitializer.GenerateCode(): Member "
                               + member.Name + " not found in function block "
                               + this.DataType.Name;
                    throw new STLangCompilerError(msg);
                }
                else
                {
                    initialValue = this.initializers[key];
                    initialValue.GenerateLoad();
                }
                member = member.Next;
            }
        }

        public override void CheckInitListSize(LexLocation location)
        {
            if (this.initializers.Count < this.memberCount)
            {
                string key;
                FunctionBlockType functionBlock;
                functionBlock = (FunctionBlockType)this.DataType;
                InstanceSymbol member = functionBlock.FirstMember;

                while (member != null)
                {
                    key = member.Name.ToUpper();
                    if (!this.initializers.ContainsKey(key))
                    {
                        this.initializers[key] = member.InitialValue;
                        Report.Warning(28, key, location);
                    }
                    member = member.Next;
                }
            }
        }

        public Expression GetInitializer(string member)
        {
            if (!this.DataType.IsFunctionBlockType)
                return Expression.Error;
            else
            {
                InstanceSymbol memberSymbol;
                string key = member.ToUpper();
                if (!this.functionBlock.LookUp(member, out memberSymbol))
                {
                    string msg;
                    msg = "GetFieldInitializer(): Member " + member + " not found in function block.";
                    throw new STLangCompilerError(msg);
                }
                else if (this.initializers.ContainsKey(key))
                    return this.initializers[key];
                else
                    return memberSymbol.DataType.DefaultValue;
            }
        }

        public bool Contains(string member, out Expression initializer)
        {
            string key = member.ToUpper();
            if (!this.initializers.ContainsKey(key))
            {
                initializer = Expression.Error;
                return false;
            }
            else
            {
                initializer = this.initializers[key];
                return true;
            }
        }

        public override int Count
        {
            get { return this.initializers.Count; }
        }

        public override string ToString()
        {
            return "(" + this.initializerString + ")";
        }

        protected readonly int memberCount;

        protected readonly FunctionBlockType functionBlock;

        protected readonly Dictionary<string, Expression> initializers;
    }
}
