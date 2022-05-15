using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;
using QUT.Gppg;
using STLang.POUDefinitions;
using STLang.ParserUtility;
using STLang.DataTypes;

namespace STLang.Symbols
{
    public class MUXFunctionSymbol : ExtensibleFunctionSymbol
    {
        public MUXFunctionSymbol() : base("MUX")
        {

        }

        public override Expression MakeSyntaxTreeNode(LexLocation loc)
        {
            Report.SemanticError(190, "", loc);
            return Expression.Error;
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
                if (inputs[0].IsConstant)
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
                        int index = Convert.ToInt32(inputs[0].Evaluate());
                        if (index < 0 || index > arguments.Count - 2)
                            return Expression.Error;
                        else
                        {
                            Expression[] argVector = arguments.ToArray();
                            string exprString = this.MakeExpressionString(arguments);
                            return new MUXFunctionCall(argVector, function.ReturnType, exprString);
                        }
                    }
                }
                else
                {
                    Expression[] argVector = arguments.ToArray();
                    StandardLibraryFunction opCode = function.FunctionCode;
                    string exprString = this.MakeExpressionString(arguments);
                    return new MUXFunctionCall(argVector, function.ReturnType, exprString);
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
                if (inputs[0].IsConstant)
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
                        int index = Convert.ToInt32(inputs[0].Evaluate());
                        if (index < 0 || index > arguments.Count - 2)
                            return Expression.Error;
                        else
                        {
                            Expression[] argVector = arguments.ToArray();
                            return new MUXFunctionCall(argVector, function.ReturnType, exprString);
                        }
                    }
                }
                else
                {
                    Expression[] argVector = inputs.ToArray();
                    StandardLibraryFunction opCode = function.FunctionCode;
                    return new MUXFunctionCall(argVector, function.ReturnType, exprString);
                }
            }
        }
    }
}
