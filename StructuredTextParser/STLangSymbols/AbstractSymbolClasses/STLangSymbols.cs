using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.ErrorManager;
using QUT.Gppg;
using STLang.MemoryLayout;

namespace STLang.Symbols
{
    public abstract class STLangSymbol
    {
        public STLangSymbol(string name)
        {
            this.name = name;
            this.dataType = TypeNode.Any;
        }

        public STLangSymbol(string name, TypeNode dataType)
        {
            this.name = name;
            this.dataType = dataType;
        }

        public virtual bool IsFunction 
        { 
            get { return false; } 
        }

        public virtual bool IsFunctionBlock 
        { 
            get { return false; } 
        }

        public virtual bool IsFunctionBlockInstance
        {
            get { return false; }
        }

        public virtual bool IsStructInstance
        {
            get { return false; }
        }

        public virtual bool IsArrayInstance
        {
            get { return false; }
        } 

        public virtual bool IsVariable
        {
            get { return false; }
        }

        public virtual bool IsDerivedType
        {
            get { return false; } 
        }

        public virtual bool IsEnumeratedConst
        {
            get { return false; } 
        }

        public virtual bool IsInstanceSymbol
        {
            get { return false; }
        }

        public virtual bool IsCompoundInstanceSymbol
        {
            get { return false; }
        }

        public string Name 
        { 
            get { return this.name; } 
        }

        public virtual TypeNode DataType
        {
            get { return this.dataType; }
            protected set { ;}
        }

        public virtual bool IsForLoopCtrlVar
        {
            get {return false;}
            set {;}
        }

        public static ErrorHandler Report 
        { 
            get; 
            set; 
        }

        public virtual MemoryLocation Location
        {
            get { throw new NotImplementedException("Getter Location not implemented."); }
        }

        public abstract Expression MakeSyntaxTreeNode(LexLocation loc = null);

        public abstract Expression MakeSyntaxTreeNode(List<Expression> argList, LexLocation loc = null);

        public abstract string TypeName { get; }

        private readonly string name;

        private readonly TypeNode dataType;
    }
}
