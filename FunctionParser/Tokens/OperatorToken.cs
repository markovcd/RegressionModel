using System;

namespace Markovcd.Classes
{
    public abstract class OperatorToken : Token, IOperator
    {
        public int Precedence { get; }
        public Associativity Associativity { get; }

        public OperatorToken(string name, int index, char value, int precedence, Associativity associativity)
            : base(name, index, 1, value.ToString())
        {
            AssertRule(value);
            Precedence = precedence;
            Associativity = associativity;
        }

        public OperatorToken(string name, char rule, int precedence, Associativity associativity)
            : base(name, $"\\{rule}")
        {
            AssertRule(rule);
            Precedence = precedence;
            Associativity = associativity;
        }

        private static void AssertRule(char rule)
        {
            if (char.IsLetterOrDigit(rule)) throw new ArgumentException();
        }

        public bool IsGreaterPrecedenceThan(IOperator o)
            => Precedence > o.Precedence;
    }
}
