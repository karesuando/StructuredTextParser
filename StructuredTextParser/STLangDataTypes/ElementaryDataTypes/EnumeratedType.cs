using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.DataTypes
{
    public class EnumeratedType : OrdinalType<ushort>
    {
        public EnumeratedType(List<string> identList)
            : base(MakeName(identList), INT_SIZE, 0, (ushort)identList.Count, "E" + enumTypeCount++)
        {
            this.identList = identList;
            this.baseType = this;
            this.initialValue = new EnumConstant(0, this, identList[0]);
        }

        public EnumeratedType(string name, ushort lower, ushort upper, List<string> identList,
                              TypeNode baseType, string typeID)
            : base(name, ENUM_SIZE, lower, upper, baseType, typeID)
        {
            this.identList = identList;
            this.baseType = baseType;
            this.initialValue = new EnumConstant(lower, this, identList[0]);
        }

        public EnumeratedType(string name, ushort lower, ushort upper, List<string> identList,
                              TypeNode baseType, Expression initVal, string typeID)
            : base(name, ENUM_SIZE, lower, upper, baseType, typeID)
        {
            this.identList = identList;
            this.baseType = baseType;
            this.initialValue = initVal;
        }

        public override bool IsEnumeratedType 
        { 
            get { return true; } 
        }

        public override TypeNode BaseType 
        { 
            get { return this.baseType; } 
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else if (!(obj is EnumeratedType))
                return false;
            else
                return this.baseType == ((EnumeratedType)obj).BaseType;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool IsInRange(ulong value)
        {
            return value >= this.LowerBound && value <= this.UpperBound;
        }

        public override bool IsInRange(SubRange subRange)
        {
            return subRange.InRange(this.LowerBound, this.UpperBound);
        }

        public override SubRange GetSubrange()
        {
            return new EnumSubrange(this.LowerBound, this.UpperBound, this);
        }

        static EnumeratedType()
        {
            enumTypeCount = 0;
            typeNames = new List<string>();
        }

        public override TypeNode MakeSubrange(string typeName, SubRange subRange, LexLocation loc)
        {
            if (!(subRange is EnumSubrange))
                return Error;
            else if (subRange.DataType != this)
                return Error;
            else
            {
                EnumSubrange enumSubrange = (EnumSubrange)subRange;
                ushort lower = enumSubrange.LowerBound;
                ushort upper = enumSubrange.UpperBound;
                
                if (!this.GetSubrange().Contains(subRange))
                {
                    char[] delims = new char[] { '(' };
                    string baseName = typeName.Split(delims)[0];
                    Report.SemanticError(-3, subRange.ToString(), baseName, loc);
                    return this;
                }
                else
                {
                    EnumeratedType baseType;
                    baseType = (EnumeratedType)subRange.DataType.BaseType;
                    IEnumerable<string> basicIdentifiers = baseType.IdentifierList;
                    List<string> identList = new List<string>();
                    for (int i = lower; i <= upper; i++)
                        identList.Add(basicIdentifiers.ElementAt(i));
                    string typeID = this.TypeID + "(" + subRange + ")";
                    return new EnumeratedType(typeName, lower, upper, identList, this, typeID);
                }
            }
        }

        public IEnumerable<string> IdentifierList 
        { 
            get { return this.identList; } 
        }

        public static IEnumerable<string> TypeNames 
        { 
            get { return typeNames; } 
        }

        public static void AddTypeName(string typeName)
        {
            typeName = typeName.ToUpper();
            if (! typeNames.Contains(typeName))
                typeNames.Add(typeName.ToUpper());
        }

        public override string ToString()
        {
            string text;
            int lower = (int)this.LowerBound;
            int upper = (int)this.UpperBound;

            if (upper > lower + 5)
                text = string.Format("ENUM({0}, ..., {1})", this.identList[lower], this.identList[upper]);
            else {
                text = "ENUM(" + this.identList[lower];
                for (int i = lower + 1; i < upper; i++)
                {
                    text += "," + this.identList[i];
                }
                text += ")";
            }
            return text;
        }

        private static string MakeName(List<string> identList)
        {
            string text;
            int count = identList.Count;

            if (count > 5)
                text = string.Format("ENUM({0}, ..., {1})", identList[0], identList[count - 1]);
            else
            {
                text = "ENUM(" + identList[0];
                for (int i = 1; i < count; i++)
                {
                    text += "," + identList[i];
                }
                text += ")";
            }
            return text;
        }

        private readonly List<string> identList;

        private readonly TypeNode baseType;

        private static int enumTypeCount;

        private static readonly List<string> typeNames;
    }
}
