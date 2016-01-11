
namespace Markovcd.Classes
{
    public abstract class NamedToken : Token
    {
        public NamedToken(string name)
            : base(name, $"\\b{name}\\b") { }

        public NamedToken(string name, int index)
            : base(name, index, name.Length, name) { }
    }
}
