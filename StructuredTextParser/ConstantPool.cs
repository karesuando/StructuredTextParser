using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using STLang.DataTypes;
using STLang.Extensions;
using STLang.Expressions;
using STLang.MemoryLayout;
using STLang.ErrorManager;

namespace STLang.ConstantPoolHandler
{
    public class ConstantPool
    {
        public ConstantPool(Dictionary<string, Expression> constantTable)
        {
            TypeNode dataType;
            List<Expression> doubles = new List<Expression>();
            List<Expression> floats = new List<Expression>();
            List<Expression> longs = new List<Expression>();
            List<Expression> ints = new List<Expression>();
            List<Expression> wstrings = new List<Expression>();
            List<Expression> strings = new List<Expression>();
            List<int> stringOffsets = new List<int>();
            List<int> wstringOffsets = new List<int>();

            foreach (Expression constant in constantTable.Values)
            {
                if (constant == null)
                    throw new STLangCompilerError("Null expression in constant table");
                else
                {
                    dataType = constant.DataType;
                    if (dataType == TypeNode.Error)
                        throw new STLangCompilerError("Error datatype of constant in constant table");
                    else if (constant.Location == MemoryLocation.UndefinedLocation)
                        continue; // Don't store unused/inlined constants in the ro data segment
                    else if (constant.Location.IsRegisterConstant)
                        continue; // Register constants are stored in local variables
                    else if (dataType == TypeNode.LReal)
                        doubles.Add(constant);
                    else if (dataType == TypeNode.Real)
                        floats.Add(constant);
                    else if (dataType == TypeNode.DInt || dataType == TypeNode.UDInt)
                        ints.Add(constant);
                    else if (dataType == TypeNode.LInt || dataType == TypeNode.ULInt)
                        longs.Add(constant);
                    else if (dataType == TypeNode.DWord)
                        ints.Add(constant);
                    else if (dataType == TypeNode.LWord)
                        longs.Add(constant);
                    else if (dataType == TypeNode.DateAndTime)
                        longs.Add(constant);
                    else if (dataType == TypeNode.Date)
                        longs.Add(constant);
                    else if (dataType == TypeNode.Time)
                        longs.Add(constant);
                    else if (dataType == TypeNode.TimeOfDay)
                        longs.Add(constant);
                    else if (dataType.IsStringType)
                        strings.Add(constant);
                    else if (dataType.IsWStringType)
                        wstrings.Add(constant);
                }
            }
            this.SortByIndex(ref doubles);
            this.SortByIndex(ref floats);
            this.SortByIndex(ref longs);
            this.SortByIndex(ref ints);
            this.SortByIndex(ref strings);
            this.SortByIndex(ref wstrings);
            foreach (Expression stringValue in strings)
            {
                MemoryLocation location = stringValue.Location;
                if (!location.IsStringLocation)
                {
                    string msg;
                    msg = "ConstantPool: StringLocation expected for STRING value";
                    throw new STLangCompilerError(msg);
                }
                else
                {
                    StringLocation stringLocation;
                    stringLocation = (StringLocation)location;
                    stringOffsets.Add(stringLocation.BufferOffset);
                }
            }
            foreach (Expression wstringValue in wstrings)
            {
                MemoryLocation location = wstringValue.Location;
                if (!location.IsStringLocation)
                {
                    string msg;
                    msg = "ConstantPool: StringLocation expected for WSTRING value";
                    throw new STLangCompilerError(msg);
                }
                else
                {
                    StringLocation stringLocation;
                    stringLocation = (StringLocation)location;
                    wstringOffsets.Add(stringLocation.BufferOffset);
                }
            }
            List<byte> bytes = new List<byte>();
            foreach (Expression value in doubles)
                bytes.AddRange(value.GetBytes());
            foreach (Expression value in longs)
                bytes.AddRange(value.GetBytes());
            foreach (Expression value in floats)
                bytes.AddRange(value.GetBytes());
            foreach (Expression value in ints)
                bytes.AddRange(value.GetBytes());
            foreach (int offset in wstringOffsets)
                bytes.AddRange(offset.GetBytes());
            foreach (int offset in stringOffsets)
                bytes.AddRange(offset.GetBytes());
            foreach (Expression value in wstrings)
                bytes.AddRange(value.GetBytes());
            foreach (Expression value in strings)
                bytes.AddRange(value.GetBytes());
            this.DoubleCount = doubles.Count;
            this.FloatCount = floats.Count;
            this.LongCount = longs.Count;
            this.IntCount = ints.Count;
            this.WStringCount = wstrings.Count;
            this.StringCount = strings.Count;
            this.Bytes = bytes.ToArray();
            this.Size = bytes.Count;
        }

        private void SortByIndex(ref List<Expression> constants)
        {
            constants = constants.OrderBy(value => value.Location.Index).ToList();
        }

        public byte[] Bytes
        {
            get;
            private set;
        }

        public int Size
        {
            get;
            private set;
        }

        public int DoubleCount
        {
            get;
            private set;
        }

        public int FloatCount
        {
            get;
            private set;
        }

        public int LongCount
        {
            get;
            private set;
        }

        public int IntCount
        {
            get;
            private set;
        }

        public int StringCount
        {
            get;
            private set;
        }

        public int WStringCount
        {
            get;
            private set;
        }
    }
}
