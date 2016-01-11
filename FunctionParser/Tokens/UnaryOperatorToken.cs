using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Markovcd.Classes
{
    public class UnaryOperatorToken : OperatorToken, IUnaryExpressionConstructor
    {
        public Func<Expression, UnaryExpression> ConstructExpression { get; }   

        public UnaryOperatorToken(string name, int index, char value, int precedence, Associativity associativity, Func<Expression, UnaryExpression> constructExpression)
            : base(name, index, value, precedence, associativity)
        {
            ConstructExpression = constructExpression;
        }

        public UnaryOperatorToken(string name, char rule, int precedence, Associativity associativity, Func<Expression, UnaryExpression> constructExpression)
           : base(name, rule, precedence, associativity)
        {
            ConstructExpression = constructExpression;
        }

        public override Token ToMatch(Match match)
            => new UnaryOperatorToken(Name, match.Index, match.Value[0], Precedence, Associativity, ConstructExpression);
    }
}
