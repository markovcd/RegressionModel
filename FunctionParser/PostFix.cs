using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Markovcd.Classes
{
    public class Postfix : IEnumerable<Token>
    {
        private readonly IEnumerable<Token> lastResult;

        public Postfix(Tokenizer tokens)
        {
            lastResult = FromInfix(tokens);
        }

        public static IEnumerable<Token> FromInfix(IEnumerable<Token> tokens)
        {
            var stack = new Stack<Token>();

            foreach (var token in tokens.SelectMany(token => FromInfix(token, stack)))
                yield return token;

            while (stack.Any())
            {
                if (stack.Peek() is LeftBracketToken) throw new InvalidOperationException();
                yield return stack.Pop();
            }
        }

        private static IEnumerable<Token> FromInfix(Token token, Stack<Token> stack)
        {
            if (token is FunctionToken || token is LeftBracketToken)
                stack.Push(token);
            else if (token is DoubleNumberToken || token is ParameterToken<double> || token is UnkownToken)
                yield return token;
            else if (token is ParameterSeparatorToken)
                foreach (var t in ProcessSeparator(stack))
                    yield return t;
            else if (token is BinaryOperatorToken)
                foreach (var t in ProcessOperator(token, stack))
                    yield return t;
            else if (token is RightBracketToken)
                foreach (var t in ProcessBracket(stack))
                    yield return t;
            else
                throw new InvalidOperationException();
        }

        private static IEnumerable<Token> ProcessSeparator(Stack<Token> stack)
        {
            while (stack.Any() && !(stack.Peek() is LeftBracketToken))
                yield return stack.Pop();

            if (!stack.Any()) throw new InvalidOperationException();
        } 

        private static IEnumerable<Token> ProcessOperator(Token token, Stack<Token> stack)
        {
            while (stack.Any() && (stack.Peek() is BinaryOperatorToken) && CompareOperators((BinaryOperatorToken)token, (BinaryOperatorToken)stack.Peek()))
                yield return stack.Pop();

            stack.Push(token);
        }

        private static IEnumerable<Token> ProcessBracket(Stack<Token> stack)
        {
            while (stack.Any() && !(stack.Peek() is LeftBracketToken))
                yield return stack.Pop();

            if (!stack.Any()) throw new InvalidOperationException();

            stack.Pop();

            if (stack.Any() && stack.Peek() is FunctionToken)
                yield return stack.Pop();
        } 

        private static bool CompareOperators(IOperator o1, IOperator o2) 
            => (o1.Associativity == Associativity.Left && o1.Precedence <= o2.Precedence) ||
               (o1.Associativity == Associativity.Right && o1.Precedence < o2.Precedence);

        public IEnumerator<Token> GetEnumerator() 
            => lastResult.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() 
            => GetEnumerator();
    }
}
