using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.SymbolTable;
using STLang.Statements;
using STLang.MemoryLayout;
using QUT.Gppg;

namespace STLang.Symbols
{
    public class StandardFunctionBlockSymbol : ProgramOrganizationUnitSymbol
    {
        public StandardFunctionBlockSymbol(string name, FunctionBlockType dataType, List<InstanceSymbol> members, int id)
            : base(name, dataType)
        {
            this.fbID = id;
            this.members = members;
            this.hasRetentiveData = false;
            this.hasRFEdgeDetection = false;
            foreach (InstanceSymbol member in members)
            {
                if (member.VarQualifier == STVarQualifier.RETAIN)
                    this.hasRetentiveData = true;
                if (member.EdgeQualifier == STDeclQualifier.F_EDGE
                 || member.EdgeQualifier == STDeclQualifier.R_EDGE)
                    this.hasRFEdgeDetection = true;
            }
        }

        public StandardFunctionBlockSymbol(string name, FunctionBlockType dataType, POUVarDeclarations memberDecls)
            : base(name, dataType)
        {
            IEnumerable<InstanceSymbol> members;
            this.hasRetentiveData = false;
            this.hasRFEdgeDetection = false;
            this.members = new List<InstanceSymbol>();
            members = memberDecls.VariableDeclList;
            foreach (InstanceSymbol member in members)
            {
                this.members.Add(member);
                if (member.VarQualifier == STVarQualifier.RETAIN)
                    this.hasRetentiveData = true;
                if (member.EdgeQualifier == STDeclQualifier.F_EDGE
                 || member.EdgeQualifier == STDeclQualifier.R_EDGE)
                    this.hasRFEdgeDetection = true;
            }
        }

        public override bool IsFunctionBlock 
        { 
            get { return true; } 
        }

        public override string TypeName 
        { 
            get { return "funktionsblock"; } 
        }

        public bool LookUp(string ident, out InstanceSymbol member)
        {
            ident = ident.ToUpper();
            member = this.members.Find(symbol => symbol.Name == ident);
            return member != null;
        }

        public void AddUndefinedInputParameter(string ident)
        {
            InstanceSymbol tmpVar = STLangSymbolTable.MakeInputVariable(ident, TypeNode.Error, -1);
            members.Add(tmpVar);
        }

        public override Expression MakeSyntaxTreeNode(LexLocation loc)
        {
            Report.SemanticError(5, "standardfunktionsblocket", this.Name, loc);
            return Expression.Error;
        }

        public override Expression MakeSyntaxTreeNode(List<Expression> arguments, LexLocation loc)
        {
            string exprStr = this.MakeExpressionString(arguments);
            return new FunctionBlockCall(arguments, this.DataType, this.fbID, exprStr);
        }

        public bool HasRetentiveData
        {
            get { return this.hasRetentiveData; }
        }

        public bool HasRFEdgeDetection
        {
            get { return this.hasRFEdgeDetection; }
        }

        private string MakeExpressionString(List<Expression> arguments)
        {
            if (arguments.Count() == 0)
                return this.Name + "()";
            else
            {
                string exprString = this.Name + "(" + arguments[0];
                foreach (Expression argument in arguments.Skip(1))
                {
                    exprString += "," + argument;
                }
                return exprString + ")";
            }
        }

        private readonly List<InstanceSymbol> members;

        private readonly bool hasRetentiveData;

        private readonly bool hasRFEdgeDetection;

        private readonly int fbID;
    }
}
