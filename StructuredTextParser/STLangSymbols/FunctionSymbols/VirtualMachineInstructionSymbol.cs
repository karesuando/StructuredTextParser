using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Parser;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.ErrorManager;
using STLang.ParserUtility;
using STLang.VMInstructions;
using STLang.POUDefinitions;
using QUT.Gppg;

namespace STLang.Symbols
{
    public class VirtualMachineInstructionSymbol : ProgramOrganizationUnitSymbol
    {
        public VirtualMachineInstructionSymbol(string name)
           : base(name, TypeNode.Void)
        {
            this.signatureList = new List<VirtualMachineFunctionSignature>();
        }

        public override bool IsFunction
        {
            get { return true; }
        }
        public override POUType POUType
        {
            get { return POUType.FUNCTION; }
        }

        public override string TypeName
        {
            get { return "standardfunktion"; }
        }

        public override Expression MakeSyntaxTreeNode(LexLocation loc)
        {
            if (this.signatureList.Count > 1)
            {
                // Error. Ambiguous function call.

                string functionSignList = "";
                foreach (VirtualMachineFunctionSignature function in this.signatureList)
                {
                    int i = 0;
                    string functionSign = this.Name + "(";
                    foreach (TypeNode dataType in function.InputDataTypes)
                    {
                        i++;
                        if (i > 1)
                            functionSign += ",";
                        functionSign += dataType.Name;
                    }
                    functionSign += ")";
                    if (functionSignList.Length > 0)
                        functionSignList += ",";
                    functionSignList += functionSign;
                }
                string funcCall = this.Name + "()";
                Report.SemanticError(18, funcCall, functionSignList, loc);
                return Expression.Error;
            }
            else
            {
                VirtualMachineFunctionSignature function = this.signatureList[0];
                List<Expression> argList = new List<Expression>();
                VirtualMachineInstruction opCode = function.OperationCode;

                foreach (TypeNode dataType in function.InputDataTypes)
                    argList.Add(dataType.DefaultValue);
                return this.EvalConstFunctionCall(opCode, argList);
            }
        }

        protected string GetFunctionSignature(List<Expression> argList)
        {
            string exprString = this.Name + "(";
            if (argList.Count > 0)
            {
                exprString += argList[0].DataType.Name;
                for (int i = 1; i < argList.Count; i++)
                    exprString += "," + argList[i].DataType.Name;
            }
            exprString += ")";
            return exprString;
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
            List<Expression> inputs, outputs;
            List<MatchedFunction> conversionCost;

            this.SortInputArguments(arguments, out inputs, out outputs);
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
                VirtualMachineFunctionSignature function;

                function = this.signatureList[fcnIndex];
                if (this.IsConstantParameters(inputs))
                {
                    VirtualMachineInstruction opCode = function.OperationCode;
                    if (opCode == VirtualMachineInstruction.NOOP)
                    {
                        // Function not implemented
                        Report.SemanticError(77, this.Name, loc);
                        return Expression.Error;
                    }
                    else
                    {
                        Expression result = this.EvalConstFunctionCall(opCode, inputs);
                        if (result != null)
                            return result;
                        else
                        {
                            Expression[] argVector = arguments.ToArray();
                            string exprString = this.MakeExpressionString(arguments);
                            return new VirtualMachineFunctionCall(argVector, function.ReturnType, opCode, exprString);
                        }
                    }
                }
                else
                {
                    Expression[] argVector = arguments.ToArray();
                    VirtualMachineInstruction opCode = function.OperationCode;
                    string exprString = this.MakeExpressionString(arguments);
                    return new VirtualMachineFunctionCall(argVector, function.ReturnType, opCode, exprString);
                }
            }
            else if (conversionCost[0].Cost == conversionCost[1].Cost)
            {
                // Ambiguous function call
                float cost0 = conversionCost[0].Cost;
                List<MatchedFunction> sameCostList;
                sameCostList = conversionCost.FindAll(f => f.Cost == cost0);
                string functionSign = this.GetFunctionSignature(arguments);
                string signatureList = this.MakeFunctionSignatureList(sameCostList);
                Report.SemanticError(18, functionSign, signatureList, loc);
                return Expression.Error;
            }
            else
            {
                int fcnIndex = conversionCost[0].Index;
                string exprString = this.MakeExpressionString(arguments);
                VirtualMachineFunctionSignature function;
                function = this.signatureList[fcnIndex];
                if (this.IsConstantParameters(inputs))
                {
                    VirtualMachineInstruction opCode = function.OperationCode;
                    if (opCode == VirtualMachineInstruction.NOOP)
                    {
                        // Function not implemented
                        Report.SemanticError(77, this.Name, loc);
                        return Expression.Error;
                    }
                    else
                    {
                        Expression result = this.EvalConstFunctionCall(opCode, inputs);
                        if (result != null)
                            return result;
                        else
                        {
                            Expression[] argVector = arguments.ToArray();
                            return new VirtualMachineFunctionCall(argVector, function.ReturnType, opCode, exprString);
                        }
                    }
                }
                else
                {
                    Expression[] argVector = inputs.ToArray();
                    VirtualMachineInstruction opCode = function.OperationCode;
                    return new VirtualMachineFunctionCall(argVector, function.ReturnType, opCode, exprString);
                }
            }
        }

        public void Add(VirtualMachineInstruction opCode, TypeNode returnType, params TypeNode[] formalTypes)
        {
            string signature = this.GetSignature(formalTypes);
            if (this.SignatureExists(signature))
            {
                string msg = string.Format("Function {0} with signature {1} already defined.", this.Name, signature);
                throw new STLangCompilerError(msg);

            }
            else
            {
                VirtualMachineFunctionSignature function;
                function = new VirtualMachineFunctionSignature();
                function.ReturnType = returnType;
                function.InputDataTypes = formalTypes;
                function.OperationCode = opCode;
                function.Signature = signature;
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

        protected List<MatchedFunction> GetBestMatchingOverloadedFunction(List<Expression> actualArguments)
        {
            int position = 0;
            List<MatchedFunction> conversionCost = new List<MatchedFunction>();
            foreach (VirtualMachineFunctionSignature function in this.signatureList)
            {
                if (function.InputCount >= actualArguments.Count)
                {
                    float totalCost = 0.0f;
                    TypeNode[] formalType = function.InputDataTypes;
                    for (int i = 0; i < actualArguments.Count; i++)
                    {
                        totalCost += formalType[i].ConversionCost(actualArguments[i]);
                    }
                    conversionCost.Add(new MatchedFunction(totalCost, position));
                }
                position++;
            }
            conversionCost.Sort((f1, f2) => { return f1.Cost.CompareTo(f2.Cost); });
            return conversionCost;
        }

        private bool SignatureExists(string signature)
        {
            return this.signatureList.Find(f => f.Signature == signature) != null;
        }

        protected bool IsConstantParameters(List<Expression> argList)
        {
            if (argList == null)
                return false;
            else
            {
                foreach (Expression arg in argList)
                {
                    if (!arg.IsConstant)
                        return false;
                }
                return true;
            }
        }

        private Expression EvalConstFunctionCall(VirtualMachineInstruction opCode, List<Expression> argList)
        {
            Expression arg0 = argList[0];
            object result;

            result = arg0.Evaluate();
            switch (opCode)
            {
                case VirtualMachineInstruction.F2D:
                    result = (double)Convert.ToSingle(result);
                    break;
                case VirtualMachineInstruction.D2F:
                    result = (float)Convert.ToDouble(result);
                    break;

                case VirtualMachineInstruction.I2F:
                    result = (float)Convert.ToInt32(result);
                    break;

                case VirtualMachineInstruction.F2I:
                    result = (int)Convert.ToSingle(result);
                    break;

                case VirtualMachineInstruction.F2L:
                    result = (long)Convert.ToSingle(result);
                    break;

                case VirtualMachineInstruction.D2I:
                    result = (int)Convert.ToDouble(result);
                    break;

                case VirtualMachineInstruction.D2L:
                    result = (long)Convert.ToDouble(result);
                    break;

                case VirtualMachineInstruction.L2D:
                    result = (double)Convert.ToInt64(result);
                    break;

                case VirtualMachineInstruction.I2D:
                    result = (double)Convert.ToInt32(result);
                    break;

                default:
                    result = null;
                    break;
            }
            if (result == null)
                return null;
            else if (result is int)
                return STLangParser.MakeConstant((int)result);
            else if (result is long)
                return STLangParser.MakeConstant((long)result);
            else if (result is double)
                return STLangParser.MakeConstant((double)result);
            else if (result is float)
                return STLangParser.MakeConstant((float)result);
            else
                return null;
        }


        protected readonly List<VirtualMachineFunctionSignature> signatureList;
    }
}
