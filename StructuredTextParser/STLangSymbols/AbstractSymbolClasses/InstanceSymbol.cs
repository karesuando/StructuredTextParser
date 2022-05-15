using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.MemoryLayout;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.Symbols
{
    // public abstract class DataSymbol
    //
    // Abstract base class for classes that represent variables in functions and 
    // function block data members.
    //
    public abstract class InstanceSymbol : STLangSymbol
    {
        public InstanceSymbol(string name, TypeNode dataType, STVarType varType,
                          STVarQualifier varQual, STDeclQualifier edgeQual, Expression initValue, 
                          MemoryLocation index, int position)
            : base(name, dataType)
        {
            this.varType = varType;
            this.varQualifier = varQual;
            this.edgeQualifier = edgeQual;
            this.initialValue = initValue;
            this.index = index;
            this.position = position;
            this.isConstant = varType == STVarType.VAR_INPUT 
                           || varQual == STVarQualifier.CONSTANT
                           || varQual == STVarQualifier.CONSTANT_RETAIN;
        }

        public InstanceSymbol(string name, TypeNode dataType, STVarType varType,
                         STVarQualifier varQual, STDeclQualifier edgeQual, Expression initValue, int position)
            : base(name, dataType)
        {
            this.varType = varType;
            this.varQualifier = varQual;
            this.edgeQualifier = edgeQual;
            this.initialValue = initValue;
            this.index = MemoryLocation.UndefinedLocation;
            this.position = position;
            this.isConstant = varType == STVarType.VAR_INPUT
                           || varQual == STVarQualifier.CONSTANT
                           || varQual == STVarQualifier.CONSTANT_RETAIN;
        }

        public override MemoryLocation Location 
        {
            get { return this.index; } 
        }

        public int Position
        {
            get { return this.position; }
        }

        public STVarType VariableType 
        { 
            get { return this.varType; } 
        }

        public string VariableTypeString
        {
            get
            {
                if (this.varQualifier != STVarQualifier.NONE)
                    return this.varType + " " + this.varQualifier;
                else
                    return this.varType.ToString();
            }
        }

        /// <summary>
        /// Variable qualifier: NONE, CONSTANT, RETAIN, ...
        /// </summary>
        public STVarQualifier VarQualifier 
        { 
            get { return this.varQualifier; } 
        }

        public STDeclQualifier EdgeQualifier 
        { 
            get { return this.edgeQualifier; } 
        }

        public override Expression MakeSyntaxTreeNode(LexLocation loc = null)
        {
            return new MemoryObject(this.Name, this.Location, this.DataType, this);
        }

        public override Expression MakeSyntaxTreeNode(List<Expression> argList, LexLocation loc = null)
        {
            Report.SemanticError(5, this.TypeName, this.Name, loc);
            return Expression.Error;
        }

        public bool IsLocalVariable
        {
            get 
            { 
                return this.varType == STVarType.VAR 
                    || this.varType == STVarType.VAR_TEMP; 
            }
        }

         public InstanceSymbol Next 
        { 
            get; 
            set;
        }

        public override bool IsVariable
        {
            get { return true; }
        }

        public Expression InitialValue
        {
            get { return this.initialValue; }
        }

        public bool IsConstant
        {
            get { return this.isConstant; }
        }

        public override bool IsInstanceSymbol
        {
            get { return true; }
        }

        public override string TypeName
        {
            get
            {
                switch (this.VariableType)
                {
                    case STVarType.VAR_INPUT:
                        return "inparameter";
                    case STVarType.VAR_OUTPUT:
                        return "utparameter";
                    case STVarType.VAR_INOUT:
                        return "in-ut-parameter";
                    case STVarType.VAR:
                        return "lokal variabel";
                    case STVarType.VAR_EXTERNAL:
                        return "extern variabel";
                    case STVarType.VAR_GLOBAL:
                        return "global variabel";
                    case STVarType.VAR_TEMP:
                        return "temporär variabel";
                    default:
                        return "variabel";
                }
            }
        }

        private readonly bool isConstant;

        private readonly STVarType varType;

        private readonly STVarQualifier varQualifier;

        private readonly STDeclQualifier edgeQualifier;

        private readonly Expression initialValue;

        private readonly MemoryLocation index;

        private readonly int position;
    }
}
