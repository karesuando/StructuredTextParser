using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.ErrorManager;
using QUT.Gppg;

namespace STLang.Expressions
{
    public abstract class InitializerList : Expression
    {
        public InitializerList(TypeNode dataType, Expression size)
            : base(dataType)
        {
            this.size = size;
            this.isZero = false;
            this.isConstant = true;
            this.initializerString = string.Empty;
        }

        public virtual void Add(Expression initElem, LexLocation loc)
        {
            throw new NotImplementedException("InitializerList.Add() not implemented");
        }

        public virtual InitializerList Expand(int factor, ErrorHandler report)
        {
            throw new NotImplementedException("InitializerList.Expand() not implemented");
        }

        public virtual void CheckInitListSize(LexLocation location)
        {
            string msg;
            msg = "InitializerList.CheckInitListSize() not implemented";
            throw new NotImplementedException(msg);
        }

        public virtual int Count 
        {
            get { throw new NotImplementedException("InitializerList.Count not implemented."); } 
        }

        public override bool IsConstant
        {
            get { return this.isConstant; }
        }

        public override bool IsZero
        {
            get { return this.isZero; }
        }

        public Expression Size
        {
            get { return this.size; }
        }

        public Expression AbsoluteAddress
        {
            get;
            set;
        }

        protected bool isZero;

        protected bool isConstant;

        protected string initializerString;

        private readonly Expression size;
    }
}
