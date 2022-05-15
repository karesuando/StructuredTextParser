using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.ErrorManager;
using STLang.VMInstructions;
using QUT.Gppg;

namespace STLang.Expressions
{
    public class DefaultArrayInitializer : ArrayInitializer
    {
        public DefaultArrayInitializer(TypeNode dataType, Expression size)
            : base(dataType, size)
        {
            if (! dataType.IsArrayType)
            {
                this.defaultValue = Expression.Error;
                this.isConstant = false;
                this.isZero = false;
                this.initializerString = "";
            }
            else {
                ArrayType array = dataType as ArrayType;
                this.defaultValue = array.ElementType.DefaultValue;
                this.isConstant = this.defaultValue.IsConstant;
                this.isZero = this.defaultValue.IsZero;
                this.initializerString = array.Range + "(" + this.defaultValue + ")";
                for (int i = this.elementCount; i > 0; i--)
                {
                    this.initializerList.Add(this.defaultValue);
                    this.flattenedInitList.Add(this.defaultValue);
                }
            }
        }

        public DefaultArrayInitializer(TypeNode dataType, Expression defaultValue, Expression size)
            : base(dataType, size)
        {
            if (!dataType.IsArrayType)
            {
                this.defaultValue = Expression.Error;
                this.isConstant = false;
                this.isZero = false;
                this.initializerString = "";
            }
            else
            {
                ArrayType array = dataType as ArrayType;
                this.defaultValue = defaultValue;
                this.isConstant = this.defaultValue.IsConstant;
                this.isZero = this.defaultValue.IsZero;
                this.initializerString = array.Range + "(" + defaultValue + ")";
                for (int i = this.elementCount; i > 0; i--)
                {
                    this.initializerList.Add(defaultValue);
                    this.flattenedInitList.Add(defaultValue);
                }
            }
        }

        public override void Add(Expression initElem, LexLocation loc)
        {
            throw new NotImplementedException("DefaultArrayInitializer.Add() not implemented.");
        }

        public override void GenerateLoad(List<int> trueBranch = null, List<int> falseBranch = null)
        {
            if (this.defaultValue == null)
            {
                string msg;
                msg = "DefaultArrayInitializer.GenerateLoad(): Initialvalue is null.";
                throw new STLangCompilerError(msg);
            }
            else if (!this.defaultValue.IsZero)
                this.defaultValue.GenerateLoad();
        }

        private readonly Expression defaultValue;
    }
}
