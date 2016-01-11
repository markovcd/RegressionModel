using System;

namespace Markovcd.Classes
{
    public class UnaryOperatorToken<T, TResult> : OperatorToken
    {
        public Func<T, TResult> Function { get; }

        public UnaryOperatorToken(string name, int index, char value, int precedence, Associativity associativity, Func<T, TResult> function)
            : base(name, index, value, precedence, associativity)
        {
            Function = function;
        }

        public UnaryOperatorToken(string name, char rule, int precedence, Associativity associativity, Func<T, TResult> function)
           : base(name, rule, precedence, associativity)
        {
            Function = function;
        }

        public override Token MatchFromRule(int index, int length, string value) 
            => new UnaryOperatorToken<T, TResult>(Name, index, value[0], Precedence, Associativity, Function);
    }
}
