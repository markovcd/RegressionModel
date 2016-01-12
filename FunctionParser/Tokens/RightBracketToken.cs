
using System.Text.RegularExpressions;
using Markovcd.Interfaces;

namespace Markovcd.Classes
{
    public class RightBracketToken : OperatorToken
    {
        public RightBracketToken(int index)
            : base(typeof(RightBracketToken).Name, index, ')', int.MaxValue, Associativity.Right)
        { }

        public RightBracketToken()
            : base(typeof(RightBracketToken).Name, ')', int.MaxValue, Associativity.Right)
        { }

        public override Token ToMatch(Match match)
            => new RightBracketToken(match.Index);

        public static readonly RightBracketToken Default = new RightBracketToken();
    }
}
