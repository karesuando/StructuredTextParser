using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.ErrorManager;
using STLang.ParserUtility;
using STLang.POUDefinitions;
using STLang.VMInstructions;
using STLang.ImplDependentParams;
using QUT.Gppg;

namespace STLang.Symbols
{
    public abstract class ExtensibleFunctionSymbol : StandardFunctionSymbol
    {
        public ExtensibleFunctionSymbol(string name) : base(name)
        {
        }

        protected override List<MatchedFunction> GetBestMatchingOverloadedFunction(List<InputParameter> inputs)
        {
            List<MatchedFunction> conversionCost = new List<MatchedFunction>();
            if (inputs.Count <= STLangParameters.MAX_EXTENSIBLE_FUNCTION_INPUTS)
            {
                int position = 0;
                foreach (StandardLibFunctionSignature function in this.signatureList)
                {
                    float totalCost = 0.0f;
                    int fixedInputCount = function.FixedInputCount;
                    TypeNode[] inputDataTypes = function.InputDataTypes;

                    for (int i = 0; i < fixedInputCount; i++)
                    {
                        // Datatype of formal and actual input argument 
                        // must match exactly.
                        
                        totalCost += inputDataTypes[i].ConversionCost(inputs[i]);
                        if (totalCost > 0)
                        {
                            LexLocation loc = inputs[i].LexicalLocation;
                            string dataTypeName = inputDataTypes[i].Name;
                            Report.SemanticError(187, this.Name, i + 1, dataTypeName, loc);
                        }
                    }
                    TypeNode variableDataType = inputDataTypes[fixedInputCount];
                    for (int i = fixedInputCount; i < inputs.Count; i++)
                    {
                        totalCost += variableDataType.ConversionCost(inputs[i]);
                    }
                    conversionCost.Add(new MatchedFunction(totalCost, position));
                    position++;
                }
                conversionCost.Sort((f1, f2) => { return f1.Cost.CompareTo(f2.Cost); });
            }
            return conversionCost;
        }
    }
}
