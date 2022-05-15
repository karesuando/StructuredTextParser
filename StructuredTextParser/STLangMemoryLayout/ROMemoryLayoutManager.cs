using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.ErrorManager;

namespace STLang.MemoryLayout
{
    public class ROMemoryLayoutManager
    {
        public ROMemoryLayoutManager()
        {
            this.Reset();
        }

        public void Reset()
        {
            this.realIndex = 0;
            this.lrealIndex = 0;
            this.dintIndex = 0;
            this.lintIndex = 0;
            this.stringIndex = 0;
            this.wstringIndex = 0;
            this.stringCharCount = 0;
            this.wstringCharCount = 0;
            this.lrealRegister = 0;
            this.roDataSegmentSize = 0;
        }

        private MemoryLocation MakeElementaryLocation(TypeNode dataType, int index)
        {
            this.roDataSegmentSize += (int)dataType.Size;
            return new ElementaryLocation(index, dataType);
        }

        private MemoryLocation MakeStringLocation(TypeNode dataType, string stringValue)
        {
            int stringBufferSize;
            MemoryLocation location;

            if (stringValue == null)
                stringBufferSize = (int)dataType.Size;
            else
                stringBufferSize = stringValue.Length + 1;
            location = new StringLocation(this.stringIndex, stringBufferSize, dataType);
            int stringPointerSize = (int)TypeNode.DInt.Size;
            this.roDataSegmentSize += stringBufferSize + stringPointerSize;
            this.stringCharCount += stringBufferSize;
            this.stringIndex++;
            return location;
        }

        private MemoryLocation MakeWStringLocation(TypeNode dataType, string stringValue)
        {
            int wstringBufferSize;
            MemoryLocation location;
            if (stringValue == null)
                wstringBufferSize = (int)dataType.Size;
            else
                wstringBufferSize = stringValue.Length + 1;
            location = new StringLocation(this.wstringIndex, wstringBufferSize, dataType);
            int wstringPointerSize = (int)TypeNode.DInt.Size;
            this.roDataSegmentSize += wstringBufferSize + wstringPointerSize;
            this.wstringCharCount += wstringBufferSize;
            this.wstringIndex++;
            return location;
        }

        public MemoryLocation GenerateIndex(TypeNode dataType, string stringValue = null)
        {
            if (dataType == null)
                throw new STLangCompilerError("GenerateIndex() failed: dataType is null");
            else if (dataType == TypeNode.LReal)
            {
                if (this.lrealRegister < MAX_REGISTER_COUNT)
                    return new RegisterConstant(dataType, this.lrealRegister++);
                else
                    return this.MakeElementaryLocation(dataType, this.lrealIndex++);
            }
            else if (dataType == TypeNode.Real)
                return this.MakeElementaryLocation(dataType, this.realIndex++);
            else if (dataType.IsDateTimeType)
                return this.MakeElementaryLocation(dataType, this.lintIndex++);
            else if (dataType.IsOrdinalType)
            {
                if (dataType.Size == 4)
                    return this.MakeElementaryLocation(dataType, this.dintIndex++);
                else if (dataType.Size == 8)
                    return this.MakeElementaryLocation(dataType, this.lintIndex++);
                else if (dataType.Size < 4)
                {
                    string msg;

                    msg = "GenerateIndex() failed: " + dataType.Name + " constants are inlined.";
                    throw new STLangCompilerError(msg);
                }
            }
            else if (dataType.IsStringType)
                return this.MakeStringLocation(dataType, stringValue);
            else if (dataType.IsWStringType)
                return this.MakeWStringLocation(dataType, stringValue);
         
            string msg2;
            msg2 = "GenerateIndex() failed for datatype " + dataType.Name;
            throw new STLangCompilerError(msg2);
            
        }

        public int RealCount
        {
            get { return this.realIndex; }
        }

        public int LRealCount
        {
            get { return this.lrealIndex; }
        }

        public int DIntCount
        {
            get { return this.dintIndex; }
        }

        public int LIntCount
        {
            get { return this.lintIndex; }
        }

        public int StringCount
        {
            get { return this.stringIndex; }
        }

        public int WStringCount
        {
            get { return this.wstringIndex; }
        }

        public int StringCharCount
        {
            get { return this.stringCharCount; }
        }

        public int WStringCharCount
        {
            get { return this.wstringCharCount; }
        }

        public int RODataSegmentSize
        {
            get { return this.roDataSegmentSize; }
        }

        public int LRealRegisterCount
        {
            get { return this.lrealRegister; }
        }

        private int roDataSegmentSize;

        private int realIndex;

        private int lrealIndex;

        private int dintIndex;

        private int lintIndex;

        private int stringIndex;

        private int wstringIndex;

        private int stringCharCount;

        private int wstringCharCount;

        private int lrealRegister;

        private const int MAX_REGISTER_COUNT = 4;
    }
}
