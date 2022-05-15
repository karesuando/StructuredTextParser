using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;

namespace STLang.ParserUtility
{
    public class StructDeclaration
    {
        public StructDeclaration()
        {
            this.members = new List<StructMemberDeclaration>();
        }

        public bool Add(StructMemberDeclaration member)
        {
            if (member == null)
                return true;
            else
            {
                bool multipleDecl;
                multipleDecl = this.members.Find(mem => string.Compare(mem.Name, member.Name, true) == 0) != null;
                if (multipleDecl)
                    return false;
                else
                {
                    this.members.Add(member);
                    return true;
                }
            }
        }

        public IEnumerable<StructMemberDeclaration> Members
        {
            get { return this.members; }
        }

        public int MemberCount
        {
            get { return this.members.Count; }
        }

        private readonly List<StructMemberDeclaration> members;
    }
}
