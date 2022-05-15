using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.MemoryLayout;
using STLang.ErrorManager;

namespace STLang.Statements
{
    public class StructVarDeclStatement : VarDeclStatement
    {
        public StructVarDeclStatement(List<MemoryObject> variables, TypeNode dataType, STDeclQualifier declQual,
                                      Expression initValue, Expression size, List<VarDeclStatement> memberInitList)
            : base(variables, dataType, declQual, initValue, size)
        {
            this.memberInitList = memberInitList;
        }

        public override void SetDeclarationInitData(RWMemoryLayoutManager memoryLayOutManager)
        {
            foreach (VarDeclStatement memberInitStat in this.memberInitList)
                memberInitStat.SetDeclarationInitData(memoryLayOutManager);   
        }

        public override void GenerateCode(List<int> exitList)
        {
            foreach (VarDeclStatement memberInitStat in this.memberInitList)
                memberInitStat.GenerateCode();
        }

        private readonly List<VarDeclStatement> memberInitList;
    }
}
