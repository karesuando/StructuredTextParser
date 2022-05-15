using System;
using System.Collections;
using System.Collections.Generic;
using STLang.DataTypes;
using STLang.Statements;
using STLang.SymbolTable;
using STLang.Expressions;
using STLang.MemoryLayout;
using STLang.POUDefinitions;
using QUT.Gppg;

namespace STLang.Symbols
{
    /// <summary>
    /// class UserDefinedFunctionBlockSymbol represents a user defined function block type.
    /// </summary>
    public class UserDefinedFunctionBlockSymbol : StandardFunctionBlockSymbol
    {
        public UserDefinedFunctionBlockSymbol(string name, FunctionBlockType dataType, POUVarDeclarations members)
            : base(name, dataType, members)
        {
            List<TypeNode> inputTypes = new List<TypeNode>();
            foreach (InstanceSymbol input in members.InputParameters)
                inputTypes.Add(input.DataType);

            this.functionBlockDef = new ProgramOrganizationUnit();
            this.functionBlockDef.Name = name;
            this.functionBlockDef.VarDeclarations = members;
            this.functionBlockDef.InputDataTypes = inputTypes.ToArray();
            this.functionBlockDef.TypeID = "{" + this.Name + "}";
            this.pouDefinitions.Add(functionBlockDef);
        }

        public void SaveDefinition(StatementList body, Dictionary<string, Expression> constantTable, RWMemoryLayoutManager rwMemoryLayout)
        {
            this.functionBlockDef.Body = body;
            this.functionBlockDef.RWMemoryLayout = rwMemoryLayout;
            this.functionBlockDef.ConstantTable = constantTable;
        }

        public override Expression MakeSyntaxTreeNode(LexLocation loc)
        {
            Report.SemanticError(5, "funktionsblockstypen", this.Name, loc);
            return Expression.Error;
        }

        private readonly ProgramOrganizationUnit functionBlockDef;
    }
}
