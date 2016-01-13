
namespace Markovcd.Classes
{
    public abstract class NamedToken : Token
    {
        public NamedToken(string name)
            : base(name, $"\\b{name}\\b") 
        {
            Assert(name);
        }

        public NamedToken(string name, int index)
            : base(name, index, name.Length, name) 
        {
            Assert(name);
        }

        private static void Assert(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (!char.IsLetter(name[0])) throw new ArgumentException(nameof(name));
            if (name.Skip(1).Any(c => (c != '_') && !char.IsLetterOrDigit(c))) throw new ArgumentException(nameof(name));
        }
    }
}
