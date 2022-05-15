using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.MemoryLayout;

namespace STLang.Expressions
{
    public abstract class Constant<T> : GenericConstant
    {
        public Constant(TypeNode dataType, T value)
            : base(dataType, value.ToString())
        {
            this.value = value;
            this.Length = 1;
            this.isTypedConstant = false;
            this.Location = MemoryLocation.UndefinedLocation;
        }

        public Constant(TypeNode dataType, T value, string exprStr)
            : base(dataType, exprStr)
        {
            this.value = value;
            this.Length = 1;
            this.isTypedConstant = exprStr.Contains('#');
            this.Location = MemoryLocation.UndefinedLocation;
        }

        public override bool IsConstant
        {
            get { return true; }
        }

        public override bool IsTypedConstant
        {
            get { return this.IsTypedConstant; }
        }

        public override bool IsLinear()
        {
            return true;
        }

        public override MemoryLocation Location
        {
            get;
            set;
        }

        public override object Evaluate()
        {
            return this.value;
        }

        protected T Value 
        { 
            get { return this.value; } 
        }

        private readonly bool isTypedConstant;

        private readonly T value;
    }
}
