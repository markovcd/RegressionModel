using System.Text.RegularExpressions;

namespace Markovcd.Classes
{
    public abstract class Token
    {
        public string Name { get; }
        public string Rule { get; }
        public int Index { get; }
        public int Length { get; }
        public string Value { get; }

        public Token(string name, int index, int length, string value)
        {
            Name = name;
            Index = index;
            Length = length;
            Value = value;
        }

        public Token(string name, string rule)
        {
            Name = name;
            Rule = rule;
        }

        public virtual string ToRegularExpression()
            => $"(?<{Name}>{Rule})";

        public abstract Token ToMatch(Match match);

        public override int GetHashCode()
            => $"token: {Name}".GetHashCode();
    }
}
