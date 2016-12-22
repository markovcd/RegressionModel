using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Markovcd.Classes
{
    public class ModelBase
    {
       
        private static double ResidualSumOfSquares(Delegate func, IEnumerable<double> y, params IReadOnlyList<double>[] x) 
            => y.Select((t, i) => Math.Pow(t - CalculateFunction<double>(func, x.Select(p => (object)p[i])), 2)).Sum();

        private static double TotalSumOfSquares(IReadOnlyCollection<double> y)
        {
            var mean = y.Sum(item => item) / y.Count;
            return y.Sum(d => Math.Pow(d - mean, 2));
        }

        public static double CalculateRSquared(Delegate func, IReadOnlyList<double> y, params IReadOnlyList<double>[] x)
            => 1 - ResidualSumOfSquares(func, y, x) / TotalSumOfSquares(y);

        public static LambdaExpression JoinFunction(IEnumerable<LambdaExpression> func, IEnumerable<double> coefficients)
            => func.Zip(coefficients, (f, d) =>
                       Expression.Lambda(
                           Expression.Multiply(f.Body,
                               Expression.Constant(d)), f.Parameters))
                   .Aggregate((total, curr) =>
                       Expression.Lambda(
                           Expression.Add(total.Body, curr.Body), total.Parameters));

        protected static IEnumerable<LambdaExpression> SplitAndCheckLambda(LambdaExpression func)
        {
            yield return Expression.Lambda(Expression.Constant(1d), func.Parameters);
            foreach (var l in SplitLambda(func).Where(e => e.Body.NodeType != ExpressionType.Constant))
                yield return l;
        }

        private static IEnumerable<LambdaExpression> SplitLambda(LambdaExpression func)
        {
            var body = func.Body as BinaryExpression;

            if (body == null)
            {
                yield return Expression.Lambda(func.Body, func.Parameters);
            }
            else if (body.NodeType == ExpressionType.Add || body.NodeType == ExpressionType.AddChecked)
            {
                var left = Expression.Lambda(body.Left, func.Parameters);
                var right = Expression.Lambda(body.Right, func.Parameters);

                foreach (var e in SplitLambda(left)) yield return e;
                foreach (var e in SplitLambda(right)) yield return e;

            }
            else if (body.NodeType == ExpressionType.Subtract || body.NodeType == ExpressionType.SubtractChecked)
            {
                var left = Expression.Lambda(body.Left, func.Parameters);
                var right = Expression.Lambda(Expression.Negate(body.Right), func.Parameters);

                foreach (var e in SplitLambda(left)) yield return e;
                foreach (var e in SplitLambda(right)) yield return e;
            }
            else
            {
                yield return Expression.Lambda(body, func.Parameters);
            }
        }

        private static object ConvertParameterValue(object value, Type type)
        {
            if (value is IConvertible)
                return Convert.ChangeType(value, type);

            return value;
        }

        private static IEnumerable<Type> GetDelegateParameterTypes(Delegate d) 
            => d.Method.GetParameters().Skip(1).Select(p => p.ParameterType);

        private static IEnumerable<object> ConvertParameterValues(IEnumerable<object> values,
            IEnumerable<Type> types) => values.Zip(types, ConvertParameterValue);

        protected static T CalculateFunction<T>(Delegate d, IEnumerable<object> arguments)
        {
            var types = GetDelegateParameterTypes(d);
            var args = ConvertParameterValues(arguments, types).ToArray();
            
            return (T)d.DynamicInvoke(args);
        }

        private static dynamic SumOfFunction(LambdaExpression func, params IReadOnlyList<object>[] parameters)
        {
            if (!func.ReturnType.IsValueType) throw new InvalidOperationException("Function return type must be a value type.");
        
            var del = func.Compile();
            var sum = 0d;

            for (var i = 0; i < parameters[0].Count; i++)
            {
                var args = parameters.Select(arr => arr[i]);
                sum += CalculateFunction<double>(del, args);
            }

            return sum;
        }        

        protected static IReadOnlyList<double> GetCoefficients(IReadOnlyList<LambdaExpression> func, IReadOnlyList<double> y, params IReadOnlyList<double>[] x)
        {
            if (x.Any(l => l.Count != y.Count)) throw new Exception("Array lengths must be equal.");

            var c = new Matrix(func.Count);
            var a = new Matrix(func.Count, func.Count);
            var yType = y.GetType().GenericTypeArguments.First();

            for (var i = 0; i < a.Height; i++)
            {
                var concat = x.Concat(new[] {y}).Select(arr => arr.Cast<object>().ToList()).ToArray();
                c[i] = SumOfFunction(MultiplyLambdaByY(func[i], yType), concat);

                for (var j = 0; j < a.Width; j++)
                    a[i, j] = SumOfFunction(MultiplyLambdas(func[i], func[j]), x.Select(arr => arr.Cast<object>().ToList()).ToArray());
            }

            return (a.Invert() * c).GetColumn();
        }

        private static LambdaExpression MultiplyLambdas(LambdaExpression func1, LambdaExpression func2)
            => Expression.Lambda(Expression.Multiply(func1.Body, func2.Body), func1.Parameters);

        private static LambdaExpression MultiplyLambdaByY(LambdaExpression func, Type type)
        {
            var y = Expression.Parameter(type, "y");
            var body = Expression.Multiply(func.Body, y);
            return Expression.Lambda(body, func.Parameters.Concat(new[] {y}));
        }
    }
}
