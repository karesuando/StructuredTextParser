using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.ErrorManager;
using STLang.ByteCodegenerator;
using STLang.MemoryLayout;
using STLang.Expressions;

namespace STLang.Statements
{
    public class POUVarDeclarations : ByteCodeGenerator
    {
        public POUVarDeclarations()
        {
            this.pouVarDeclarations = new List<DeclarationStatement>();
        }

        public POUVarDeclarations(DeclarationStatement declaration)
        {
            this.pouVarDeclarations = new List<DeclarationStatement>();
            this.pouVarDeclarations.Add(declaration);
        }

        public void Add(DeclarationStatement declaration)
        {
            this.pouVarDeclarations.Add(declaration);
        }

        public IEnumerable<DeclarationStatement> VariableDeclarations
        {
            get { return this.pouVarDeclarations; }
        }

        public IEnumerable<InstanceSymbol> VariableDeclList
        {
            get
            {
                return (from declaration in this.pouVarDeclarations
                        from varInitDecl in declaration.VarInitDeclList
                        select varInitDecl.SymbolList).SelectMany(symbols => symbols);
            }
        }

        public IEnumerable<InstanceSymbol> InputParameters
        {
            get
            {
                return (from declaration in this.pouVarDeclarations
                        where declaration.IsInputDeclaration
                        from varInitDecl in declaration.VarInitDeclList
                        select varInitDecl.SymbolList).SelectMany(symbols => symbols);
            }
        }

        public IEnumerable<InstanceSymbol> OutputParameters
        {
            get
            {
                return (from declaration in this.pouVarDeclarations
                        where declaration.IsOutputDeclaration
                        from varInitDecl in declaration.VarInitDeclList
                        select varInitDecl.SymbolList).SelectMany(symbols => symbols);
            }
        }

        public IEnumerable<InstanceSymbol> RetainedVariables
        {
            get
            {
                return (from declaration in this.pouVarDeclarations
                        where declaration.IsRetainedVariable
                        from varInitDecl in declaration.VarInitDeclList
                        select varInitDecl.SymbolList).SelectMany(symbols => symbols);
            }
        }

        public IEnumerable<DeclarationStatement> LocalVariables
        {
            get
            {
                return from declaration in this.pouVarDeclarations
                       where declaration.IsLocalDeclaration
                       select declaration;
            }
        }

        public void SetDeclarationSize(RWMemoryLayoutManager memoryManager)
        {
            foreach (DeclarationStatement declaration in this.pouVarDeclarations)
            {
                declaration.SetDeclarationSize(memoryManager);
            }
        }

        public Expression Size
        {
            get;
            set;
        }

        public override string ToString()
        {
            string declStr = "";
            foreach (DeclarationStatement declaration in this.pouVarDeclarations)
            {
                declStr += declaration.ToString();
            }
            return declStr;
        }

        private readonly List<DeclarationStatement> pouVarDeclarations;
    }
}
