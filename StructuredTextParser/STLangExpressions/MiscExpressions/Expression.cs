using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.DataTypes;
using System.Collections;
using STLang.ErrorManager;
using STLang.MemoryLayout;
using STLang.ConstantTokens;
using STLang.ByteCodegenerator;
using STLang.Statements;

namespace STLang.Expressions
{
    public abstract class Expression : ByteCodeGenerator
    {
        public Expression(TypeNode dataType, string exprStr = "")
        {
            this.dataType = dataType;
            this.stringValue = exprStr;
        }

        static Expression()
        {
            Error = new ErrorExpression();
        }

        public string Name
        {
            get { return this.stringValue; }
        }

        public override string ToString()
        {
            return this.stringValue;
        }

        public int Length // Length of expression = Sum of the number of operands and operators in the expression
        { 
            get; 
            set; 
        }  

        public virtual TypeNode DataType 
        { 
            get { return this.dataType; } 
        }

        public virtual bool IsConstant 
        { 
            get { return false; } 
        }

        public virtual bool IsSmallConstant // -1,0,1
        {
            get { return false; }
        }

        public virtual bool IsTypedConstant
        {
            get { return false; }
        }

        public virtual bool IsConstantLValue
        {
            get { return false; }
        }

        public virtual bool IsZero
        {
            get { return false; }
        }

        public virtual bool IsLValue 
        { 
            get { return false; } 
        }

        public virtual bool IsCompoundExpression
        {
            get { return false; }
        }

        public virtual int Priority
        {
            get { return MAX_PRIORITY; }
        }

        public static ErrorHandler Report 
        { 
            get; 
            set; 
        }

        public virtual Expression DeMorgan()
        {
            return this;
        }

        public virtual Expression InvertRelation(bool doInvert = false)
        {
            return this;
        }

        public virtual bool IsInverted
        {
            get 
            { 
                throw new NotImplementedException("IsInverted getter not implemented."); 
            }
            set 
            { 
                throw new NotImplementedException("IsInverted setter not implemented."); 
            }
        }

        public virtual MemoryLocation Location
        {
            get { return MemoryLocation.UndefinedLocation; }
            set 
            { 
                throw new NotImplementedException("Location setter not implemented."); 
            }
        }

        public virtual object Evaluate()
        {
           throw new NotImplementedException("Evaluate() not implemented");
        }

        public virtual int Evaluate(int bound, List<ForLoopData> value)
        {
            throw new NotImplementedException("Evaluate(int, List<ForLoopData>) not implemented");
        }

        public virtual bool IsLinear()
        {
            return false;
        }

        public virtual bool ConstantForLoopBounds(List<ForLoopData> value)
        {
            return false;
        }

        public virtual void GenerateStore()
        {
            throw new NotImplementedException("GenerateStore() not implemented");
        }

        public virtual void GenerateBoolExpression(List<int> trueBranch, List<int> falseBranch)
        {
            this.GenerateLoad(trueBranch, falseBranch);
        }

        public virtual byte[] GetBytes()
        {
            throw new NotImplementedException("GetBytes() not implemented");
        }

        public virtual string GetKey()
        {
            return this.ToString();
        }

        public virtual void GenerateLoad(List<int> trueBranch = null, List<int> falseBranch = null)
        {
            throw new NotImplementedException("GenerateCode() not implemented");
        }

        private readonly TypeNode dataType;

        private readonly string stringValue;

        public static readonly Expression Error;

        public const int UNDEFINED_INDEX = int.MinValue;

        private const int MAX_PRIORITY = 10000;
    }
}
