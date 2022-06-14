using QUT.Gppg;
using STLang.Expressions;

namespace STLang.ParserUtility 
{ 
    public class NamedValue 
    {
        public NamedValue(string name, Expression value, LexLocation idLoc, LexLocation valueLoc)
        {
            this.Name = name;
            this.Value = value;
            this.IdLoc = idLoc;
            this.ValueLoc = valueLoc;
        }

        public string Name { get; private set; }
        public Expression Value { get; private set; }
        public LexLocation IdLoc { get; private set; }
        public LexLocation ValueLoc { get; private set; }
    }
}
