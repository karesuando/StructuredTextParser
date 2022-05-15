using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Symbols;
using QUT.Gppg;

namespace STLang.Expressions
{
    public class DefaultStructInitializer : StructInitializer
    {
        public DefaultStructInitializer(TypeNode dataType, Expression size)
            : base(dataType, size)
        {
            this.Initialize(dataType);
        }

        public override void Add(Expression memberInit, LexLocation location)
        {
            string msg;
            msg = "void DefaultStructInitializer::Add() not implemented";
            throw new NotImplementedException(msg);
        }

        private void Initialize(TypeNode dataType)
        {
            if (dataType.IsStructType)
            {
                string key;
                FieldSymbol field;
                Expression initialValue;

                this.isZero = true;
                this.isConstant = true;
                field = this.structure.FirstField;
                while (field != null)
                {
                    key = field.Name.ToUpper();
                    initialValue = field.InitialValue;
                    this.initializers.Add(key, initialValue);
                    if (!initialValue.IsConstant)
                        this.isConstant = false;
                    if (!initialValue.IsZero)
                        this.isZero = false;
                    if (this.initializerString.Length > 0)
                        this.initializerString += ",";
                    this.initializerString += field.Name + " := " + initialValue;
                    field = field.Next;
                }
            }
        }
    }
}
