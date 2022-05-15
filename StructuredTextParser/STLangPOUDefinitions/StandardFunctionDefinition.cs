using System;
using STLang.Symbols;
using STLang.DataTypes;
using STLang.Statements;
using STLang.MemoryLayout;
using STLang.VMInstructions;
using System.Collections.Generic;

namespace STLang.POUDefinitions
{
    public class VirtualMachineFunctionSignature
    {
        public TypeNode ReturnType { get; set; }

        public TypeNode[] InputDataTypes { get; set; }

        public VirtualMachineInstruction OperationCode { get; set; }

        public string Signature { get; set; }

        public string TypeID { get; set; }

        public int InputCount { get; set; }
    }

    public class StandardLibFunctionSignature : VirtualMachineFunctionSignature
    {
        public StandardLibFunctionSignature()
        {
            this.OperationCode = VirtualMachineInstruction.CALL;
        }

        public int FixedInputCount { get; set; }

        public StandardLibraryFunction FunctionCode { get; set; }
    }
}
