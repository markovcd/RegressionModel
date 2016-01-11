
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

        public override Token MatchFromRule(int index, int length, string value) 
            => new LeftBracketToken(index);

        public static readonly LeftBracketToken Default = new LeftBracketToken();
    }
}
