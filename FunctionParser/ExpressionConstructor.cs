using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Markovcd.Interfaces;

namespace Markovcd.Classes
{
    public class ExpressionConstructor
    {
        public LambdaExpression LambdaExpression { get; }
        public IEnumerable<ParameterExpression> Parameters { get; } 

        public ExpressionConstructor(Postfix postfix)
        {
            Parameters = postfix.Parameters.Select(p => p.ConstructExpression()).ToList();
            var body = FromPostfix(postfix, Parameters);
            LambdaExpression = Expression.Lambda(body, Parameters);
        }

        public static Expression FromPostfix(IEnumerable<Token> postfix, IEnumerable<ParameterExpression> parameters = null)
        {
            var stack = new Stack<Expression>();

            foreach (var token in postfix)
            {
                if (token is IParameterExpressionConstructor)
                {
                    var token2 = token as IParameterExpressionConstructor;
                    stack.Push(parameters == null ? token2.ConstructExpression() : token2.ConstructExpression(parameters));
                }
                else if (token is IExpressionConstructor)
                    stack.Push((token as IExpressionConstructor).ConstructExpression());
                else if (token is IBinaryExpressionConstructor)
                {
                    var args = PopArguments(stack, 2);
                    stack.Push((token as IBinaryExpressionConstructor).ConstructExpression(args[0], args[1]));
                }
                else if (token is IMethodExpressionConstructor)
                {
                    var argCount = (token as IMethodExpressionConstructor).ParameterCount;
                    stack.Push((token as IMethodExpressionConstructor).ConstructExpression(PopArguments(stack, argCount)));
                }
                else if (token is UnkownToken)
                    throw new InvalidOperationException($"Invalid token: \"{token.Value}\"");
                else
                    throw new InvalidOperationException();
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
