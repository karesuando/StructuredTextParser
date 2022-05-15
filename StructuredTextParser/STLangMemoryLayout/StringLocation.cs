using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;

namespace STLang.MemoryLayout
{
    public class StringLocation : MemoryLocation
    {
        public StringLocation(int index, int bufferOffset, TypeNode dataType, int elemCount = 1) :
            base(index, dataType, elemCount)
        {
            this.bufferOffset = bufferOffset;
        }

        public override MemoryLocation AddOffset(int offset)
        {
            return new ElementaryLocation(this.Index + offset, this.DataType, this.ElementCount);
        }

        public override bool IsStringLocation
        {
            get { return true; }
        }

        public int BufferOffset
        {
            get { return this.bufferOffset; }
        }

        private readonly int bufferOffset;
    }
}
