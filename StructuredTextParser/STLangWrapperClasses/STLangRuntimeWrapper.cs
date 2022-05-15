using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace STLang.RuntimeWrapper
{
    public class STLangPOUObject
    {
        private STLangPOUObject()
        {
            throw new NotImplementedException("STLangPOUObject() not implemented");
        }

        private STLangPOUObject(int inputCount, int outputCount, int pouHandle)
        {
            this.inputCount = inputCount;
            this.outputCount = outputCount;
            this.pouHandle = pouHandle;
        }

        
        [DllImport("STLangRuntime.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]

        static extern int CreatePOUObject(IntPtr name, byte[] header, uint[] instructions, byte[] rwDataSegInit, 
                                          byte[] stringVarData, byte[] readOnlyDataSegment, byte[] inputs, byte[] outputs, 
                                          byte[] retained, byte[] externalPOUs, double d0, double d1, double d2, double d3);

        public static STLangPOUObject CreatePOUObject(string name, byte[] header, uint[] instructions, byte[] rwDataSegInit, 
                                          byte[] stringVarData, byte[] roDataSegment, byte[] inputs, int inputCount,
                                          byte[] outputs, int outputCount, byte[] retained, byte[] externalPOUs, 
                                          double d0, double d1, double d2, double d3)
        {
            IntPtr namePtr = Marshal.AllocHGlobal(name.Length);
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] nameBytes = encoding.GetBytes(name);
            Marshal.Copy(nameBytes, 0, namePtr, nameBytes.Length);
            int handle = CreatePOUObject(namePtr, header, instructions, rwDataSegInit, stringVarData, roDataSegment, inputs, outputs, retained, externalPOUs, d0, d1, d2, d3);
            Marshal.FreeHGlobal(namePtr);
            return new STLangPOUObject(inputCount, outputCount, handle);
        }

        [DllImport("STLangRuntime.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int CreatePOUObject(StringBuilder executable);

        public static STLangPOUObject Create(string execFile)
        {
            StringBuilder executable = new StringBuilder(execFile);
            int handle = 0;
            int inputCount = 0;
            int outputCount = 0;
            try
            {
                handle = CreatePOUObject(executable);
                inputCount = GetInPutCount(handle);
                outputCount = GetOutPutCount(handle);
            }
            catch (SystemException e)
            {
                string msg = e.Message;
                int i;

                i = 0;
            }
            return new STLangPOUObject(inputCount, outputCount, handle);
        }

        [DllImport("STLangRuntime.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern void GetPOUName(IntPtr intPtr, int POUHandle);

        public string POUName
        {
            get
            {
                IntPtr intPtr = Marshal.AllocHGlobal(MaxIONameLength);
                GetPOUName(intPtr, this.pouHandle);
                string pouName = Marshal.PtrToStringAnsi(intPtr);
                Marshal.FreeHGlobal(intPtr);
                return pouName;
            }
        }

        [DllImport("STLangRuntime.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int GetInPutCount(int POUHandle);

        public int InputCount
        {
            get { return GetInPutCount(this.pouHandle); }
        }

        [DllImport("STLangRuntime.dll", CallingConvention = CallingConvention.Cdecl)]   
        static extern int GetOutPutCount(int POUHandle);

        public int GetOutputCount
        {
            get { return GetOutPutCount(this.pouHandle); }
        }

        [DllImport("STLangRuntime.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern double GetOutputValue(StringBuilder OutPut, int POUHandle);

        public double GetOutputValue(string output)
        {
            StringBuilder stringBldr = new StringBuilder(output.ToUpper());
            double result = GetOutputValue(stringBldr, this.pouHandle);
            return result;
        }

        [DllImport("STLangRuntime.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void SetOutputValue(StringBuilder OutPut, double Value, int POUHandle);

        public void SetOutputValue(string output, double value)
        {
            StringBuilder stringBldr = new StringBuilder(output.ToUpper());
            SetOutputValue(stringBldr, value, this.pouHandle);
        }

        [DllImport("STLangRuntime.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern int GetInputNames(IntPtr[] Inputs, int POUHandle);

        public string[] InputNames
        {
            get
            {
                int inputCount = GetInPutCount(this.pouHandle);
                IntPtr[] buffer = new IntPtr[inputCount];
                for (int i = 0; i < inputCount; i++)
                    buffer[i] = Marshal.AllocHGlobal(MaxIONameLength);
                GetInputNames(buffer, this.pouHandle);
                string[] inputs = new string[inputCount];
                for (int i = 0; i < inputCount; i++)
                {
                    inputs[i] = Marshal.PtrToStringAnsi(buffer[i]);
                    Marshal.FreeHGlobal(buffer[i]);
                }
                return inputs;
            }
        }

        [DllImport("STLangRuntime.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern int GetOutputNames(IntPtr[] Outputs, int POUHandle);

        public string[] OutputNames
        {
            get
            {
                int outputCount = GetOutPutCount(this.pouHandle);
                IntPtr[] buffer = new IntPtr[outputCount];
                for (int i = 0; i < outputCount; i++)
                    buffer[i] = Marshal.AllocHGlobal(MaxIONameLength);
                GetOutputNames(buffer, this.pouHandle);
                string[] outputs = new string[outputCount];
                for (int i = 0; i < outputCount; i++)
                {
                    outputs[i] = Marshal.PtrToStringAnsi(buffer[i]);
                    Marshal.FreeHGlobal(buffer[i]);
                }
                return outputs;
            }
        }

        [DllImport("STLangRuntime.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern double ExecutePOU(double[] argV, int argC, int pouHandle);

        public double Execute(double[] inputs)
        {
            return ExecutePOU(inputs, this.inputCount, this.pouHandle);
        }

        private const int MaxIONameLength = 64;

        private readonly int inputCount;

        private readonly int outputCount;

        private readonly int pouHandle;
    }
}
