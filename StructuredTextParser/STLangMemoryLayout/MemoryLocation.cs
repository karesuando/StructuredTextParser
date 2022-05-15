using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.ErrorManager;

namespace STLang.MemoryLayout
{
    // public abstract class MemoryLocation
    //
    // MemoryLocation is an abstract base class for classes that represent a memory
    // location of a variable.
    //
    public abstract class MemoryLocation
    {
        public MemoryLocation(int index, TypeNode dataType, int elemCount)
        {
            if (index > MAX_INDEX)
                throw new STLangCompilerError("Maximum offset exceeded.");
            else
            {
                this.index = index;
                this.dataType = dataType;
                this.elementCount = elemCount;
                this.AbsoluteAddress = null;
                this.Size = null;
            }
        }

        static MemoryLocation()
        {
            undefinedLocation = new UndefinedLocation();
        }
  
        public Expression AbsoluteAddress
        {
            get;
            set;
        }

        public Expression Size
        {
            get;
            set;
        }

        public TypeNode DataType
        {
            get { return this.dataType; }
        }

        public int Index
        {
            get { return this.index; }
        }

        public int ElementCount
        {
            get { return this.elementCount; }
        }

        public virtual bool IsRegisterVariable
        {
            get { return false; }
        }

        public virtual bool IsRegisterConstant
        {
            get { return false; }
        }

        public virtual bool IsElementaryLocation
        {
            get { return false; }
        }

        public virtual bool IsStringLocation
        {
            get { return false; }
        }

        public virtual MemoryLocation AddOffset(int offset)
        {
            throw new NotImplementedException("RegisterLocation:AddOffset() not implemented.");
        }

        public static UndefinedLocation UndefinedLocation
        {
            get { return undefinedLocation; }
        }

        private static readonly UndefinedLocation undefinedLocation;

        public const uint MAX_INDEX = 0x00ffffff;

        private readonly int index;

        private readonly TypeNode dataType;

        private readonly int elementCount;
    }
}
