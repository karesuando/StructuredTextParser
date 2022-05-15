using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using STLang.Parser;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.ErrorManager;
using STLang.VMInstructions;
using STLang.POUDefinitions;
using QUT.Gppg;

namespace STLang.Symbols
{
    public class BitShiftFunctionSymbol : VirtualMachineInstructionSymbol
    {
        public BitShiftFunctionSymbol(string name)
            : base(name)
        {

        }

        public override Expression MakeSyntaxTreeNode(List<Expression> arguments, LexLocation loc)
        {
            List<MatchedFunction> conversionCost;

            conversionCost = this.GetBestMatchingOverloadedFunction(arguments);
            if (conversionCost.Count == 0)
            {
                // Too many parameters
                Report.SemanticError(6, this.Name, loc);
                return Expression.Error;
            }
            else if (conversionCost[0].Cost >= TypeNode.MAX_CONVERSION_COST)
            {
                // No matching overloaded function found
                string signature = this.GetFunctionSignature(arguments);
                Report.SemanticError(7, signature, loc);
                return Expression.Error;
            }
            else if (conversionCost.Count == 1)
            {
                int fcnIndex = conversionCost[0].Index;
                VirtualMachineFunctionSignature function = this.signatureList[fcnIndex];
                VirtualMachineInstruction opCode = function.OperationCode;
                if (this.IsConstantParameters(arguments))
                    return this.EvalConstExpression(opCode, arguments);
                else
                {
                    Expression[] argV = arguments.ToArray();
                    TypeNode returnType = function.ReturnType;
                    string exprString = this.MakeExpressionString(arguments);
                    if (argV[1].IsConstant)
                    {
                        int bits = Convert.ToInt32(argV[1].Evaluate());
                        if (bits == 1)
                        {
                            if (opCode == VirtualMachineInstruction.IBSHL)
                                opCode = VirtualMachineInstruction.IBSHL_1;
                            else if (opCode == VirtualMachineInstruction.IBSHR)
                                opCode = VirtualMachineInstruction.IBSHR_1;
                            argV = new Expression[] { argV[0] };
                            return new VirtualMachineFunctionCall(argV, returnType, opCode, exprString);
                        }
                    }
                    return new VirtualMachineFunctionCall(argV, returnType, opCode, exprString);
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
                VirtualMachineFunctionSignature function = this.signatureList[fcnIndex];
                VirtualMachineInstruction opCode = function.OperationCode;
                if (this.IsConstantParameters(arguments))
                    return this.EvalConstExpression(opCode, arguments);
                else
                {
                    Expression[] argV = arguments.ToArray();
                    TypeNode returnType = function.ReturnType;
                    string exprString = this.MakeExpressionString(arguments);
                    if (argV[1].IsConstant)
                    {
                        int bits = Convert.ToInt32(argV[1].Evaluate());
                        if (bits == 1)
                        {
                            if (opCode == VirtualMachineInstruction.IBSHL)
                                opCode = VirtualMachineInstruction.IBSHL_1;
                            else if (opCode == VirtualMachineInstruction.LBSHL)
                                opCode = VirtualMachineInstruction.LBSHL_1;
                            else if (opCode == VirtualMachineInstruction.IBSHR)
                                opCode = VirtualMachineInstruction.IBSHR_1;
                            else if (opCode == VirtualMachineInstruction.LBSHR)
                                opCode = VirtualMachineInstruction.LBSHR_1;
                            argV = new Expression[] { argV[0] };
                            return new VirtualMachineFunctionCall(argV, returnType, opCode, exprString);
                        }
                    }
                    return new VirtualMachineFunctionCall(argV, returnType, opCode, exprString);
                }
            }
        }

        private Expression MakeBitStringConstant(ulong value, TypeNode dataType)
        {
            if (dataType == TypeNode.Byte)
                return STLangParser.MakeConstant((byte)value);
            else if (dataType == TypeNode.Word)
                return STLangParser.MakeConstant((ushort)value);
            else if (dataType == TypeNode.DWord)
                return STLangParser.MakeConstant((uint)value);
            else
                return STLangParser.MakeConstant(value);
        }

        private Expression EvalConstExpression(VirtualMachineInstruction opCode, List<Expression> argList)
        {
            TypeNode dataType = argList[0].DataType;
            ulong value = Convert.ToUInt64(argList[0].Evaluate());
            int bits = Convert.ToInt32(argList[1].Evaluate());
            switch (opCode)
            {
                case VirtualMachineInstruction.IBSHL:
                    return this.MakeBitStringConstant(value << bits, dataType);

                case VirtualMachineInstruction.IBSHR:
                    return this.MakeBitStringConstant(value >> bits, dataType);

                case VirtualMachineInstruction.LBSHL:
                    return this.MakeBitStringConstant(value << bits, dataType);

                case VirtualMachineInstruction.LBSHR:
                    return this.MakeBitStringConstant(value >> bits, dataType);

                default:
                    throw new STLangCompilerError("Unknown bit-shift opcode: " + opCode);
            }

        }
    }
}
