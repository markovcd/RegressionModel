using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using Markovcd.Classes;

namespace Markovcd.Classes
{
    public class ModelBase
    {
       
        private static double ResidualSumOfSquares(Delegate func, IEnumerable<double> y, params IReadOnlyList<double>[] x)
        {
            var types = func.Method.GetParameters().Skip(1).Select(p => p.ParameterType);
            return
                y.Select((t, i) => Math.Pow(t - CalculateFunction<double>(func, x.Select(p => (object)p[i]), types), 2))
                    .Sum();
        }

        /*private static double TotalSumOfSquares<T>(IEnumerable<T> y, IFormatProvider formatProvider = null)
            where T : IConvertible
        {
            formatProvider = formatProvider ?? CultureInfo.InvariantCulture;
            return TotalSumOfSquares(y.Select(item => item.ToDouble(formatProvider)).ToList());
        }*/

        private static double TotalSumOfSquares(IReadOnlyCollection<double> y)
        {
            var mean = y.Sum(item => item) / y.Count;
            return y.Sum(d => Math.Pow(d - mean, 2));
        }

        public static double CalculateRSquared(Delegate func, IReadOnlyList<double> y, params IReadOnlyList<double>[] x)
            => 1 - ResidualSumOfSquares(func, y, x) / TotalSumOfSquares(y);

        /*public static LambdaExpression JoinFunction(LambdaExpression func, IEnumerable<double> coefficients)
            => JoinFunction(SplitAndCheckLambda(func), coefficients);*/

        public static LambdaExpression JoinFunction(IEnumerable<LambdaExpression> func, IEnumerable<double> coefficients)
            => func.Zip(coefficients, (f, d) =>
                       Expression.Lambda(
                           Expression.Multiply(f.Body,
                               Expression.Constant(d)), f.Parameters))
                   .Aggregate((total, curr) =>
                       Expression.Lambda(
                           Expression.Add(total.Body, curr.Body), total.Parameters));

        /*public static double Calculate(Delegate func, params double[] x)
            => (double)func.DynamicInvoke(x);

        private static double Calculate(IEnumerable<LambdaExpression> func, IEnumerable<double> coefficients, params double[] x)
            => Calculate(JoinFunction(func, coefficients), x);

        public static double Calculate(LambdaExpression func, IEnumerable<double> coefficients, params double[] x)
            => Calculate(SplitAndCheckLambda(func), coefficients, x);
*/
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

        /*private static double SumOfFunction(LambdaExpression func, IReadOnlyList<double> y, params IReadOnlyList<double>[] x)
        {
            var sum = 0d;
            var del = func.Compile();

            for (var i = 0; i < y.Count; i++)
            {
                var args = x.Select(p => p[i]).ToArray();
                sum += (double)del.DynamicInvoke(y[i], args);
            }

            return sum;
        }*/

        private static object ConvertParameterValue(object value, Type type)
        {
            if (value is IConvertible)
                return Convert.ChangeType(value, type);

            return value;
        }

        private static IEnumerable<object> ConvertParameterValues(IEnumerable<object> values,
            IEnumerable<Type> types) => values.Zip(types, ConvertParameterValue);

        private static T CalculateFunction<T>(Delegate d, IEnumerable<object> arguments, IEnumerable<Type> types)
        {
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
                var types = func.Parameters.Select(p => p.Type);
                //sum += (double)del.DynamicInvoke(args);
                sum += CalculateFunction<double>(del, args, types);
            }

            return sum;
        }
        /*
        private static double SumOfFunction(LambdaExpression func, params IReadOnlyList<double>[] x)
        {
            var sum = 0d;
            var del = func.Compile();

            for (var i = 0; i < x.First().Count; i++)
            {
                var args = x.Select(p => p[i]).ToArray();
                sum += (double)del.DynamicInvoke(args);
            }

            return sum;
        }
        */
        /*public static IReadOnlyList<double> GetCoefficients(LambdaExpression func, IReadOnlyList<object> y, params IReadOnlyList<object>[] x)
            => GetCoefficients(SplitAndCheckLambda(func).ToList(), y, x);*/

        /*protected static IReadOnlyList<double> GetCoefficients(IReadOnlyList<LambdaExpression> func, IReadOnlyList<double> y, params IReadOnlyList<double>[] x)
        {
            var c = new Matrix(func.Count);
            var a = new Matrix(func.Count, func.Count);

            for (var i = 0; i < a.Height; i++)
            {
                c[i] = SumOfFunction(MultiplyLambdaByY(func[i]), y, x);

                for (var j = 0; j < a.Width; j++)
                    a[i, j] = SumOfFunction(MultiplyLambdas(func[i], func[j]), x);
            }

            return (a.Invert() * c).GetColumn();
        }*/

        protected static IReadOnlyList<double> GetCoefficients(IReadOnlyList<LambdaExpression> func, IReadOnlyList<double> y, params IReadOnlyList<double>[] x)
        {
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
