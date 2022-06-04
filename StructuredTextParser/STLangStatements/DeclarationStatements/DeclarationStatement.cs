using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.Expressions;
using STLang.MemoryLayout;
using STLang.ByteCodegenerator;

namespace STLang.Statements
{
    public abstract class DeclarationStatement : Statement
    {
        public DeclarationStatement(VarDeclStatement varDeclStat, STVarType varType, STVarQualifier varQual, POUType pouType)
        {
            this.variableType = varType;
            this.variableQualifier = varQual;
            this.varInitDeclList = new List<VarDeclStatement>{varDeclStat};
            this.pouType = pouType;
        }

        public DeclarationStatement(List<VarDeclStatement> varDeclStatList, STVarType varType, STVarQualifier varQual, POUType pouType)
        {
            this.variableType = varType;
            this.variableQualifier = varQual;
            this.varInitDeclList = varDeclStatList;
            this.pouType = pouType;
        }

        public IEnumerable<VarDeclStatement> VarInitDeclList
        {
            get { return this.varInitDeclList; }
        }

        public STVarType VariableType
        {
            get { return this.variableType; }
        }

        public STVarQualifier VariableQualifier
        {
            get { return this.variableQualifier; }
        }

        public POUType ProgramOrganizationUnitType
        {
            get { return this.pouType; }
        }

        public bool IsConstantDeclaration
        {
            get
            {
                return this.variableQualifier == STVarQualifier.CONSTANT
                    || this.variableQualifier == STVarQualifier.CONSTANT_RETAIN;
            }
        }

        public bool IsInputDeclaration
        {
            get
            {
                return this.variableType == STVarType.VAR_INPUT
                    || this.variableType == STVarType.VAR_INOUT;
            }
        }

        public bool IsOutputDeclaration
        {
            get
            {
                return this.variableType == STVarType.VAR_OUTPUT;
            }
        }

        public bool IsRetainedVariable
        {
            get
            {
                return this.variableQualifier == STVarQualifier.RETAIN
                    || this.variableQualifier == STVarQualifier.CONSTANT_RETAIN;
            }
        }

        public bool IsLocalDeclaration
        {
            get 
            { 
                return this.variableType == STVarType.VAR
                    || this.variableType == STVarType.VAR_TEMP
                    || this.variableType == STVarType.VAR_OUTPUT; 
            }
        }

        public abstract void SetDeclarationSize(RWMemoryLayoutManager memory);

        public override string ToString()
        {
            int count;
            string typeName;
            string identList = "";
            string varDecl = this.variableType.ToString() + "\n";
            foreach (VarDeclStatement varInitDecl in this.varInitDeclList)
            {
                identList = "    ";
                count = varInitDecl.Count;
                foreach (InstanceSymbol symbol in varInitDecl.SymbolList)
                {
                    identList += symbol.Name;
                    if (count > 1)
                        identList += ",";
                    count--;
                }
                typeName = varInitDecl.DataType.Name;
                identList += " : " + typeName + ";\n";
                varDecl += identList;
            }
            varDecl += "\nEND_VAR\n";
            return varDecl;
        }

        private readonly STVarType variableType;

        private readonly STVarQualifier variableQualifier;

        private readonly List<VarDeclStatement> varInitDeclList;

        private readonly POUType pouType;
    }
}
