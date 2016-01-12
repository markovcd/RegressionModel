using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Markovcd.Interfaces;

namespace Markovcd.Classes
{
    public class BinaryOperatorToken : OperatorToken, IBinaryExpressionConstructor
    {
        public virtual Func<Expression, Expression, BinaryExpression> ConstructExpression { get; }

        public BinaryOperatorToken(string name, int index, char value, int precedence, Associativity associativity, Func<Expression, Expression, BinaryExpression> constructExpression)
            : base(name, index, value, precedence, associativity)
        {
            ConstructExpression = constructExpression;
        }

        public BinaryOperatorToken(string name, char rule, int precedence, Associativity associativity, Func<Expression, Expression, BinaryExpression> constructExpression)
           : base(name, rule, precedence, associativity)
        {
            ConstructExpression = constructExpression;
        }

        public override Token ToMatch(Match match) 
            => new BinaryOperatorToken(Name, match.Index, match.Value[0], Precedence, Associativity, ConstructExpression);

        public static readonly BinaryOperatorToken Addition = new BinaryOperatorToken(nameof(Addition), '+', 0, Associativity.Left, Expression.Add);
        public static readonly BinaryOperatorToken Subtraction = new BinaryOperatorToken(nameof(Subtraction), '-', 0, Associativity.Left, Expression.Subtract);
        public static readonly BinaryOperatorToken Multiplication = new BinaryOperatorToken(nameof(Multiplication), '*', 1, Associativity.Left, Expression.Multiply);
        public static readonly BinaryOperatorToken Division = new BinaryOperatorToken(nameof(Division), '/', 1, Associativity.Left, Expression.Divide);
        public static readonly BinaryOperatorToken Power = new BinaryOperatorToken(nameof(Power), '^', 2, Associativity.Right, Expression.Power);
    }
}
