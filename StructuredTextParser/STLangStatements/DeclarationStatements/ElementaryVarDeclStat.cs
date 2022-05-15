using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.MemoryLayout;
using STLang.ErrorManager;
using STLang.VMInstructions;
using LarmiaControl.Evo.Core.StructuredText.Properties;

namespace STLang.Statements
{
    public class ElementaryVarDeclStatement : VarDeclStatement
    {
        public ElementaryVarDeclStatement(List<MemoryObject> variables, TypeNode dataType, STDeclQualifier declQual, Expression initVal, Expression declSize) :
            base(variables, dataType, declQual, initVal, declSize)
        {
        }

        public override void SetDeclarationInitData(RWMemoryLayoutManager memLayOutManager)
        {
        }

        public override void GenerateCode(List<int> exitList)
        {
            Expression initialValue = this.InitialValue;
            if (this.VariableList == null || initialValue == null)
                throw new STLangCompilerError(Resources.ELEMVARDECL);
            else if (initialValue.IsSmallConstant)
            {
                foreach (Expression variable in this.VariableList)
                {
                    initialValue.GenerateLoad();
                    variable.GenerateStore();
                }
            }
            else {
                initialValue.GenerateLoad();
                int varCount = this.VariableList.Count();
                foreach (Expression variable in this.VariableList)
                {
                    if (varCount > 1)
                    {
                        if (this.DataType.IsTextType)
                            initialValue.GenerateLoad();
                        else if (this.DataType == TypeNode.LReal)
                            this.StoreInstruction(VirtualMachineInstruction.DDUPL);
                        else if (this.DataType == TypeNode.Real)
                            this.StoreInstruction(VirtualMachineInstruction.FDUPL);
                        else if (this.DataType.IsOrdinalType)
                        {
                            if (this.DataType.Size < TypeNode.LInt.Size)
                                this.StoreInstruction(VirtualMachineInstruction.IDUPL);
                            else
                                this.StoreInstruction(VirtualMachineInstruction.LDUPL);
                        }
                    }
                    variable.GenerateStore();
                    varCount--;
                }
            }
        }
    }
}
