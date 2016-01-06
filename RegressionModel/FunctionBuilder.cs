using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Markovcd.Classes
{
    public static class FunctionBuilder
    {  
        public static BinaryExpression ToParams(this ParameterExpression oldParam, ParameterExpression newParam, IEnumerable<ParameterExpression> parameters)
            => Expression.ArrayIndex(newParam, Expression.Constant(parameters.Select((p, i) => new {p, i})
                                                                             .First(a => a.p.Equals(oldParam)).i));

        public static Expression<Func<T[], T>> ToParams<T>(this LambdaExpression func)
        {
            if (func.Parameters.Any(p => p.Type != typeof (T))) throw new InvalidOperationException();

            var parameter = Expression.Parameter(typeof (T[]));
            var body = ToParams(func.Body, func.Parameters, parameter);

            return Expression.Lambda<Func<T[], T>>(body, parameter);
        }

        private static Expression ToParams(Expression expression, IEnumerable<ParameterExpression> parameters,
            ParameterExpression newParam)
        {
            if (expression.NodeType == ExpressionType.Parameter)
                return (expression as ParameterExpression).ToParams(newParam, parameters);

            if (expression.NodeType == ExpressionType.Call)
            {
                var method = expression as MethodCallExpression;
                var arguments = method.Arguments.Select(a => ToParams(a, parameters, newParam));
                return Expression.Call(method.Method, arguments);
            }

            if (!(expression is BinaryExpression))
                return expression;

            var binary = (BinaryExpression) expression;
            var left = ToParams(binary.Left, parameters, newParam);
            var right = ToParams(binary.Right, parameters, newParam);

            return binary.Update(left, binary.Conversion, right);
        }
    }
}
