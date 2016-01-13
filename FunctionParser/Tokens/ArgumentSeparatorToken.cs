using System.Text.RegularExpressions;
using Markovcd.Interfaces;

namespace Markovcd.Classes
{
    class ArgumentSeparatorToken : OperatorToken
    {
        public ArgumentSeparatorToken(int index)
            : base(nameof(ArgumentSeparatorToken), index, ',', int.MinValue, Associativity.None)
        { }

        public ArgumentSeparatorToken() :
            base(nameof(ArgumentSeparatorToken), ',', int.MinValue, Associativity.None)
        { }

        public override Token ToMatch(Match match)
            => new ArgumentSeparatorToken(match.Index);

        public static readonly ArgumentSeparatorToken Default = new ArgumentSeparatorToken();
    }
}
