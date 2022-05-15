using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using STLang.DataTypes;
using STLang.ErrorManager;

namespace STLang.MemoryLayout
{
    public class ElementaryLocation : MemoryLocation
    {
        public ElementaryLocation(int index, TypeNode dataType, int elemCount = 1) :
            base(index, dataType, elemCount)
        {
        }

        public override bool IsElementaryLocation
        {
            get { return true; }
        }

        public override MemoryLocation AddOffset(int offset)
        {
            return new ElementaryLocation(this.Index + offset, this.DataType, this.ElementCount);
        }
    }
}
