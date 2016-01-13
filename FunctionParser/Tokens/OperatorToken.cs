using System;
using Markovcd.Interfaces;

namespace Markovcd.Classes
{
    public abstract class OperatorToken : Token, IOperator
    {
        public int Precedence { get; }
        public Associativity Associativity { get; }

        public OperatorToken(string name, int index, char value, int precedence, Associativity associativity)
            : base(name, index, 1, value.ToString())
        {
            Assert(value);
            Precedence = precedence;
            Associativity = associativity;
        }

        public OperatorToken(string name, char rule, int precedence, Associativity associativity)
            : base(name, $"\\{rule}")
        {
            Assert(rule);
            Precedence = precedence;
            Associativity = associativity;
        }

        private static void Assert(char name)
        {
            if (char.IsLetterOrDigit(name)) throw new ArgumentException();
        }
    }
}
