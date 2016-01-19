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
            Parameters = postfix.Parameters.Select(p => p.ConstructExpression())
                                           .Cast<ParameterExpression>()
                                           .ToList();

            var body = FromPostfix(postfix, Parameters);
            LambdaExpression = Expression.Lambda(body, Parameters);
        }

        public static Expression FromPostfix(IEnumerable<Token> postfix, IEnumerable<ParameterExpression> parameters = null)
        {
            var stack = new Stack<Expression>();

            foreach (var expression in postfix.Select(token => ConstructExpression(stack, token, parameters)))
                stack.Push(expression);

            var result = stack.Pop();

            if (stack.Any()) throw new InvalidOperationException();

            return result;
        }

        private static Expression ConstructExpression(Stack<Expression> stack, Token token, IEnumerable<ParameterExpression> parameters = null)
        {
            Expression expression;

            var parameter = token as IParameterExpressionConstructor;
            var other = token as IExpressionConstructor;
            var binary = token as IBinaryExpressionConstructor;
            var method = token as IMethodExpressionConstructor;

            if (parameter != null)
                expression = parameters == null ? parameter.ConstructExpression() : parameter.ConstructExpression(parameters);
            else if (binary != null)
            {
                var args = PopArguments(stack, 2);
                expression = binary.ConstructExpression(args[0], args[1]);
            }
            else if (method != null)
            {
                var args = PopArguments(stack, method.ParameterCount);
                expression = method.ConstructExpression(args);
            }
            else if (other != null)
                expression = other.ConstructExpression();
            else if (token is UnkownToken)
                throw new InvalidOperationException($"Invalid token: \"{token.Value}\"");
            else
                throw new InvalidOperationException();

            return expression;
        }

        private static IList<T> PopArguments<T>(Stack<T> stack, int count)
        {
            var args = new List<T>();
            while (count-- > 0) args.Insert(0, stack.Pop());
            return args;
        } 
    }
}
