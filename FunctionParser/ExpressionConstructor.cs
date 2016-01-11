using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Markovcd.Classes
{
    public class ExpressionConstructor
    {
        public LambdaExpression Expression { get; }

        public ExpressionConstructor(Postfix postfix)
        {
            var e = FromPostfix(postfix);
        }

        public static Expression FromPostfix(IEnumerable<Token> postfix)
        {
            var stack = new Stack<Expression>();

            foreach (var token in postfix)
            {
                if (token is IExpressionConstructor)
                    stack.Push((token as IExpressionConstructor).ConstructExpression);
                else if (token is IBinaryExpressionConstructor)
                {
                    var args = PopArguments(stack, 2);
                    stack.Push((token as IBinaryExpressionConstructor).ConstructExpression(args[0], args[1]));
                }
                else if (token is IMethodExpressionConstructor)
                {
                    var argCount = (token as IMethodExpressionConstructor).ParameterCount;
                    (token as IMethodExpressionConstructor).ConstructExpression(PopArguments(stack, argCount));
                }
                else if (token is UnkownToken) throw new InvalidOperationException($"Invalid token: \"{token.Value}\"");
                else throw new InvalidOperationException();
            }

            var result = stack.Pop();

            if (stack.Any()) throw new InvalidOperationException();

            return result;
        }

        private static IList<T> PopArguments<T>(Stack<T> stack, int count)
        {
            var args = new List<T>();

            while (count-- > 0) args.Insert(0, stack.Pop());
            return args;
        } 
    }
}
