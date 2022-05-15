using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.DataTypes
{
    public class DerivedType : TypeNode
    {
        public DerivedType(string name, TypeNode baseType, Expression initValue) 
            : base(name, baseType.Size, baseType.TypeID)
        {
            this.baseType = baseType;
            this.initialValue = initValue;
        }

        public override TypeNode BaseType 
        { 
            get { return this.baseType; } 
        }

        public override bool IsDerivedType
        {
            get { return true; }
        }

        public override bool IsElementaryType 
        { 
            get { return this.baseType.IsElementaryType; } 
        }

        public override bool IsOrdinalType 
        { 
            get { return this.baseType.IsOrdinalType; } 
        }

        public override bool IsBitStringType 
        { 
            get { return this.baseType.IsBitStringType; } 
        }

        public override bool IsIntegerType 
        { 
            get { return this.baseType.IsIntegerType; } 
        }

        public override bool IsSignedIntType 
        { 
            get { return this.baseType.IsSignedIntType; } 
        }

        public override bool IsUnsignedIntType 
        { 
            get { return this.baseType.IsUnsignedIntType; } 
        }

        public override bool IsStringType 
        { 
            get { return this.baseType.IsStringType; } 
        }

        public override bool IsWStringType 
        { 
            get { return this.baseType.IsWStringType; } 
        }

        public override bool IsDateClass 
        { 
            get { return this.baseType.IsDateClass; } 
        }

        public override bool IsDateType 
        { 
            get { return this.baseType.IsDateType; } 
        }

        public override bool IsDateTimeType 
        { 
            get { return this.baseType.IsDateTimeType; } 
        }

        public override bool IsTimeType 
        { 
            get { return this.baseType.IsTimeType; } 
        }

        public override bool IsTimeOfDayType 
        { 
            get { return this.baseType.IsTimeOfDayType; } 
        }

        public override bool IsEnumeratedType 
        { 
            get { return this.baseType.IsEnumeratedType; } 
        }

        public override bool IsArrayType
        {
            get { return this.baseType.IsArrayType; }
        }

        public override bool IsFunctionBlockType
        {
            get { return this.baseType.IsFunctionBlockType; }
        }

        public override bool IsStructType
        {
            get { return this.baseType.IsStructType; }
        }

        public override bool IsSubrangeType 
        {
            get { return this.baseType.IsSubrangeType; }
        }

        public override bool IsInRange(SubRange subRange)
        {
            return this.baseType.IsInRange(subRange);
        }

        public override bool IsInRange(ulong value)
        {
            return this.baseType.IsInRange(value);
        }

        public override bool IsInRange(long value)
        {
            return this.baseType.IsInRange(value);
        }

        public override TypeNode MakeSubrange(string name, SubRange subRange, LexLocation loc)
        {
            return this.baseType.MakeSubrange(name, subRange, loc);
        }

        public override SubRange GetSubrange()
        {
            return this.baseType.GetSubrange();
        }

        public override float ConversionCost(Expression expression)
        {
            return this.baseType.ConversionCost(expression);
        }

        private readonly TypeNode baseType;
    }
}
