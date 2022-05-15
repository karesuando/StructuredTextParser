using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public class ArrayType : TypeNode
    {
        public ArrayType()
            : base("ARRAY[0..1] OF #ERROR#", 0, "[0..1]#ERROR#")
        {
            this.lowerBound = 0;
            this.upperBound = 1;
            this.elementType = TypeNode.Error;
            this.alignment = 0;
            this.basicElementType = TypeNode.Error;
            this.initialValue = new DefaultArrayInitializer(this, Expression.Error);
        }

        public ArrayType(string typeName, int lower, int upper, uint byteCount, Expression size,
               TypeNode elemType, TypeNode basicType, string typeID)
            : base(typeName, byteCount, typeID)
        {
            this.lowerBound = lower;
            this.upperBound = upper;
            this.elementType = elemType;
            this.alignment = elemType.Alignment;
            SaveDataType(typeID, this);
            this.basicElementType = basicType;
            this.initialValue = new DefaultArrayInitializer(this, size);
        }

        public ArrayType(string typeName, int lower, int upper, uint byteCount, Expression size, 
               TypeNode elemType, TypeNode basicType, Expression initialValue, string typeID)
            : base(typeName, byteCount, typeID)
        {
            this.lowerBound = lower;
            this.upperBound = upper;
            this.elementType = elemType;
            this.alignment = elemType.Alignment;
            SaveDataType(typeID, this);
            this.basicElementType = basicType;
            this.initialValue = new DefaultArrayInitializer(this, initialValue, size);
        }

        public ArrayType(string typeName, int lower, int upper, uint byteCount, Expression size,
                         TypeNode elemType, TypeNode basicType, Dictionary<string, InitializerList> flattenedInitLists, string typeID)
            : base(typeName, byteCount, typeID)
        {
            this.lowerBound = lower;
            this.upperBound = upper;
            this.elementType = elemType;
            this.alignment = elemType.Alignment;
            SaveDataType(typeID, this);
            this.basicElementType = basicType;
            this.initialValue = new DefaultArrayOfStructInitializer(this, size, flattenedInitLists);
        }

        public override bool IsArrayType
        {
            get { return true; }
        }

        public int LowerBound 
        { 
            get { return this.lowerBound; } 
        }

        public int UpperBound 
        { 
            get { return this.upperBound; } 
        }

        public int Range 
        { 
            get { return this.upperBound - this.lowerBound + 1; } 
        }

        public TypeNode ElementType 
        { 
            get { return this.elementType; } 
        }

        public TypeNode BasicElementType
        {
            get { return this.basicElementType; }
        }

        public override uint Alignment 
        { 
            get { return this.alignment; } 
        }

        private readonly int lowerBound;

        private readonly int upperBound;

        private readonly uint alignment;

        private readonly TypeNode elementType;

        private readonly TypeNode basicElementType;
    }
}
