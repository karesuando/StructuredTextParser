using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QUT.Gppg;
using STLang.DataTypes;
using STLang.Symbols;

namespace STLang.Expressions
{
    public class DefaultFunctionBlockInitializer : FunctionBlockInitializer
    {
        public DefaultFunctionBlockInitializer(TypeNode dataType, Expression size)
            : base(dataType, size) 
        {
            this.Initialize(dataType);
        }

        public override void Add(Expression memberInit, LexLocation location)
        {
            string msg;
            msg = "void DefaultFunctionBlockInitializer::Add() not implemented";
            throw new NotImplementedException(msg);
        }

        private void Initialize(TypeNode dataType)
        {
            if (dataType.IsFunctionBlockType)
            {
                string key;
                InstanceSymbol member;
                Expression initialValue;

                member = ((FunctionBlockType)dataType).FirstMember;
                while (member != null)
                {
                    key = member.Name.ToUpper();
                    initialValue = member.InitialValue;
                    this.initializers[key] = initialValue;
                    if (this.initializerString.Length > 0)
                        this.initializerString += ",";
                    this.initializerString += member.Name + " := " + initialValue;
                    if (!initialValue.IsConstant)
                        this.isConstant = false;
                    if (!initialValue.IsZero)
                        this.isZero = false;
                    member = member.Next;
                }
            }
        }
    }
}
