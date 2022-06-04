using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using STLang.DataTypes;
using STLang.Extensions;
using STLang.Statements;
using STLang.Expressions;
using STLang.ErrorManager;
using STLang.MemoryLayout;
using STLang.ParserUtility;
using STLang.VMInstructions;
using STLang.POUDefinitions;
using QUT.Gppg;
using StructuredTextParser.Properties;

namespace STLang.Symbols
{
    public class UserDefinedFunctionSymbol : ProgramOrganizationUnitSymbol
    {
        public UserDefinedFunctionSymbol(string name) 
            : base(name, TypeNode.Error)
        {
        }

        static UserDefinedFunctionSymbol()
        {
            ReInitialize();
        }

        public static void ReInitialize()
        {
            functionID = 1;
            functionIDTable = new Hashtable();
        }

        public override bool IsFunction 
        { 
            get { return true; } 
        }

        public override POUType POUType
        {
            get { return POUType.FUNCTION; }
        }

        public override Expression MakeSyntaxTreeNode(LexLocation loc)
        {
            return new FunctionName(this);
        }

        public override TypeNode DataType
        {
            get;
            protected set;
        }

        private void CheckInputParameters(List<Expression> arguments, MatchedFunction function)
        {
            foreach (Expression argument in arguments)
            {

            }
        }

        private void SortInputArguments(List<Expression> arguments, out List<Expression> inputs, out List<Expression> outputs)
        {
            List<POUParameter> inputs2;
            inputs2 = new List<POUParameter>();
            outputs = new List<Expression>();
            foreach (Expression argument in arguments)
            {
                if (argument is InputParameter)
                    inputs2.Add(argument as POUParameter);
                else if (argument is OutputParameter)
                    outputs.Add(argument);
            }
            inputs = (from input in inputs2
                      orderby input.Position
                      select input.RValue).ToList();
        }

        public override Expression MakeSyntaxTreeNode(List<Expression> arguments, LexLocation loc)
        {
            List<Expression> inputs,outputs;
            List<MatchedFunction> matchingFunctions;
           
            this.SortInputArguments(arguments, out inputs, out outputs);
            matchingFunctions = this.GetBestMatchingOverloadedFunction(inputs);
            if (matchingFunctions.Count == 0)
            {
                // Too many parameters
                Report.SemanticError(6, this.Name, loc);
                return Expression.Error;
            }
            else if (matchingFunctions[0].Cost >= TypeNode.MAX_CONVERSION_COST)
            {
                // No matching overloaded function found
                string signature = this.GetFunctionSignature(inputs);
                Report.SemanticError(7, signature, loc);
                return Expression.Error;
            }
            else if (matchingFunctions.Count == 1)
            {
                ProgramOrganizationUnit function;
                int fcnIndex = matchingFunctions[0].Number;
                function = this.pouDefinitions[fcnIndex];
                string exprString = this.MakeExpressionString(arguments);
                Expression[] parameters = inputs.Concat(outputs).ToArray();
                return new UserDefinedFunctionCall(parameters, function.ReturnType, 0, exprString, this);
                
            }
            else if (matchingFunctions[0].Cost == matchingFunctions[1].Cost)
            {
                // Ambiguous function call
                float cost0 = matchingFunctions[0].Cost;
                List<MatchedFunction> sameCostList;
                sameCostList = matchingFunctions.FindAll(f => f.Cost == cost0);
                string functionSign = this.GetFunctionSignature(inputs);
                string signatureList = this.MakeFunctionSignatureList(sameCostList);
                Report.SemanticError(18, functionSign, signatureList, loc);
                return Expression.Error;
            }
            else
            {
                ProgramOrganizationUnit function;
                int fcnIndex = matchingFunctions[0].Number;
                function = this.pouDefinitions[fcnIndex];
                string exprString = this.MakeExpressionString(arguments);
                Expression[] parameters = inputs.Concat(outputs).ToArray();
                return new UserDefinedFunctionCall(parameters, function.ReturnType, 0, exprString, this);
            }
        }

        public void Add(TypeNode returnType, POUVarDeclarations varDecls, LexLocation location)
        {
            IEnumerable<InstanceSymbol> formals = varDecls.InputParameters;
            string signature = this.GetSignature(formals);
            if (this.SignatureExists(signature))
               Report.SemanticError(0, this.Name, signature, location);
            else
            {
                uint functionID = this.GetFunctionID(this.Name, signature);
                ProgramOrganizationUnit functionDef = new ProgramOrganizationUnit()
                {
                    Name = this.Name + functionID,
                    POUType = POUType.FUNCTION,
                    ReturnType = returnType,
                    VarDeclarations = varDecls,
                    InputDataTypes = this.MakeDataTypeList(formals),
                    Signature = signature,
                    TypeID = signature + "->" + returnType.TypeID,
                    InputCount = formals.Count()
                };
                this.pouDefinitions.Add(functionDef);
                this.DataType = returnType;
            }
        }

        public void SaveDefinition(StatementList functionBody, Dictionary<string, Expression> constants, RWMemoryLayoutManager rwMemoryLayout)
        {
            ProgramOrganizationUnit function = this.pouDefinitions.Last();
            if (function == null)
                throw new STLangCompilerError(Resources.SAVEFCNBODY);
            else
            {
                function.Body = functionBody;
                function.ConstantTable = constants;
                function.RWMemoryLayout = rwMemoryLayout;
            }
        }

        private TypeNode[] MakeDataTypeList(IEnumerable<InstanceSymbol> formalList)
        {
            return (from formal in formalList
                    select formal.DataType).ToArray();
        }

        private class MatchedFunction
        {
            public MatchedFunction(float cost, int index)
            {
                this.cost = cost;
                this.number = index;
            }

            public float Cost 
            { 
                get { return this.cost; } 
            }

            public int Number 
            { 
                get { return this.number; } 
            }

            private readonly float cost;

            private readonly int number;
        }

        private bool SignatureExists(string signature)
        {
            return this.pouDefinitions.Find(pou => pou.Signature == signature) != null;
        }

        private List<MatchedFunction> GetBestMatchingOverloadedFunction(List<Expression> actuals)
        {
            int position = 0;
            List<MatchedFunction> conversionCost = new List<MatchedFunction>();
            foreach (ProgramOrganizationUnit function in this.pouDefinitions)
            {
                if (function.InputCount >= actuals.Count)
                {
                    float totalCost = 0.0f;
                    TypeNode[] formalType = function.InputDataTypes;
                    for (int i = 0; i < actuals.Count; i++)
                    {
                        totalCost += formalType[i].ConversionCost(actuals[i]);
                    }
                    conversionCost.Add(new MatchedFunction(totalCost, position));
                }
                position++;
            }
            conversionCost.Sort((f1, f2) => { return f1.Cost.CompareTo(f2.Cost); });
            return conversionCost;
        }

        private string GetSignature(IEnumerable<InstanceSymbol> formalList)
        {
            int count = 0;
            string signature = "";
            foreach (InstanceSymbol formal in formalList)
            {
                if (count > 0)
                    signature += ",";
                signature += formal.DataType.TypeID;
                count++;
            }
            return "(" + signature + ")";
        }

        private string GetFunctionSignature(List<Expression> argList)
        {
            string exprString = this.Name + "(";
            exprString += argList[0].DataType.Name;
            for (int i = 1; i < argList.Count; i++)
                exprString += "," + argList[i].DataType.Name;
            exprString += ")";
            return exprString;
        }

        private string MakeFunctionSignatureList(List<MatchedFunction> list)
        {
            int i;
            string typeList;
            string funcSignList = "";
            TypeNode[] formalTypeList;
            foreach (MatchedFunction function in list)
            {
                typeList = "";
                i = function.Number;
                if (funcSignList.Length > 0)
                    funcSignList += ",";
                funcSignList += this.Name + "(";
                formalTypeList = this.pouDefinitions[i].InputDataTypes;
                foreach (TypeNode dataType in formalTypeList)
                {
                    if (typeList.Length > 0)
                        typeList += ",";
                    typeList += dataType.Name;
                }
                funcSignList += typeList + ")";
            }
            return funcSignList;
        }

        private string MakeExpressionString(List<Expression> arguments)
        {
            string exprString = this.Name + "(";
            if (arguments.Count > 0)
            {
                exprString += arguments[0].ToString();
                for (int i = 1; i < arguments.Count; i++)
                    exprString += "," + arguments[i].ToString();
            }
            exprString += ")";
            return exprString;
        }

        private uint GetFunctionID(string name, string signature)
        {
            string key = name + ":" + signature;
            key = key.ToUpper();
            if (functionIDTable.Contains(key))
            {
                string msg;

                msg = string.Format("Funktionen {0} med signaturen {1} redan definierad", name, signature);
                throw new STLangCompilerError(msg);
            }
            else
            {
                functionIDTable[key] = functionID;
                return functionID++;
            }
        }

        public override string TypeName 
        { 
            get { return "användardefinierad funktion"; } 
        }

        private static uint functionID;

        private static Hashtable functionIDTable;
    }
}
