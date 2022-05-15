using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;

namespace STLang.MemoryLayout
{
    public class StructLocation : MemoryLocation
    {
        public StructLocation()
            : base(-1, TypeNode.Error, -1)
        {
            this.memberLocation = new Dictionary<string, MemoryLocation>();
        }

        public void AddMember(string name, MemoryLocation memberLoc)
        {
            string key = name.ToUpper();
            memberLocation[key] = memberLoc;
        }

        private readonly Dictionary<string, MemoryLocation> memberLocation;
    }
}
