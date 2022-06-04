using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using STLang.Symbols;
using STLang.RuntimeWrapper;
using STLang.Extensions;
using STLang.Expressions;
using STLang.DisAssemble;
using STLang.ErrorManager;
using STLang.MemoryLayout;
using STLang.VMInstructions;
using STLang.ConstantPoolHandler;

namespace STLang.ByteCodegenerator
{
    public abstract class ByteCodeGenerator
    {
        static ByteCodeGenerator()
        {
            byteCode = new List<uint>();
            readOnlyData = new List<byte>();
            inputParameters = new List<IOParameter>();
            outputParameters = new List<IOParameter>();
            retainedVariables = new List<IOParameter>();
            rwDataSegInitData = new List<InitializerData>();
            externalSymbols = new List<ExternalPOUSymbol>();
            stringVariableData = new List<StringVariableData>();
            fileHeader.MagicNumber = 0xabadface;
            fileHeader.MinorVersion = 0;
            fileHeader.MajorVersion = 1;
            d0 = double.NaN;
            d1 = double.NaN;
            d2 = double.NaN;
            d3 = double.NaN;
        }

        public static void StorePOUName(string pouName)
        {
            fileHeader.POUName = pouName;
        }

        public static int StoreByteArray(byte[] bytes, int alignment)
        {
            int remainder = readOnlyData.Count % alignment;
            if (remainder != 0)
            {
                int padding = alignment - remainder;
                for (int i = padding; i > 0; i--)
                    readOnlyData.Add(0x00);
            }
            int offset = readOnlyData.Count;
            readOnlyData.AddRange(bytes);
            return offset;
        }

        protected void StoreByteArrayAtOffset(byte[] bytes, int offset)
        {
            if (offset + bytes.Count() > readOnlyData.Count)
                throw new STLangCompilerError("StoreByteArrayAtOffset(" + offset + ") failed.");
            else
            {
                foreach (byte aByte in bytes)
                    readOnlyData[offset++] = aByte;
            }
        }

        public static void StoreConstantTable(Dictionary<string, Expression> constantTable)
        {
            ConstantPool constantCollection = new ConstantPool(constantTable);
            fileHeader.ConstantTableOffset = StoreByteArray(constantCollection.Bytes, 8);
            LRealConstant.GetRegisterConstants(ref d0, ref d1, ref d2, ref d3);
        }

        protected void StoreInitializerData(int srcAddr, int dstAddr, int byteCount)
        {
            InitializerData initData = new InitializerData();
            initData.SrcAddress = srcAddr;
            initData.DstAddress = dstAddr;
            initData.ByteCount = byteCount;
            rwDataSegInitData.Add(initData);
        }

        public static void StoreStringVariableData(int bufferOffset, int index, int size, int stringType, int elementCount = 1)
        {
            StringVariableData stringVariable = new StringVariableData();
            stringVariable.BufferOffset = bufferOffset;
            stringVariable.StringIndex = index;
            stringVariable.ElementCount = elementCount;
            stringVariable.BufferSize = size;
            stringVariable.StringType = stringType;
            stringVariableData.Add(stringVariable);
        }

        public static void StoreInputParameter(string name, int index)
        {
            IOParameter input = new IOParameter();
            input.Name = name;
            input.Index = index;
            inputParameters.Add(input);
        }

        public static void StoreOutputParameter(string name, int index)
        {
            IOParameter output = new IOParameter();
            output.Name = name;
            output.Index = index;
            outputParameters.Add(output);
        }

        public static void StoreRetainedVariable(string name, int index)
        {
            IOParameter retainedVar = new IOParameter();
            retainedVar.Name = name;
            retainedVar.Index = index;
            retainedVariables.Add(retainedVar);
        }

        protected void StoreExternalPOUSymbol(string name, int pouID)
        {
            Predicate<ExternalPOUSymbol> identicalID;
            identicalID = fnc => fnc.PouID == pouID;
            if (!externalSymbols.Exists(identicalID))
            {
                ExternalPOUSymbol pou = new ExternalPOUSymbol();
                pou.Name = name;
                pou.PouID = pouID;
                pou.Type = POUType.FUNCTION;
                pou.LRealBase = -1;
                pou.RealBase = -1;
                pou.SIntBase = -1;
                pou.IntBase = -1;
                pou.DIntBase = -1;
                pou.LIntBase = -1;
                pou.StringBase = -1;
                pou.WStringBase = -1;
                externalSymbols.Add(pou);
            }
        }

        protected void StoreExternalPOUSymbol(string name, int pouID, FramePointer fp)
        {
            Predicate<ExternalPOUSymbol> identicalID;
            identicalID = fncnBlk => fncnBlk.PouID == pouID;
            if (!externalSymbols.Exists(identicalID))
            {
                ExternalPOUSymbol pou = new ExternalPOUSymbol();
                pou.Name = name;
                pou.PouID = pouID;
                pou.Type = POUType.FUNCTION_BLOCK;
                pou.LRealBase = fp.LRealBase;
                pou.RealBase = fp.RealBase;
                pou.SIntBase = fp.SIntBase;
                pou.IntBase = fp.IntBase;
                pou.DIntBase = fp.DIntBase;
                pou.LIntBase = fp.LIntBase;
                pou.StringBase = fp.StringBase;
                pou.WStringBase = fp.WStringBase;
                externalSymbols.Add(pou);
            }
        }
       
        public static void StoreIOParameterCount() 
        {
            fileHeader.InputCount = inputParameters.Count;
            fileHeader.OutputCount = outputParameters.Count;
        }

        public static void StoreRWDataSegmentInfo(RWMemoryLayoutManager rwDataLayout)
        {
            fileHeader.LRealVarCount = rwDataLayout.LRealCount;
            fileHeader.RealVarCount = rwDataLayout.RealCount;
            fileHeader.IntVarCount = rwDataLayout.IntCount;
            fileHeader.DIntVarCount = rwDataLayout.DIntCount;
            fileHeader.LIntVarCount = rwDataLayout.LIntCount;
            fileHeader.StringVarCount = rwDataLayout.StringCount;
            fileHeader.WStringVarCount = rwDataLayout.WStringCount;
            fileHeader.StringDataCount = stringVariableData.Count;
            fileHeader.RWDataSegmentSize = rwDataLayout.RWDataSegmentSize;
        }

        public static void StoreRODataSegmentInfo(ROMemoryLayoutManager roDataLayout)
        {
            fileHeader.RODataSegmentSize = readOnlyData.Count;
            fileHeader.CodeSegmentSize = byteCode.Count;
            fileHeader.LRealConstCount = roDataLayout.LRealCount;
            fileHeader.RealConstCount = roDataLayout.RealCount;
            fileHeader.DIntConstCount = roDataLayout.DIntCount;
            fileHeader.StringCount = roDataLayout.StringCount;
            fileHeader.WStringCount = roDataLayout.WStringCount;
            fileHeader.InitializerCount = rwDataSegInitData.Count;
            fileHeader.ExternalCount = externalSymbols.Count;
        } 

        protected void StoreInstruction(VirtualMachineInstruction opCode)
        {
            byteCode.Add(((uint)opCode << 24));
        }

        protected void StoreInstruction(StandardLibraryFunction stdFunction)
        {
            uint instruction = (uint)VirtualMachineInstruction.CALL << 24;
            uint operand = (uint)stdFunction;
            instruction |= operand;
            byteCode.Add(instruction);
        }

        protected void StoreInstruction(VirtualMachineInstruction opCode, int operand)
        {
            uint instruction = (((uint)opCode << 24) | (uint)operand);
            byteCode.Add(instruction);
        }

        protected void StoreInstruction(VirtualMachineInstruction opCode, short operand)
        {
            uint instruction = (((uint)opCode << 24) | (ushort)operand);
            byteCode.Add(instruction);
        }

        public static void StorePOUType(POUType type)
        {
            fileHeader.POUType = type;
        }

        // BackPatch(int pc, uint jumpTarget)
        //
        // Fills in the target address of an (un)conditional jump instruction.
        // at location pc.
        //
        protected void BackPatch(int pc, int jumpTarget)
        {
            if (pc >= 0 && pc < ILC)
                byteCode[pc] |= (uint)jumpTarget;
            else
                throw new STLangCompilerError("BackPatch(): pc out of range");
        }

        // BackPatch(List<int> jumpInstructions, uint jumpTarget)
        //
        // Fills in the target address of a list of (un)conditional jump instructions.
        //
        protected void BackPatch(List<int> jumpInstructions, uint jumpTarget)
        {
            foreach (int pc in jumpInstructions)
            {
                if (pc >= 0 && pc < ILC)
                    byteCode[pc] |= jumpTarget;
                else
                {
                    string msg;
                    msg = string.Format("BackPatch(): pc={0} out of range.", pc);
                    throw new STLangCompilerError(msg);
                }
            }
            jumpInstructions.Clear();
        }

        // byte[] ToByteArray<T>(T structure)
        //
        // Converts a structure to an array of bytes.
        //
        protected static byte[] ToByteArray<T>(T structure)
        {
            int size = Marshal.SizeOf(structure);
            byte[] bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(structure, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        // byte[] ToByteArray<T>(T[] array)
        //
        // Converts an array of structures to an array of bytes.
        //
        protected static byte[] ToByteArray<T>(T[] array)
        {
            List<byte[]> byteArrayList = new List<byte[]>();
            foreach (T structure in array)
            {
                byteArrayList.Add(ToByteArray(structure));
            }
            return byteArrayList.GetBytes();
        }

        protected static byte[] ToByteArray<T>(List<T> list)
        {
            return ToByteArray(list.ToArray());
        }

        private static byte[] DoublesToArray()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(d0.GetBytes());
            bytes.AddRange(d1.GetBytes());
            bytes.AddRange(d2.GetBytes());
            bytes.AddRange(d3.GetBytes());
            return bytes.ToArray();
        }

        // Instruction Location Counter
        //
        protected static int ILC
        { 
            get { return byteCode.Count; } 
        }

        protected int AllocateMemory(int size, int alignment)
        {
            int dlc = readOnlyData.Count;
            int result = dlc % alignment;
            if (result != 0)
            {
                int padding = alignment - result;

                for (int i = 0; i < padding; i++)
                    readOnlyData.Add(0x00);
                dlc += padding; 
            }
            readOnlyData.AddRange(new byte[size]);
            return dlc;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct ExternalPOUSymbol
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 35)]
            public string Name;

            [MarshalAs(UnmanagedType.U4)]
            public POUType Type;

            [MarshalAs(UnmanagedType.I4)]
            public int PouID;

            [MarshalAs(UnmanagedType.I4)]
            public int RealBase;

            [MarshalAs(UnmanagedType.I4)]
            public int LRealBase; 

            [MarshalAs(UnmanagedType.I4)]
            public int DIntBase;

            [MarshalAs(UnmanagedType.I4)]
            public int LIntBase;

            [MarshalAs(UnmanagedType.I4)]
            public int SIntBase;

            [MarshalAs(UnmanagedType.I4)]
            public int IntBase;

            [MarshalAs(UnmanagedType.I4)]
            public int StringBase;

            [MarshalAs(UnmanagedType.I4)]
            public int WStringBase;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct InitializerData
        {
            [MarshalAs(UnmanagedType.I4)]
            public int SrcAddress;

            [MarshalAs(UnmanagedType.I4)]
            public int DstAddress;

            [MarshalAs(UnmanagedType.I4)]
            public int ByteCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct StringVariableData
        {
            [MarshalAs(UnmanagedType.I4)]
            public int BufferOffset;

            [MarshalAs(UnmanagedType.I4)]
            public int StringIndex;

            [MarshalAs(UnmanagedType.I4)]
            public int BufferSize;

            [MarshalAs(UnmanagedType.I4)]
            public int ElementCount;

            [MarshalAs(UnmanagedType.I4)]
            public int StringType;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct IOParameter
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string Name;

            [MarshalAs(UnmanagedType.I4)]
            public int Index;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct ExecutableFileHeader
        {
            [MarshalAs(UnmanagedType.U4)]
            public uint MagicNumber;

            [MarshalAs(UnmanagedType.U4)]
            public uint MinorVersion;

            [MarshalAs(UnmanagedType.U4)]
            public uint MajorVersion;

            [MarshalAs(UnmanagedType.U4)]
            public POUType POUType;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string POUName;

            [MarshalAs(UnmanagedType.I4)]
            public int RODataSegmentSize;

            [MarshalAs(UnmanagedType.I4)]
            public int CodeSegmentSize;

            [MarshalAs(UnmanagedType.I4)]
            public int ConstantTableOffset;

            [MarshalAs(UnmanagedType.I4)]
            public int RWDataSegmentSize;

            [MarshalAs(UnmanagedType.I4)]
            public int ExternalCount;

            [MarshalAs(UnmanagedType.I4)]
            public int InputCount;

            [MarshalAs(UnmanagedType.I4)]
            public int OutputCount;

            [MarshalAs(UnmanagedType.I4)]
            public int RetainedCount;

            [MarshalAs(UnmanagedType.I4)]
            public int InitializerCount;

            [MarshalAs(UnmanagedType.I4)]
            public int StringDataCount;

            [MarshalAs(UnmanagedType.I4)]
            public int LRealVarCount;

            [MarshalAs(UnmanagedType.I4)]
            public int RealVarCount;

            [MarshalAs(UnmanagedType.I4)]
            public int LIntVarCount;

            [MarshalAs(UnmanagedType.I4)]
            public int DIntVarCount;

            [MarshalAs(UnmanagedType.I4)]
            public int IntVarCount;

            [MarshalAs(UnmanagedType.I4)]
            public int StringVarCount;

            [MarshalAs(UnmanagedType.I4)]
            public int WStringVarCount;

            [MarshalAs(UnmanagedType.I4)]
            public int LRealConstCount;

            [MarshalAs(UnmanagedType.I4)]
            public int RealConstCount;

            [MarshalAs(UnmanagedType.I4)]
            public int LIntConstCount;

            [MarshalAs(UnmanagedType.I4)]
            public int DIntConstCount;

            [MarshalAs(UnmanagedType.I4)]
            public int StringCount;

            [MarshalAs(UnmanagedType.I4)]
            public int WStringCount;
        }

        public static void CreateExecutable(string path, string outputFile)
        {
            if (!path.EndsWith("\\"))
                path += "\\";
            string executableFile = path + outputFile;

            if (File.Exists(executableFile))
                File.Delete(executableFile);
            using (FileStream fileStream = new FileStream(executableFile, FileMode.CreateNew))
            {
                using (BinaryWriter executable = new BinaryWriter(fileStream))
                {
                    byte[] header = ToByteArray(fileHeader);
                    byte[] inputs = ToByteArray(inputParameters);
                    byte[] outputs = ToByteArray(outputParameters);
                    byte[] retained = ToByteArray(retainedVariables);
                    byte[] initData = ToByteArray(rwDataSegInitData);
                    byte[] externals = ToByteArray(externalSymbols);
                    byte[] instructions = byteCode.GetBytes();
                    byte[] roDataSegment = readOnlyData.ToArray();
                    byte[] doubleConstants = DoublesToArray();
                    byte[] stringVarData = ToByteArray(stringVariableData);
                    executable.Write(header);
                    executable.Write(instructions);
                    executable.Write(roDataSegment);
                    executable.Write(doubleConstants);
                    executable.Write(initData);
                    executable.Write(inputs);
                    executable.Write(outputs);
                    executable.Write(retained);
                    executable.Write(externals);
                    executable.Write(stringVarData);
                }
            }
#if DEBUG
            DisAssembler.DisassembleByteCode(byteCode, outputFile);
#endif   
            byteCode.Clear();
            readOnlyData.Clear();
            externalSymbols.Clear();
            rwDataSegInitData.Clear();
            inputParameters.Clear();
            outputParameters.Clear();
            retainedVariables.Clear();
            d0 = double.NaN;
            d1 = double.NaN;
            d2 = double.NaN;
            d3 = double.NaN;
        }

        public static STLangPOUObject CreateExecutable(string name, bool overWrite = false)
        {
            byte[] header = ToByteArray(fileHeader);
            byte[] inputs = ToByteArray(inputParameters);
            byte[] outputs = ToByteArray(outputParameters);
            byte[] retained = ToByteArray(retainedVariables);
            byte[] initData = ToByteArray(rwDataSegInitData);
            byte[] externals = ToByteArray(externalSymbols);
            uint[] instructions = byteCode.ToArray();
            byte[] roDataSegment = readOnlyData.ToArray();
            byte[] stringVarData = ToByteArray(stringVariableData);
#if DEBUG
            DisAssembler.DisassembleByteCode(byteCode, name);
#endif
            int inputCount = inputParameters.Count;
            int outputCount = outputParameters.Count;
            inputParameters.Clear();
            outputParameters.Clear();
            byteCode.Clear();
            readOnlyData.Clear();
            externalSymbols.Clear();
            rwDataSegInitData.Clear();
            return STLangPOUObject.CreatePOUObject(name, header, instructions, initData, stringVarData, 
                                                   roDataSegment, inputs, inputCount, outputs, outputCount, 
                                                   retained, externals, d0, d1, d2, d3);
        }

        private static VirtualMachineInstruction GetByteCodeAt(int i)
        {
            if (i < 0 || i >= byteCode.Count)
                throw new STLangCompilerError("GetByteCodeAt() failed: Bytecode index out of range.");
            else
                return (VirtualMachineInstruction)(byteCode[i] >> 24);
        }

        private static bool IsJumpInstruction(VirtualMachineInstruction instruction)
        {
            switch (instruction)
            {
                case VirtualMachineInstruction.IJGT:
                case VirtualMachineInstruction.FJGT:
                case VirtualMachineInstruction.DJGT:
                case VirtualMachineInstruction.IJLT:
                case VirtualMachineInstruction.FJLT:
                case VirtualMachineInstruction.DJLT:
                case VirtualMachineInstruction.IJLE:
                case VirtualMachineInstruction.FJLE:
                case VirtualMachineInstruction.DJLE:
                case VirtualMachineInstruction.IJGE:
                case VirtualMachineInstruction.FJGE:
                case VirtualMachineInstruction.DJGE:
                case VirtualMachineInstruction.IJNE:
                case VirtualMachineInstruction.FJNE:
                case VirtualMachineInstruction.DJNE:
                case VirtualMachineInstruction.IJEQ:
                case VirtualMachineInstruction.FJEQ:
                case VirtualMachineInstruction.DJEQ:
                case VirtualMachineInstruction.JUMP:
                case VirtualMachineInstruction.IJEQZ:
                case VirtualMachineInstruction.FJEQZ:
                case VirtualMachineInstruction.DJEQZ:
                case VirtualMachineInstruction.IJNEZ:
                case VirtualMachineInstruction.FJNEZ:
                case VirtualMachineInstruction.DJNEZ:
                case VirtualMachineInstruction.IJGTZ:
                case VirtualMachineInstruction.FJGTZ:
                case VirtualMachineInstruction.DJGTZ:
                case VirtualMachineInstruction.IJLTZ:
                case VirtualMachineInstruction.FJLTZ:
                case VirtualMachineInstruction.DJLTZ:
                case VirtualMachineInstruction.IJGEZ:
                case VirtualMachineInstruction.FJGEZ:
                case VirtualMachineInstruction.DJGEZ:
                case VirtualMachineInstruction.IJLEZ:
                case VirtualMachineInstruction.FJLEZ:
                case VirtualMachineInstruction.DJLEZ:
                    return true;
                default:
                    return false;
            }
        }

        public static void OptimizeByteCode()
        {
            //
            // Find (un)conditonal jumps to an unconditonal jump (to an unconditional jump etc.)
            // and replace the target of the first jump with the target 
            // of the final jump in the chain. An unconditional jump to a return
            // instruction (RETN) is replaced by RETN.
            //
            for (int i = 0; i < byteCode.Count; i++)
            {
                List<int> pcList;
                VirtualMachineInstruction targetInstr;
                VirtualMachineInstruction sourceInstr;

                pcList = new List<int>();
                sourceInstr = GetByteCodeAt(i);
                if (IsJumpInstruction(sourceInstr))
                {
                    int label = (int)(byteCode[i] & 0x00ffffff);
                    targetInstr = GetByteCodeAt(label);
                    if (targetInstr == VirtualMachineInstruction.JUMP)
                    {
                        int j = i;
                        uint instruction;
                        do
                        {
                            pcList.Add(j);
                            j = label;
                            label = (int)(byteCode[label] & 0x00ffffff);
                            targetInstr = GetByteCodeAt(label);
                        }
                        while (targetInstr == VirtualMachineInstruction.JUMP);
                        foreach (int pc in pcList)
                        {
                            instruction = byteCode[pc] & 0xff000000;
                            byteCode[pc] = instruction | (uint)label;
                        }
                    }
                    else if (targetInstr == VirtualMachineInstruction.RETN)
                    {
                        if (sourceInstr == VirtualMachineInstruction.JUMP)
                            byteCode[i] = (uint)VirtualMachineInstruction.RETN << 24;
                    }
                }
            }
        }

        private static double d0, d1, d2, d3;

        private static readonly List<uint> byteCode;

        private static ExecutableFileHeader fileHeader;

        private static readonly List<byte> readOnlyData;

        private static readonly List<IOParameter> inputParameters;

        private static readonly List<IOParameter> outputParameters;

        private static readonly List<IOParameter> retainedVariables;

        private static readonly List<ExternalPOUSymbol> externalSymbols;

        private static readonly List<InitializerData> rwDataSegInitData;

        private static readonly List<StringVariableData> stringVariableData;
    }
}
