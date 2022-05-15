using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Extensions;
using STLang.ConstantTokens;
using STLang.VMInstructions;
using STLang.MemoryLayout;


namespace STLang.Expressions
{
    public class LRealConstant : Constant<double>
    {
        public LRealConstant(TokenDouble token)
            : base(TypeNode.LReal, token.Value, token.ToString())
        {
            double value = token.Value;
            this.isSmallConstant = value == -1.0 || value == 0.0 || value == 1.0;
        }

        public LRealConstant(double value)
            : base(TypeNode.LReal, value, "LREAL#" + value.ToString())
        {
            this.isSmallConstant = value == -1.0 || value == 0.0 || value == 1.0;
        }

        public LRealConstant(double value, string valueStr, TypeNode dataType)
            : base(dataType, value, valueStr)
        {
            this.isSmallConstant = value == -1.0 || value == 0.0 || value == 1.0;
        }

        public LRealConstant(double value, string strValue)
            : base(TypeNode.LReal, value, strValue)
        {
            this.isSmallConstant = value == -1.0 || value == 0.0 || value == 1.0;
        }

        static LRealConstant()
        {
            registerConst = new List<double>();
        }

        public override bool IsZero
        {
            get { return this.Value == 0.0d; }
        }

        public override bool IsSmallConstant
        {
            get { return this.isSmallConstant; }
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            if (this.Value == -1.0d)
                this.StoreInstruction(VirtualMachineInstruction.DCONST_N1);
            else if (this.Value == 0.0d)
                this.StoreInstruction(VirtualMachineInstruction.DCONST_0);
            else if (this.Value == 1.0d)
                this.StoreInstruction(VirtualMachineInstruction.DCONST_1);
            else
            {
                if (this.Location == MemoryLocation.UndefinedLocation)
                {
                    this.Location = this.GetConstantIndex();
                    if (this.Location.IsRegisterConstant)
                        registerConst.Add(this.Value);
                }
                int index = this.Location.Index;
                if (!this.Location.IsRegisterConstant)
                    this.StoreInstruction(VirtualMachineInstruction.DCONST, index);
                else if (index == 0)
                    this.StoreInstruction(VirtualMachineInstruction.DCONST0);
                else if (index == 1)
                    this.StoreInstruction(VirtualMachineInstruction.DCONST1);
                else if (index == 2)
                    this.StoreInstruction(VirtualMachineInstruction.DCONST2);
                else if (index == 3)
                    this.StoreInstruction(VirtualMachineInstruction.DCONST3);
              
            }
        }

        public static int GetRegisterConstants(ref double d0, ref double d1, ref double d2, ref double d3)
        {
            int count = registerConst.Count;
            if (count == 0)
                return 0;
            else if (count == 1)
                d0 = registerConst[0];
            else if (count == 2)
            {
                d0 = registerConst[0];
                d1 = registerConst[1];
            }
            else if (count == 3)
            {
                d0 = registerConst[0];
                d1 = registerConst[1];
                d2 = registerConst[2];
            }
            else
            {
                d0 = registerConst[0];
                d1 = registerConst[1];
                d2 = registerConst[2];
                d3 = registerConst[3];
            }
            registerConst.Clear();
            return count;
        }

        public override byte[] GetBytes()
        {
            return this.Value.GetBytes();
        }

        private readonly bool isSmallConstant;

        private static readonly List<double> registerConst;
    }
}
