using System;
using System.Collections;
using System.Collections.Generic;
using STLang.DataTypes;
using STLang.Statements;
using STLang.MemoryLayout;
using STLang.Expressions;


namespace STLang.POUDefinitions
{
    public class ProgramOrganizationUnit
    {
        public string Name { get; set; }

        public TypeNode ReturnType { get; set; }

        public int InputCount { get; set; }

        public TypeNode[] InputDataTypes { get; set; }

        public POUVarDeclarations VarDeclarations { get; set; }

        public RWMemoryLayoutManager RWMemoryLayout { get; set; }

        public Dictionary<string, Expression> ConstantTable { get; set; }

        public StatementList Body { get; set; }

        public string Signature { get; set; }

        public string TypeID { get; set; }
    }
}
