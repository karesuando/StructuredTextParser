using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.ErrorManager;
using STLang.VMInstructions;
using QUT.Gppg;

namespace STLang.Symbols
{
    public class TypeConverterSymbol : STLangSymbol
    {
        public TypeConverterSymbol(string name) : base(name)
        {
            this.signatureList = new List<TypeConverter>();
        }

        public override string TypeName 
        { 
            get { return "typkonverteringsfunktion"; } 
        }

        public override Expression MakeSyntaxTreeNode(LexLocation loc)
        {
            // Error: Missing parameters
            Report.SemanticError(140, this.Name, loc);
            return Expression.Error;
        }

        public override bool IsFunction
        {
            get { return true; }
        }

        private bool SignatureExists(string signature)
        {
            return this.signatureList.Find(f => f.Signature == signature) != null;
        }

        public override Expression MakeSyntaxTreeNode(List<Expression> argList, LexLocation loc)
        {
            List<MatchedTypeConverter> typeConvCandidates;

            typeConvCandidates = this.GetBestMatchingTypeConverter(argList);
            if (typeConvCandidates.Count == 0)
            {
                // Too many parameters
                Report.SemanticError(6, this.Name, loc);
                return Expression.Error;
            }
            else if (typeConvCandidates[0].Cost >= TypeNode.MAX_CONVERSION_COST)
            {
                // No matching overloaded function found
                string signature = "(" + argList[0].DataType.TypeID + ")";
                Report.SemanticError(7, signature, loc);
                return Expression.Error;
            }
            else
            {
                Expression arg = argList[0];
                int pos = typeConvCandidates[0].Number;
                TypeConverter function = this.signatureList[pos];
                VirtualMachineInstruction opCode = function.OpCode;
                if (arg.IsConstant && opCode != VirtualMachineInstruction.CONV)
                    return this.EvalConstExpression(opCode, arg);
                else
                {
                    switch (opCode)
                    {
                        case VirtualMachineInstruction.I2D:
                            return new Int2LRealOperator(arg, false);
                        case VirtualMachineInstruction.I2F:
                            return new Int2RealOperator(arg, false);
                        case VirtualMachineInstruction.F2D:
                            return new Real2LRealOperator(arg, false);
                        case VirtualMachineInstruction.D2F:
                            return new LReal2RealOperator(arg, false);
                        case VirtualMachineInstruction.D2I:
                            if (function.ToType.IsIntegerType)
                                return new LReal2IntOperator(arg, false);
                            else if (function.ToType.IsBitStringType)
                                return new LReal2DWordOperator(arg, false);
                            else
                                return new LReal2IntOperator(arg, false);
                        case VirtualMachineInstruction.F2I:
                            return new Real2IntOperator(arg, false);
                    }
                    TypeNode dataType = function.ToType;
                    string exprString = this.Name + "(" + arg.ToString() + ")";
                    return new GenericTypeConvOperator(arg, dataType, opCode, function.Code, exprString);
                }
            }
        }

        public void Add(TypeNode toType, TypeNode fromType, VirtualMachineInstruction opCode)
        {
            string signature = "(" + fromType.TypeID + ")";
            if (this.SignatureExists(signature))
            {
                string msg = string.Format("Conversion function {0} with signature {1} already declared.", this.Name, signature);
                throw new STLangCompilerError(msg);
            }
            else
            {
                int code = (toType.TypeID[0] << 8) | fromType.TypeID[0];
                TypeConverter typeConverter = new TypeConverter();
                typeConverter.ToType = toType;
                typeConverter.FromType = fromType;
                typeConverter.Signature = "(" + fromType.TypeID + ")";
                typeConverter.OpCode = opCode;
                typeConverter.Code = (ushort)code;
                this.signatureList.Add(typeConverter);
            }
        }

        private List<MatchedTypeConverter> GetBestMatchingTypeConverter(List<Expression> actualArguments)
        {
            List<MatchedTypeConverter> conversionCost = new List<MatchedTypeConverter>();
            if (actualArguments.Count == 1)
            {
                int position = 0;
                foreach (TypeConverter function in this.signatureList)
                {
                    TypeNode formalType = function.FromType;
                    float cost = formalType.ConversionCost(actualArguments[0]);
                    conversionCost.Add(new MatchedTypeConverter(cost, position));
                    position++;
                }
                conversionCost.Sort((f1, f2) => { return f1.Cost.CompareTo(f2.Cost); });
            }
            return conversionCost;
        }

        private class TypeConverter
        {
            public TypeNode ToType { get; set; }

            public TypeNode FromType { get; set; }

            public string Signature { get; set; }

            public VirtualMachineInstruction OpCode { get; set; }

            public ushort Code { get; set; }
        }

        private class MatchedTypeConverter
        {
            public MatchedTypeConverter(float cost, int index)
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

        private Expression EvalConstExpression(VirtualMachineInstruction opCode, Expression expression)
        {
            if (expression == null)
                return Expression.Error;
            else { 
                object result;
                object arg = expression.Evaluate();
                switch (opCode)
                {
                    case VirtualMachineInstruction.F2D:
                        result = (double)Convert.ToSingle(arg);
                        break;
                    case VirtualMachineInstruction.D2F:
                        result = (float)Convert.ToDouble(arg);
                        break;
                    case VirtualMachineInstruction.I2F:
                        result = (float)Convert.ToInt64(arg);
                        break;
                    case VirtualMachineInstruction.F2I:
                        result = (long)Convert.ToSingle(arg);
                        break;
                    case VirtualMachineInstruction.D2I:
                        result = (long)Convert.ToDouble(arg);
                        break;
                    case VirtualMachineInstruction.I2D:
                        result = (double)Convert.ToInt64(arg);
                        break;
                    default:
                        return expression;
                }
                if (result is double)
                    return new LRealConstant((double)result);
                else if (result is float)
                    return new RealConstant((float)result);
                else
                    return new LIntConstant((long)result);

            }
        }

        private readonly List<TypeConverter> signatureList;
    }
}
