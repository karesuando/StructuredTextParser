using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;
using STLang.ErrorManager;
using System.Collections;
using QUT.Gppg;

namespace STLang.DataTypes
{
    public abstract class TypeNode
    {
        public TypeNode(string name, uint size, string typeID = "")
        {
            this.name = name;
            this.size = size;
            this.typeID = typeID;
        }

        static TypeNode()
        {
            userDefinedTypes = new Dictionary<string,TypeNode>();
            // Elementary types
            SInt = new SIntType();
            Int = new IntType();
            DInt = new DIntType();
            LInt = new LIntType();
            USInt = new USIntType();
            UInt = new UIntType();
            UDInt = new UDIntType();
            ULInt = new ULIntType();
            Real = new RealType();
            LReal = new LRealType();
            Bool = new BoolType();
            Byte = new ByteType();
            Char = new CharType();
            WChar = new WCharType();
            Word = new WordType();
            DWord = new DWordType();
            LWord = new LWordType();
            TimeOfDay = new TimeOfDayType();
            Date = new DateType();
            Time = new TimeType();
            DateAndTime = new DateAndTimeType();
            String = new StringType();
            WString = new WStringType();
            Void = new VoidType();

            // Generic types
            Any = new AnyType();
            AnyDerived = new AnyDerivedType();
            AnyElementary = new AnyElementaryType();
            AnyMagnitude = new AnyMagnitudeType();
            AnyNum = new AnyNumType();
            AnyInt = new AnyIntType();
            AnyBit = new AnyBitType();
            AnyReal = new AnyRealType();
            AnyString = new AnyStringType();
            AnyDate = new AnyDateType();
            Error = new ErrorType();
        }

        public virtual bool IsElementaryType 
        { 
            get { return false; } 
        }

        public virtual bool IsNumericalType 
        { 
            get { return false; } 
        }

        public virtual bool IsOrdinalType 
        { 
            get { return false; } 
        }

        public virtual bool IsDerivedType
        {
            get { return false; }
        }

        public virtual bool IsBitStringType 
        { 
            get { return false; } 
        }

        public virtual bool IsIntegerType 
        { 
            get { return false; } 
        }

        public virtual bool IsSignedIntType 
        { 
            get { return false; } 
        }

        public virtual bool IsUnsignedIntType 
        { 
            get { return false; } 
        }

        public virtual bool IsAnyStringType
        {
            get { return false; }
        }

        public virtual bool IsStringType 
        { 
            get { return false; } 
        }

        public virtual bool IsCharType
        {
            get { return false; }
        }

        public virtual bool IsWCharType
        {
            get { return false; }
        }

        public virtual bool IsWStringType 
        { 
            get { return false; } 
        }

        public virtual bool IsDateClass 
        { 
            get { return false; } 
        }

        public virtual bool IsDateType 
        { 
            get { return false; } 
        }

        public virtual bool IsDateTimeType 
        { 
            get { return false; } 
        }

        public virtual bool IsTimeType 
        { 
            get { return false; } 
        }

        public virtual bool IsTimeOfDayType 
        { 
            get { return false; } 
        }

        public virtual bool IsEnumeratedType 
        { 
            get { return false; } 
        }

        public virtual bool IsArrayType
        {
            get { return false; }
        }

        public virtual bool IsStructType
        {
            get { return false; }
        }

        public virtual bool IsFunctionBlockType
        {
            get { return false; }
        }

        public virtual bool IsUndefinedType
        {
            get { return false; }
        }

        public virtual TypeNode BaseType
        {
            get { return this; }
        }

        public string Name 
        { 
            get { return this.name; } 
        }

        public string TypeID
        {
            get { return this.typeID; }
        }

        public uint Size 
        { 
            get { return this.size; } 
        }

        public virtual int BitCount 
        { 
            get { return (int)(8 * this.size); } 
        }

        public virtual uint Alignment 
        { 
            get { return this.size; } 
        }

        public virtual bool IsSubrangeType 
        {
            get { return false; } 
        }

        public virtual bool IsInRange(SubRange subRange)
        {
            string msg;
            msg = "IsInRange(SubRange) not implemented for type " + this.Name;
            throw new NotImplementedException(msg);
        }

        public virtual bool IsInRange(ulong value)
        {
            string msg;
            msg = "IsInRange(ulong) not implemented for type " + this.Name;
            throw new NotImplementedException(msg);
        }

        public virtual bool IsInRange(long value)
        {
            string msg;
            msg = "IsInRange(long) not implemented for type " + this.Name;
            throw new NotImplementedException(msg);
        }

        public virtual TypeNode MakeSubrange(string name, SubRange subRange, LexLocation loc)
        {
            string msg;
            msg = "MakeSubrange(string, Subrange) not implemented for type " + this.Name;
            throw new NotImplementedException(msg);
        }

        public virtual SubRange GetSubrange()
        {
            string msg = "GetSubrange() not implemented for type " + this.Name;
            throw new NotImplementedException(msg);
        }

        public virtual Expression CreateInitializerList(int elementCount)
        {
            string msg = "CreateInitializerList() not implmeneted.";
            throw new NotImplementedException(msg);
        }

        public static bool operator ==(TypeNode dataType1, TypeNode dataType2)
        {
            if ((object)dataType2 == null)
                return (object)dataType1 == null;
            else if ((object)dataType1 == null)
                return false;
            else if ((object)dataType1 == (object)dataType2)
                return true;
            else if (dataType1 is DerivedType || dataType1.IsSubrangeType)
                return dataType1.BaseType == dataType2;
            else if (dataType2 is DerivedType || dataType2.IsSubrangeType)
                return dataType2.BaseType == dataType1;
            else
                return false;
        }

        public static bool operator !=(TypeNode dataType1, TypeNode dataType2)
        {
            return ! (dataType1 == dataType2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        // public override bool Equals(object obj)
        //
        // Two types t1 and t2 are equal if either t1 == t2 or 
        // both are instances of the same class.
        //
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else if (! (obj is TypeNode))
                return false;
            else if ((TypeNode)obj == this)
                return true;
            else
                return obj.GetType() == this.GetType();
        }

        public virtual float ConversionCost(Expression expression)
        {
            if (expression == null)
                return 0.0f;
            else if (expression.DataType == this)
                return 0.0f;
            else if (expression.DataType == Error)
                return 0.0f;
            else
                return MAX_CONVERSION_COST;
        }

        public static bool LookUpType(string typeID, out TypeNode userDefType)
        {
            if (userDefinedTypes.ContainsKey(typeID))
            {
                userDefType = userDefinedTypes[typeID];
                return true;
            }
            else
            {
                userDefType = null;
                return false;
            }
        }

        protected static void SaveDataType(string typeID, TypeNode userDefType)
        {
            if (! userDefinedTypes.ContainsKey(typeID))
            {
                userDefinedTypes[typeID] = userDefType;
            }
        }

        public static ErrorHandler Report { get; set; }

        public Expression DefaultValue
        {
            get { return this.initialValue; }
            set { this.initialValue = value; }
        }

        private readonly string name;

        private readonly uint size;

        private readonly string typeID;

        protected Expression initialValue;

        protected const int INT_SIZE = sizeof(short);

        protected const int SINT_SIZE = sizeof(sbyte);

        protected const int CHAR_SIZE = sizeof(byte);

        protected const int WCHAR_SIZE = sizeof(char);

        protected const int DINT_SIZE = sizeof(int);

        protected const int LINT_SIZE = sizeof(long);

        protected const int UINT_SIZE = sizeof(ushort);

        protected const int USINT_SIZE = sizeof(byte);

        protected const int UDINT_SIZE = sizeof(uint);

        protected const int ULINT_SIZE = sizeof(ulong);

        protected const int REAL_SIZE = sizeof(float);

        protected const int LREAL_SIZE = sizeof(double);

        protected const int DATE_SIZE = sizeof(long);

        protected const int TOD_SIZE = sizeof(int);

        protected const int DT_SIZE = sizeof(long);

        protected const int TIME_SIZE = sizeof(long);

        protected const int BOOL_SIZE = sizeof(bool);

        protected const int BYTE_SIZE = sizeof(byte);

        protected const int WORD_SIZE = sizeof(ushort);

        protected const int DWORD_SIZE = sizeof(uint);

        protected const int LWORD_SIZE = sizeof(ulong);

        protected const int ENUM_SIZE = sizeof(int);

        public const float MAX_CONVERSION_COST = 1000.0f;

        public static readonly VoidType Void;

        public static readonly StringType String;

        public static readonly WStringType WString;

        public static readonly SIntType SInt;

        public static readonly IntType Int;

        public static readonly DIntType DInt;

        public static readonly LIntType LInt;

        public static readonly USIntType USInt;

        public static readonly UIntType UInt;

        public static readonly UDIntType UDInt;

        public static readonly ULIntType ULInt;

        public static readonly RealType Real;

        public static readonly LRealType LReal;

        public static readonly BoolType Bool;

        public static readonly ByteType Byte;

        public static readonly CharType Char;

        public static readonly WCharType WChar;

        public static readonly WordType Word;

        public static readonly DWordType DWord;

        public static readonly LWordType LWord;

        public static readonly TimeOfDayType TimeOfDay;

        public static readonly DateType Date;

        public static readonly TimeType Time;

        public static readonly DateAndTimeType DateAndTime;

        public static readonly AnyType Any;

        public static readonly AnyElementaryType AnyElementary;

        public static readonly AnyMagnitudeType AnyMagnitude;

        public static readonly AnyDerivedType AnyDerived;

        public static readonly AnyNumType AnyNum;

        public static readonly AnyIntType AnyInt;

        public static readonly AnyRealType AnyReal;

        public static readonly AnyStringType AnyString;

        public static readonly AnyBitType AnyBit;

        public static readonly AnyDateType AnyDate;

        public static readonly ErrorType Error;

        private static readonly Dictionary<string, TypeNode> userDefinedTypes;
    }
}
