
using System.Text.RegularExpressions;
using Markovcd.Interfaces;

namespace Markovcd.Classes
{
    public class LeftBracketToken : OperatorToken
    {
        public LeftBracketToken(int index)
            : base(nameof(LeftBracketToken), index, '(', int.MaxValue, Associativity.Left)
        { }

        public LeftBracketToken() :
            base(nameof(LeftBracketToken), '(', int.MaxValue, Associativity.Left)
        { }

        public override Token ToMatch(Match match) 
            => new LeftBracketToken(match.Index);

        public static readonly LeftBracketToken Default = new LeftBracketToken();
    }
}
