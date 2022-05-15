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
    public class FunctionBlockVarDeclStat : VarDeclStatement
    {
        public FunctionBlockVarDeclStat(List<MemoryObject> variables, TypeNode dataType,
                                        STDeclQualifier declQual, Expression initialValue,
                                        Expression size, List<VarDeclStatement> memberDeclStats)
            : base(variables, dataType, declQual, initialValue, size)
        {
            this.memberDeclStats = memberDeclStats;
        }

        public override void GenerateCode(List<int> exitList)
        {
            foreach (VarDeclStatement memberDeclStat in this.memberDeclStats)
                memberDeclStat.GenerateCode();
        }

        public override void SetDeclarationInitData(RWMemoryLayoutManager memLayOutManager)
        {

        }

        private readonly List<VarDeclStatement> memberDeclStats;
    }
}
