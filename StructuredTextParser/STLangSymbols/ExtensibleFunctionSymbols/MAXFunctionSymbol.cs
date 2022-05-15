using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Parser;
using STLang.DataTypes;
using STLang.Extensions;
using STLang.Expressions;
using STLang.ErrorManager;
using STLang.VMInstructions;
using STLang.POUDefinitions;
using STLang.ParserUtility;
using STLang.ImplDependentParams;
using QUT.Gppg;

namespace STLang.Symbols
{
    public class MAXFunctionSymbol : ExtensibleFunctionSymbol
    {
        public MAXFunctionSymbol() : base("MAX")
        {

        }

        public override Expression MakeSyntaxTreeNode(LexLocation loc)
        {
            string functionSignList = string.Empty;
            foreach (StandardLibFunctionSignature function in this.signatureList)
            {
                string functionSign;
                string dataTypeName = function.InputDataTypes[0].Name;
                functionSign = string.Format("{0}({1},{1}, ... ,{1})", this.Name, dataTypeName);
                if (functionSignList.Length > 0)
                    functionSignList += ",";
                functionSignList += functionSign;
            }
            string funcCall = this.Name + "()";
            Report.SemanticError(18, funcCall, functionSignList, loc);
            return Expression.Error;
        }

        protected override void SortArguments(List<Expression> arguments, out List<InputParameter> inputs, out List<OutputParameter> outputs)
        {
            inputs = new List<InputParameter>();
            outputs = new List<OutputParameter>();
            foreach (Expression argument in arguments)
            {
                if (argument is InputParameter)
                    inputs.Add(argument as InputParameter);
                else if (argument is OutputParameter)
                    outputs.Add(argument as OutputParameter);
            }
        }

        public override Expression MakeSyntaxTreeNode(List<Expression> arguments, LexLocation loc)
        {
            List<InputParameter> inputs;
            List<OutputParameter> outputs;
            List<MatchedFunction> conversionCost;

            this.SortArguments(arguments, out inputs, out outputs);
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
                    Expression result = this.EvaluateConstantFunctionCall(arguments, function.ReturnType);
                    Expression[] argVector = inputs.ToArray();
                    string exprString = this.MakeExpressionString(arguments);
                    return new ExtensibleFunctionCall(argVector, function.ReturnType, opCode, exprString);
                }
                else
                {
                    Expression[] argVector = arguments.ToArray();
                    StandardLibraryFunction opCode = function.FunctionCode;
                    string exprString = this.MakeExpressionString(arguments);
                    return new ExtensibleFunctionCall(argVector, function.ReturnType, opCode, exprString);
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
                    return this.EvaluateConstantFunctionCall(arguments, function.ReturnType);
                else if (conversionCost[0].Cost == 0)
                {
                    Expression[] argVector = inputs.ToArray();
                    StandardLibraryFunction opCode = function.FunctionCode;
                    return new ExtensibleFunctionCall(argVector, function.ReturnType, opCode, exprString);
                }
                else
                {
                    Expression expression;
                    List<Expression> inputList;
                    TypeNode inputType = function.ReturnType;

                    inputList = new List<Expression>();
                    foreach (InputParameter input in inputs)
                    {
                        expression = input.RValue;
                        if (inputType.ConversionCost(expression) > 0.0f)
                        {
                            if (expression.IsConstant)
                            {
                                if (expression.DataType == TypeNode.Real && inputType == TypeNode.LReal)
                                {
                                    double value = Convert.ToDouble(expression.Evaluate());
                                    expression = STLangParser.MakeConstant(value);
                                }
                                else if (expression.DataType.IsIntegerType && inputType == TypeNode.LReal)
                                {
                                    double value = Convert.ToDouble(expression.Evaluate());
                                    expression = STLangParser.MakeConstant(value);
                                }
                                else if (expression.DataType.IsIntegerType && inputType == TypeNode.Real)
                                {
                                    float value = Convert.ToSingle(expression.Evaluate());
                                    expression = STLangParser.MakeConstant(value);
                                }
                            }
                            else if (expression.DataType == TypeNode.Real && inputType == TypeNode.LReal)
                                expression = new Real2LRealOperator(expression);
                            else if (expression.DataType.IsIntegerType && inputType == TypeNode.LReal)
                            {
                                if (expression.DataType == TypeNode.LInt)
                                    expression = new LInt2LRealOperator(expression);
                                else
                                    expression = new Int2LRealOperator(expression);
                            }
                            else if (expression.DataType.IsIntegerType && inputType == TypeNode.Real)
                            {
                                if (expression.DataType == TypeNode.LInt)
                                    expression = new LInt2RealOperator(expression);
                                else
                                    expression = new Int2RealOperator(expression);
                            }
                        }
                        inputList.Add(expression);
                    }
                    Expression[] argVector = inputList.ToArray();
                    StandardLibraryFunction opCode = function.FunctionCode;
                    return new ExtensibleFunctionCall(argVector, function.ReturnType, opCode, exprString);
                }
            }
        }

        private Expression EvaluateConstantFunctionCall(List<Expression> inputs, TypeNode dataType)
        {
            if (dataType.IsSignedIntType)
            {
                List<long> intList = new List<long>();
                foreach (Expression input in inputs)
                    intList.Add(Convert.ToInt64(input.Evaluate()));
                return inputs[intList.MaxIndex()];
                
            }
            else if (dataType.IsUnsignedIntType)
            {
                List<ulong> uintList = new List<ulong>();
                foreach (Expression input in inputs)
                    uintList.Add(Convert.ToUInt64(input.Evaluate()));
                return inputs[uintList.MaxIndex()];
            }
            else if (dataType.IsBitStringType)
            {
                List<ulong> bitStringList = new List<ulong>();
                foreach (Expression input in inputs)
                    bitStringList.Add(Convert.ToUInt64(input.Evaluate()));
                return inputs[bitStringList.MaxIndex()];
            }
            else if (dataType == TypeNode.LReal)
            {
                List<double> doubleList = new List<double>();
                foreach (Expression input in inputs)
                    doubleList.Add(Convert.ToDouble(input.Evaluate()));
                return inputs[doubleList.MaxIndex()];
            }
            else if (dataType == TypeNode.Real)
            {
                List<float> floatList = new List<float>();
                foreach (Expression input in inputs)
                    floatList.Add(Convert.ToSingle(input.Evaluate()));
                return inputs[floatList.MaxIndex()];
            }
            else if (dataType == TypeNode.Time)
            {
                List<TimeSpan> timeList = new List<TimeSpan>();
                foreach (Expression input in inputs)
                    timeList.Add((TimeSpan)input.Evaluate());
                return inputs[timeList.MaxIndex()];
            }
            else if (dataType == TypeNode.Date)
            {
                List<DateTime> dateList = new List<DateTime>();
                foreach (Expression input in inputs)
                    dateList.Add(Convert.ToDateTime(input.Evaluate()));
                return inputs[dateList.MaxIndex()];
            }
            else if (dataType == TypeNode.DateAndTime)
            {
                List<DateTime> dateTimeList = new List<DateTime>();
                foreach (Expression input in inputs)
                    dateTimeList.Add(Convert.ToDateTime(input.Evaluate()));
                return inputs[dateTimeList.MaxIndex()];
            }
            else if (dataType == TypeNode.TimeOfDay)
            {
                List<TimeSpan> timeOfDayList = new List<TimeSpan>();
                foreach (Expression input in inputs)
                    timeOfDayList.Add((TimeSpan)input.Evaluate());
                return inputs[timeOfDayList.MaxIndex()];
            }
            else if (dataType.IsTextType)
            {
                List<string> stringList = new List<string>();
                foreach (Expression input in inputs)
                    stringList.Add(Convert.ToString(input.Evaluate()));
                return inputs[stringList.MaxIndex()];
            }
            else
            {
                string msg;
                msg = "EvaluateConstantFunctionCall(): Illegal parameter type ";
                throw new STLangCompilerError(msg + dataType.Name);
            }
        }
    }
}
