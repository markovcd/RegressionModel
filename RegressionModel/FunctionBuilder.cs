using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Markovcd.Classes
{
    public static class FunctionBuilder
    {
        /// <summary>
        /// Converts a parameter to array index call.
        /// </summary>
        /// <param name="oldParam">Parameter to convert.</param>
        /// <param name="newParam">Parameter of type array to place in array index expression.</param>
        /// <param name="parameters">Parameter list that contains "oldParam".</param>
        /// <returns>ArrayIndex expression with index of oldParam in parameters enumerable.</returns>
        private static BinaryExpression ToParams(this ParameterExpression oldParam, ParameterExpression newParam, IEnumerable<ParameterExpression> parameters)
            => Expression.ArrayIndex(newParam, Expression.Constant(parameters.Select((p, i) => new {p, i})
                                                                             .First(a => a.p.Equals(oldParam)).i));
        /// <summary>
        /// Finds all parameters in lambda expression and converts them to single parameter of type array. 
        /// </summary>
        /// <param name="func">Lambda expression to convert.</param>
        /// <example>Expression "(x1, x2, x3) => x1 * x2 + x3 / x1" becomes "(x) => x[0] * x[1] + x[2] / x[0]". </example>
        /// <returns>Lambda expression with single parameter of type array made from all parameters.</returns>
        public static Expression<Func<T[], T>> ToParams1<T>(this LambdaExpression func)
        {
            if (func.Parameters.Any(p => p.Type != typeof (T))) throw new InvalidOperationException();
            
            var parameter = Expression.Parameter(typeof (T[]));
            var body = ToParams(func.Body, func.Parameters, parameter);

            return Expression.Lambda<Func<T[], T>>(body, parameter);
        }

        /// <summary>
        /// Finds given parameters in an expression and converts them to given parameter array. 
        /// </summary>
        /// <param name="expression">Expression to convert.</param>
        /// <param name="parameters">Parameters to find.</param>
        /// <param name="newParam">Parameter of type array to place in an expression</param>
        /// <example>Expression "(x1, x2, x3) => x1 * x2 + x3 / x1" becomes "(x) => x[0] * x[1] + x[2] / x[0]". </example>
        /// <returns>Expression with single parameter of type array.</returns>
        private static Expression ToParams(Expression expression, IEnumerable<ParameterExpression> parameters,
            ParameterExpression newParam)
        {
            var method = expression as MethodCallExpression;
            var parameter = expression as ParameterExpression;
            var binary = expression as BinaryExpression;

            if (parameter != null)
                return parameter.ToParams(newParam, parameters);

            if (method != null)
                return Expression.Call(method.Method, 
                    method.Arguments.Select(a => ToParams(a, parameters, newParam)));
            
            if (binary == null)
                return expression;

            var left = ToParams(binary.Left, parameters, newParam);
            var right = ToParams(binary.Right, parameters, newParam);

            return binary.Update(left, binary.Conversion, right);
        }
    }
}
