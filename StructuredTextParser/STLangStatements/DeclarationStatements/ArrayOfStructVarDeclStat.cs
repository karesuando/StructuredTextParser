using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;
using STLang.MemoryLayout;
using STLang.Symbols;
using STLang.DataTypes;

namespace STLang.Statements
{
    public class ArrayOfStructVarDeclStatement : VarDeclStatement
    {
        public ArrayOfStructVarDeclStatement(List<MemoryObject> variables, TypeNode dataType,
                            STDeclQualifier declQual, Expression initializer,Expression size,
                            List<VarDeclStatement> memberInit)
            : base(variables, dataType, declQual, initializer, size)
        {
            this.memberInitList = memberInit;
        }

        public override void GenerateCode(List<int> exitList)
        {
            foreach (VarDeclStatement memberInitStat in this.memberInitList)
                memberInitStat.GenerateCode();
        }

        public override void SetDeclarationInitData(RWMemoryLayoutManager memoryLayOutManager)
        {
            foreach (VarDeclStatement memberInitStat in this.memberInitList)
                memberInitStat.SetDeclarationInitData(memoryLayOutManager);   
        }

        private readonly List<VarDeclStatement> memberInitList;
    }
}
