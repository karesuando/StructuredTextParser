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
using StructuredTextParser.Properties;

namespace STLang.Statements
{
    public class ArrayVarDeclStatement : VarDeclStatement
    {
        public ArrayVarDeclStatement(List<MemoryObject> variables, TypeNode dataType, 
                                     STDeclQualifier declQual, Expression initValue,
                                     Expression size, int initValCase, double ratio = 1.0)
            : base(variables, dataType, declQual, initValue, size)
        {
            this.constRatio = ratio;
            this.initialValueCase = initValCase;
            this.assignStatList = new List<AssignmentStat>();
        }

        public ArrayVarDeclStatement(List<MemoryObject> variables, TypeNode dataType,
                                     STDeclQualifier declQual, Expression initValue,
                                     Expression size, int initValCase, double ratio, List<AssignmentStat> assignStats)
            : base(variables, dataType, declQual, initValue, size)
        {
            this.constRatio = ratio;
            this.assignStatList = assignStats;
            this.initialValueCase = initValCase;
        }

        public override void SetDeclarationInitData(RWMemoryLayoutManager memLayOutManager)
        {
            foreach (MemoryObject variable in this.VariableList)
            {
                memLayOutManager.SetAbsoluteAddress(variable.Symbol);
            }
        }

        public override void GenerateCode(List<int> exitList = null)
        {
            if (this.VariableList == null || this.InitialValue == null)
                throw new STLangCompilerError(Resources.ARRAYVARDECL);
            else
            {
                MemoryObject var0 = this.VariableList.ElementAt(0);
                switch (this.initialValueCase)
                {
                    case 0:
                        //  Default array initializer {0}. Set memory block to 0.
                        this.Size.GenerateLoad();
                        var0.Location.AbsoluteAddress.GenerateLoad();
                        this.StoreInstruction(VirtualMachineInstruction.MEM_SETZ);
                        break;

                    case 1:
                        {
                            ArrayType array = (ArrayType)this.DataType;
                            TypeNode basicElementType = array.BasicElementType;
                            int size = (int)basicElementType.Size;
                            basicElementType.DefaultValue.GenerateLoad();
                            var0.Location.AbsoluteAddress.GenerateLoad();
                            this.Size.GenerateLoad();
                            this.StoreInstruction(VirtualMachineInstruction.MEM_SETB, size);
                        }
                        break;

                    case 2:
                        //
                        // Array declaration with initializer list.
                        //
                        if (this.constRatio < 0.7)
                            this.LoadArrayInitializer();
                        else {
                            ArrayInitializer arrayInit;
                            arrayInit = (ArrayInitializer)this.InitialValue;
                            foreach (Expression var in this.VariableList)
                            {
                                var.Location.AbsoluteAddress.GenerateLoad();
                                arrayInit.AbsoluteAddress.GenerateLoad();
                                this.Size.GenerateLoad();
                                this.StoreInstruction(VirtualMachineInstruction.ROM_COPY);
                            }
                            foreach (AssignmentStat assignStat in this.assignStatList)
                                assignStat.GenerateCode();
                        }
                        break;

                    case 3:
                        //
                        // Constant array declaration with initializer list.
                        // Initialize constant part of the array at POU
                        // object creation time.
                        // 
                        if (this.constRatio < 0.7)
                            this.LoadArrayInitializer();
                        else {
                            int srcAddr, dstAddr;
                            Expression absAddress;
                            ArrayInitializer arrayInit;
                            arrayInit = (ArrayInitializer)this.InitialValue;
                            absAddress = arrayInit.AbsoluteAddress;
                            srcAddr = Convert.ToInt32(absAddress.Evaluate());
                            int byteCount = Convert.ToInt32(this.Size.Evaluate());
                            foreach (Expression variable in this.VariableList)
                            {
                                absAddress = variable.Location.AbsoluteAddress;
                                dstAddr = Convert.ToInt32(absAddress.Evaluate());
                                this.StoreInitializerData(srcAddr, dstAddr, byteCount);
                            }
                            foreach (AssignmentStat assignStat in this.assignStatList)
                                assignStat.GenerateCode();
                        }
                        break;

                    case 4:
                        break;
                }
            }
        }

        private void LoadArrayInitializer()
        {
            // Evaluate each initializer in the list and push the 
            // result in reverse order on the stack. The stack is 
            // then copied to memory.

            ArrayInitializer arrayInit;
            IEnumerable<Expression> initializerList;
            arrayInit = (ArrayInitializer)this.InitialValue;
            initializerList = arrayInit.FlattenedInitializerList;

            initializerList.Reverse();
            MemoryObject var0 = this.VariableList.ElementAt(0);
            foreach (Expression initialValue in initializerList)
                initialValue.GenerateLoad();
            int elemSize = (int)arrayInit.BasicElementType.Size;
            int parameter = elemSize | (this.VariableList.Count() << 4);
            this.Size.GenerateLoad();
            var0.Location.AbsoluteAddress.GenerateLoad();
            this.StoreInstruction(VirtualMachineInstruction.COPY_STACK, parameter);
        }

        private readonly double constRatio;

        private readonly int initialValueCase;

        private readonly List<AssignmentStat> assignStatList; // Non-constant initializers
    }
}
