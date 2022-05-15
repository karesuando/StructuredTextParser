using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.ErrorManager;
using STLang.VMInstructions;
using STLang.MemoryLayout;
using STLang.Parser;

namespace STLang.Statements
{
    public class LocalVarDeclaration : DeclarationStatement
    {
        public LocalVarDeclaration(List<VarDeclStatement> locals, STVarType varType, STVarQualifier varQual, ProgramOrganizationUnitType pouType)
            : base(locals, varType, varQual, pouType)
        {
            this.varQualifier = varQual;
        }

        public override void GenerateCode(List<int> exitList)
        {
            foreach (VarDeclStatement varInitDecl in this.VarInitDeclList)
            {
                varInitDecl.GenerateCode();
            }
        }

        public override void SetDeclarationSize(RWMemoryLayoutManager memoryManager)
        {
            foreach (VarDeclStatement varInitDecl in this.VarInitDeclList)
            {
                varInitDecl.SetDeclarationInitData(memoryManager);
            }
        }

        private readonly STVarQualifier varQualifier;
    }
}
