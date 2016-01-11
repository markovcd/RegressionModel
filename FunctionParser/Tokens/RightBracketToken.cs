
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

        public override Token MatchFromRule(int index, int length, string value)
            => new RightBracketToken(index);

        public static readonly RightBracketToken Default = new RightBracketToken();
    }
}
