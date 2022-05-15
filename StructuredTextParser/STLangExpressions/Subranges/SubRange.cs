using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;

namespace STLang.Subranges
{
    public abstract class SubRange
    {
        public SubRange(Expression lower, Expression upper, TypeNode dataType)
        {
            this.lowerBound = lower;
            this.upperBound = upper;
            this.dataType = dataType;
            string lowerString = lower.ToString();
            string upperString = upper.ToString();
            if (lowerString.Contains('#'))
            {
                char[] separator = { '#' };
                string[] result = lowerString.Split(separator);
                lowerString = result[1];
            }
            if (upperString.Contains('#'))
            {
                char[] separator = { '#' };
                string[] result = upperString.Split(separator);
                upperString = result[1];
            }
            this.subRangeString = lowerString + ".." + upperString;
        }

        public SubRange(long lower, long upper, TypeNode dataType)
        {
            this.lowerBound = this.MakeExpression(lower, dataType);
            this.upperBound = this.MakeExpression(upper, dataType);
            this.subRangeString = lower + ".." + upper;
            this.dataType = dataType;
        }

        public SubRange(ulong lower, ulong upper, TypeNode dataType)
        {
            this.lowerBound = this.MakeExpression(lower, dataType);
            this.upperBound = this.MakeExpression(upper, dataType);
            this.subRangeString = lower + ".." + upper;
            this.dataType = dataType;
        }

        public SubRange(ushort lower, ushort upper, TypeNode dataType)
        {
            this.lowerBound = this.MakeExpression(lower, dataType);
            this.upperBound = this.MakeExpression(upper, dataType);
            this.subRangeString = this.lowerBound + ".." + this.upperBound;
            this.dataType = dataType;
        }

        private Expression MakeExpression(long value, TypeNode dataType)
        {
            if (dataType == TypeNode.SInt)
                return new SIntConstant((sbyte)value);
            else if (dataType == TypeNode.Int)
                return new IntConstant((short)value);
            else if (dataType == TypeNode.DInt)
                return new DIntConstant((int)value);
            else
                return new LIntConstant(value);
        }

        private Expression MakeExpression(ulong value, TypeNode dataType)
        {
            if (dataType == TypeNode.USInt)
                return new USIntConstant((byte)value);
            else if (dataType == TypeNode.UInt)
                return new UIntConstant((ushort)value);
            else if (dataType == TypeNode.UDInt)
                return new UDIntConstant((uint)value);
            else
                return new ULIntConstant(value);
        }

        private Expression MakeExpression(ushort enumValue, TypeNode dataType)
        {
            if (!dataType.IsEnumeratedType)
                return Expression.Error;
            else
            {
                EnumeratedType baseType = (EnumeratedType)dataType.BaseType;
                IEnumerable<string> identList = baseType.IdentifierList;
                if (enumValue >= identList.Count())
                    return Expression.Error;
                else
                {
                    string enumIdent = identList.ElementAt(enumValue);
                    return new EnumConstant(enumValue, dataType, enumIdent);
                }
            }
        }

        static SubRange()
        {
            Error = new ErrorSubrange();
        }

        public TypeNode DataType
        {
            get { return this.dataType; }
        }

        public override string ToString()
        {
            return this.subRangeString;
        }

        public virtual bool InRange(long lower, long upper)
        {
            throw new NotImplementedException("");
        }

        public virtual bool InRange(ulong lower, ulong upper)
        {
            throw new NotImplementedException("");
        }

        public abstract object CreateInterval();

        public abstract bool Contains(Expression value);

        public abstract bool Contains(SubRange subRange);

        public abstract bool AreDisjoint(SubRange subRange);

        private readonly Expression lowerBound;

        private readonly Expression upperBound;

        private readonly TypeNode dataType;

        private readonly string subRangeString;

        public static readonly SubRange Error;
    }
}
