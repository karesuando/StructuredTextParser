using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.Expressions;
using STLang.MemoryLayout;

namespace STLang.Statements
{
    public class IOParameterDeclaration : DeclarationStatement
    {
        public IOParameterDeclaration(STVarType formalParType, STVarQualifier formalParQual, 
                                      List<VarDeclStatement> formalParDecls, POUType pouType)
            : base(formalParDecls, formalParType, formalParQual, pouType)
        {
           
        }

        public override void SetDeclarationSize(RWMemoryLayoutManager memory)
        {
        }

        public override void GenerateCode(List<int> exitList)
        {
            foreach (VarDeclStatement varInitDecl in this.VarInitDeclList)
            {
                Expression formal;
                List<Expression> formalList = new List<Expression>();
        
                switch (this.VariableType)
                {
                    case STVarType.VAR_INPUT:
                        foreach (InstanceSymbol formalSymbol in varInitDecl.SymbolList)
                        {
                            formal = formalSymbol.MakeSyntaxTreeNode();
                            formalList.Add(formal);
                        }
                        foreach (Expression inputParameter in formalList)
                            inputParameter.GenerateStore();
                        break;

                    case STVarType.VAR_INOUT:
                        foreach (InstanceSymbol formalSymbol in varInitDecl.SymbolList)
                        {
                            formal = formalSymbol.MakeSyntaxTreeNode();
                            formalList.Add(formal);
                        }
                        foreach (Expression inOutParameter in formalList)
                            inOutParameter.GenerateStore();
                        break;

                    case STVarType.VAR_OUTPUT:
                        varInitDecl.GenerateCode();
                        //switch (this.ProgramOrganizationUnitType)
                        //{
                        //    case POUType.FUNCTION:
                        //        varInitDecl.GenerateCode();
                        //        break;

                        //    case POUType.FUNCTION_BLOCK:
                        //        foreach (InstanceSymbol outputSymbol in varInitDecl.SymbolList)
                        //        {

                        //        }
                        //        break;

                        //    case POUType.PROGRAM:
                        //        break;

                        //}
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
