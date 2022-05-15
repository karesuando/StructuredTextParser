using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.MemoryLayout;
using STLang.ErrorManager;
using STLang.ByteCodegenerator;

namespace STLang.Statements
{
    public abstract class VarDeclStatement : Statement
    {
        public VarDeclStatement(List<MemoryObject> variables, TypeNode dataType, STDeclQualifier declQual, Expression initValue, Expression size)
        {
            this.size = size;
            this.dataType = dataType;
            this.initialValue = initValue;
            this.declQualifier = declQual;
            this.varQualifier = variables[0].Symbol.VarQualifier;
            this.variableList = variables;
            this.symbolList = (from variable in variables select variable.Symbol).ToList();
            this.ByteCount = size;
        }

        public abstract void SetDeclarationInitData(RWMemoryLayoutManager memLayOutManager);

        public int Count
        {
            get { return this.symbolList.Count; }
        }

        public TypeNode DataType
        {
            get { return this.dataType; }
        }

        public Expression InitialValue
        {
            get { return this.initialValue; }
        }

        public Expression Size
        {
            get { return this.size; }
        }

        public Expression ByteCount
        {
            get;
            set;
        }

        public STDeclQualifier DeclQualifier
        {
            get { return this.declQualifier; }
        }

        public STVarQualifier VarQualifier
        {
            get { return this.varQualifier; }
        }

        public IEnumerable<InstanceSymbol> SymbolList
        {
            get { return this.symbolList; }
        }

        public IEnumerable<MemoryObject> VariableList
        {
            get { return this.variableList; }
        }

        private readonly Expression size;

        private readonly TypeNode dataType;

        private readonly Expression initialValue;

        private readonly STVarQualifier varQualifier;

        private readonly STDeclQualifier declQualifier;

        private readonly List<InstanceSymbol> symbolList;

        private readonly List<MemoryObject> variableList;
    }
}
