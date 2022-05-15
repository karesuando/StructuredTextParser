using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.POUDefinitions;

namespace STLang.Symbols
{
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

        protected readonly List<ProgramOrganizationUnit> pouDefinitions;
    }
}
