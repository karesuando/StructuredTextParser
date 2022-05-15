using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Parser;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.ErrorManager;
using STLang.VMInstructions;
using STLang.POUDefinitions;
using STLang.ParserUtility;
using QUT.Gppg;

namespace STLang.Symbols
{
    public class StandardFunctionSymbol : ProgramOrganizationUnitSymbol
    {
        public StandardFunctionSymbol(string name)
           : base(name, TypeNode.Void)
        {
            this.signatureList = new List<StandardLibFunctionSignature>();
        }

        public override bool IsFunction 
        { 
            get { return true; } 
        }

        public override string TypeName 
        { 
            get { return "standardfunktion"; } 
        }

        public override Expression MakeSyntaxTreeNode(LexLocation loc)
        {
            Report.SemanticError(5, this.TypeName, this.Name, loc);
            return new StandardFunctionCall(new Expression[0], TypeNode.Error, StandardLibraryFunction.NONE, this.Name);
        }

        protected string GetFunctionSignature(List<InputParameter> inputs)
        {
            string exprString = this.Name + "(";
            if (inputs.Count > 0)
            {
                exprString += inputs[0];
                for (int i = 1; i < inputs.Count; i++)
                    exprString += "," + inputs[i];
            }
            exprString += ")";
            return exprString;
        }

        protected bool ErrorInputTypeExists(List<InputParameter> inputs)
        {
            foreach (InputParameter input in inputs)
            {
                if (input.DataType == TypeNode.Error)
                    return true;
            }
            return false;
        }

        protected string MakeFunctionSignatureList(List<MatchedFunction> list)
        {
            int i;
            string typeList;
            string funcSignList = "";
            TypeNode[] formalTypeList;
            foreach (MatchedFunction function in list)
            {
                typeList = "";
                i = function.Index;
                if (funcSignList.Length > 0)
                    funcSignList += ",";
                funcSignList += this.Name + "(";
                formalTypeList = this.signatureList[i].InputDataTypes;
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

        protected string MakeExpressionString(List<Expression> arguments)
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

        protected virtual void SortArguments(List<Expression> arguments, out List<InputParameter> inputs, out List<OutputParameter> outputs)
        {
            List<InputParameter> inputs2;
            inputs2 = new List<InputParameter>();
            outputs = new List<OutputParameter>();
            foreach (Expression argument in arguments)
            {
                if (argument is InputParameter)
                    inputs2.Add(argument as InputParameter);
                else if (argument is OutputParameter)
                    outputs.Add(argument as OutputParameter);
            }
            inputs = (from input in inputs2
                      orderby input.Position
                      select input).ToList();
        }

        public override Expression MakeSyntaxTreeNode(List<Expression> arguments, LexLocation loc)
        {
            List<InputParameter> inputs;
            List<OutputParameter> outputs;
            List<MatchedFunction> conversionCost;

            this.SortArguments(arguments, out inputs, out outputs);
            if (this.ErrorInputTypeExists(inputs)) 
                return Expression.Error; 
            conversionCost = this.GetBestMatchingOverloadedFunction(inputs);
            if (conversionCost.Count == 0)
            {
                // Too many parameters
                Report.SemanticError(6, this.Name, loc);
                return Expression.Error;
            }
            else if (conversionCost[0].Cost >= TypeNode.MAX_CONVERSION_COST)
            {
                // No matching overloaded function found
                string signature = this.GetFunctionSignature(inputs);
                Report.SemanticError(7, signature, loc);
                return Expression.Error;
            }
            else if (conversionCost.Count == 1)
            {
                int fcnIndex = conversionCost[0].Index;
                StandardLibFunctionSignature function = this.signatureList[fcnIndex];
                if (this.IsConstantInputParameters(inputs))
                {
                    StandardLibraryFunction opCode = function.FunctionCode;
                    if (opCode == StandardLibraryFunction.NONE)
                    {
                        // Function not implemented
                        Report.SemanticError(77, this.Name, loc);
                        return Expression.Error;
                    }
                    else
                    {
                        Expression result = this.EvaluateConstantFunctionCall(opCode, inputs);
                        if (result != null)
                            return result;
                        else
                        {
                            Expression[] argVector = arguments.ToArray();
                            string exprString = this.MakeExpressionString(arguments);
                            return new StandardFunctionCall(argVector, function.ReturnType, opCode, exprString);
                        }
                    }
                }
                else
                {
                    Expression[] argVector = arguments.ToArray();
                    StandardLibraryFunction opCode = function.FunctionCode;
                    string exprString = this.MakeExpressionString(arguments);
                    return new StandardFunctionCall(argVector, function.ReturnType, opCode, exprString);
                }
            }
            else if (conversionCost[0].Cost == conversionCost[1].Cost)
            {
                // Ambiguous function call
                float cost0 = conversionCost[0].Cost;
                List<MatchedFunction> sameCostList;
                sameCostList = conversionCost.FindAll(f => f.Cost == cost0);
                string functionSign = this.GetFunctionSignature(inputs);
                string signatureList = this.MakeFunctionSignatureList(sameCostList);
                Report.SemanticError(18, functionSign, signatureList, loc);
                return Expression.Error;
            }
            else
            {
                int fcnIndex = conversionCost[0].Index;
                string exprString = this.MakeExpressionString(arguments);
                StandardLibFunctionSignature function = this.signatureList[fcnIndex];
                if (this.IsConstantInputParameters(inputs))
                {
                    StandardLibraryFunction opCode = function.FunctionCode;
                    if (opCode == StandardLibraryFunction.NONE)
                    {
                        // Function not implemented
                        Report.SemanticError(77, this.Name, loc);
                        return Expression.Error;
                    }
                    else
                    {
                        Expression result = this.EvaluateConstantFunctionCall(opCode, inputs);
                        if (result != null)
                            return result;
                        else
                        {
                            Expression[] argVector = arguments.ToArray();
                            return new StandardFunctionCall(argVector, function.ReturnType, opCode, exprString);
                        }
                    }
                }
                else
                {
                    Expression[] argVector = inputs.ToArray();
                    StandardLibraryFunction opCode = function.FunctionCode;
                    return new StandardFunctionCall(argVector, function.ReturnType, opCode, exprString);
                }
            }
        }

        public void Add(StandardLibraryFunction opCode, TypeNode returnType, int fixedInputTypeCount, params TypeNode[] formalTypes)
        {
            string signature = this.GetSignature(formalTypes);
            if (this.SignatureExists(signature))
            {
                string msg = string.Format("Function {0} with signature {1} already defined.", this.Name, signature);
                throw new STLangCompilerError(msg);

            }
            else
            {
                StandardLibFunctionSignature function;
                function = new StandardLibFunctionSignature();
                function.ReturnType = returnType;
                function.InputDataTypes = formalTypes;
                function.FunctionCode = opCode;
                function.Signature = signature;
                function.FixedInputCount = fixedInputTypeCount;
                function.TypeID = signature + "->" + returnType.TypeID;
                function.InputCount = formalTypes.Length;
                this.signatureList.Add(function);
            }
        }

        protected class MatchedFunction
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

            public int Index 
            { 
                get { return this.number; } 
            }

            private readonly float cost;

            private readonly int number;
        }

        protected string GetSignature(params TypeNode[] paramTypes)
        {
            int count = 0;
            string signature = "";

            foreach (TypeNode formalType in paramTypes)
            {
                if (count > 0)
                    signature += ",";
                signature += formalType.TypeID;
                count++;
            }
            return "(" + signature + ")";
        }

        protected virtual List<MatchedFunction> GetBestMatchingOverloadedFunction(List<InputParameter> inputs)
        {
            int position = 0;
            List<MatchedFunction> conversionCost = new List<MatchedFunction>();
            foreach (StandardLibFunctionSignature function in this.signatureList)
            {
                if (function.InputCount >= inputs.Count)
                {
                    float totalCost = 0.0f;
                    int fixedInputCount = function.FixedInputCount;
                    TypeNode[] inputDataTypes = function.InputDataTypes;

                    for (int i = 0; i < fixedInputCount; i++)
                    {
                        // Fixed input datatypes must match exactly
                        //
                        totalCost += inputDataTypes[i].ConversionCost(inputs[i]);
                        if (totalCost > 0)
                        {
                            LexLocation loc = inputs[i].LexicalLocation;
                            string dataTypeName = inputDataTypes[i].Name;
                            Report.SemanticError(187, this.Name, i + 1, dataTypeName, loc);
                        }
                    }
                    for (int i = fixedInputCount; i < inputs.Count; i++)
                    {
                        totalCost += inputDataTypes[i].ConversionCost(inputs[i]);
                    }
                    conversionCost.Add(new MatchedFunction(totalCost, position));
                }
                position++;
            }
            conversionCost.Sort((f1, f2) => { return f1.Cost.CompareTo(f2.Cost); });
            return conversionCost;
        }

        protected bool SignatureExists(string signature)
        {
            return this.signatureList.Find(f => f.Signature == signature) != null;
        }

        protected bool IsConstantInputParameters(List<InputParameter> inputs)
        {
            if (inputs == null)
                return false;
            else
            {
                foreach (InputParameter arg in inputs)
                {
                    if (!arg.IsConstant)
                        return false;
                }
                return true;
            }
        }

        private Expression EvaluateConstantFunctionCall(StandardLibraryFunction opCode, List<InputParameter> inputs)
        {
            Expression arg0 = inputs[0];
            object result;

            result = arg0.Evaluate();
            switch (opCode)
            {
                case StandardLibraryFunction.DSIN:
                    result = Math.Sin(Convert.ToDouble(result));
                    break;

                case StandardLibraryFunction.FSIN:
                    result = Convert.ToSingle(Math.Sin(Convert.ToDouble(result)));
                    break;

                case StandardLibraryFunction.DCOS:
                    result = Math.Cos(Convert.ToDouble(result));
                    break;

                case StandardLibraryFunction.FCOS:
                    result = Convert.ToSingle(Math.Cos(Convert.ToDouble(result)));
                    break;

                case StandardLibraryFunction.DTAN:
                    result = Math.Tan(Convert.ToDouble(result));
                    break;

                case StandardLibraryFunction.FTAN:
                    result = Convert.ToSingle(Math.Tan(Convert.ToDouble(result)));
                    break;

                case StandardLibraryFunction.DLOGN:
                    result = Math.Log(Convert.ToDouble(result));
                    break;

                case StandardLibraryFunction.FLOGN:
                    result = Convert.ToSingle(Math.Log(Convert.ToDouble(result)));
                    break;

                case StandardLibraryFunction.DLOG10:
                    result = Math.Log10(Convert.ToDouble(result));
                    break;

                case StandardLibraryFunction.FLOG10:
                    result = Convert.ToSingle(Math.Log10(Convert.ToDouble(result)));
                    break;

                case StandardLibraryFunction.DEXP:
                    result = Math.Exp(Convert.ToDouble(result));
                    break;

                case StandardLibraryFunction.FEXP:
                    result = Convert.ToSingle(Math.Exp(Convert.ToDouble(result)));
                    break;

                case StandardLibraryFunction.DASIN:
                    result = Math.Asin(Convert.ToDouble(result));
                    break;

                case StandardLibraryFunction.FASIN:
                    result = Convert.ToSingle(Math.Asin(Convert.ToDouble(result)));
                    break;

                case StandardLibraryFunction.DACOS:
                    result = Math.Acos(Convert.ToDouble(result));
                    break;

                case StandardLibraryFunction.FACOS:
                    result = Convert.ToSingle(Math.Acos(Convert.ToDouble(result)));
                    break;

                case StandardLibraryFunction.DATAN:
                    result = Math.Atan(Convert.ToDouble(result));
                    break;

                case StandardLibraryFunction.FATAN:
                    result = Convert.ToSingle(Math.Atan(Convert.ToDouble(result)));
                    break;

                case StandardLibraryFunction.DSQRT:
                    result = Math.Sqrt(Convert.ToDouble(result));
                    break;

                case StandardLibraryFunction.FSQRT:
                    result = Convert.ToSingle(Math.Sqrt(Convert.ToDouble(result)));
                    break;

                default:
                    result = null;
                    break;
            }
            if (result == null)
                return null;
            else if (result is double)
                return STLangParser.MakeConstant((double)result);
            else if (result is float)
                return STLangParser.MakeConstant((float)result);
            else
                return Expression.Error;
        }

        protected readonly List<StandardLibFunctionSignature> signatureList;
    }
}
