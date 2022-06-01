using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.POUDefinitions;

namespace STLang.Symbols
{
    public enum POUType : uint
    {
        FUNCTION = 0x2fc9a35d,
        FUNCTION_BLOCK = 0x5f3ec8a2,
        CLASS = 0xf1e7ba94,
        PROGRAM = 0x61ac50b3
    }
    public abstract class ProgramOrganizationUnitSymbol : STLangSymbol
    {
        public ProgramOrganizationUnitSymbol(string name, TypeNode dataType) 
            : base(name, dataType)
        {
            this.pouDefinitions = new List<ProgramOrganizationUnit>();
        }

        public IEnumerable<ProgramOrganizationUnit> Definitions
        {
            get { return this.pouDefinitions; }
        }

        public abstract POUType POUType { get; }

        protected readonly List<ProgramOrganizationUnit> pouDefinitions;
    }
}
