using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace Markovcd.Classes
{
    public class Model
    {
        public LambdaExpression Function { get; }
        public IReadOnlyList<LambdaExpression> SplitFunction { get; } 
        public Delegate CompiledFunction { get; }

        public IReadOnlyList<IReadOnlyList<double>> X { get; }
        public IReadOnlyList<double> Y { get; }
        public IReadOnlyList<double> Coefficients { get; }
        public double RSquared { get; }
        public int ParameterCount { get; }

        public Model(LambdaExpression func, IReadOnlyList<double> y, params IReadOnlyList<double>[] x)
        {
            X = x;
            Y = y;
            ParameterCount = x.Length;
            Function = func;
            SplitFunction = SplitAndCheckLambda(func).ToList();
            
            Coefficients = GetCoefficients(SplitFunction, y, x);
            
            CompiledFunction = Compile(SplitFunction, Coefficients);
            RSquared = CalculateRSquared(CompiledFunction, y, x);
        }

        public double Calculate(params double[] x) => Calculate(CompiledFunction, x);

        private static double ResidualSumOfSquares(Delegate func, IEnumerable<double> y, params IReadOnlyList<double>[] x) 
            => y.Select((t, i) => Math.Pow(t - (double) func.DynamicInvoke(x.Select(p => p[i]).ToArray()), 2)).Sum();

        private static double TotalSumOfSquares(IReadOnlyList<double> y)
        {
            var mean = y.Sum() / y.Count;
            return y.Sum(d => Math.Pow(d - mean, 2));
        }

        public static double CalculateRSquared(Delegate func, IReadOnlyList<double> y, params IReadOnlyList<double>[] x)
            => 1 - ResidualSumOfSquares(func, y, x) / TotalSumOfSquares(y);

        public static Delegate Compile(LambdaExpression func, IEnumerable<double> coefficients)
            => Compile(SplitAndCheckLambda(func), coefficients);

        private static Delegate Compile(IEnumerable<LambdaExpression> func, IEnumerable<double> coefficients) 
            => func.Zip(coefficients, (f, d) =>
                       Expression.Lambda(
                           Expression.Multiply(f.Body,
                               Expression.Constant(d)), f.Parameters))
                   .Aggregate((total, curr) =>
                       Expression.Lambda(
                           Expression.Add(total.Body, curr.Body), total.Parameters))
                   .Compile();
   
        public static double Calculate(Delegate func, params double[] x)
            => (double)func.DynamicInvoke(x);

        private static double Calculate(IEnumerable<LambdaExpression> func, IEnumerable<double> coefficients, params double[] x)
            => Calculate(Compile(func, coefficients), x);

        public static double Calculate(LambdaExpression func, IEnumerable<double> coefficients, params double[] x)
            => Calculate(SplitAndCheckLambda(func), coefficients, x);

        private static IEnumerable<LambdaExpression> SplitAndCheckLambda(LambdaExpression func)
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

        private static double SumOfFunction(LambdaExpression func, IReadOnlyList<double> y, params IReadOnlyList<double>[] x)
        {
            var sum = 0d;
            var del = func.Compile();
           
            for (var i = 0; i < y.Count; i++)
            {
                var args = x.Select(p => p[i]).ToArray();
                sum += (double) del.DynamicInvoke(y[i], args);
            }

            return sum;
        }

        private static double SumOfFunction(LambdaExpression func, params IReadOnlyList<double>[] x)
        {
            var sum = 0d;
            var del = func.Compile();

            for (var i = 0; i < x.First().Count; i++)
            {
                var args = x.Select(p => p[i]).ToArray();
                sum += (double) del.DynamicInvoke(args);
            }

            return sum;
        }

        public static IReadOnlyList<double> GetCoefficients(LambdaExpression func, IReadOnlyList<double> y, params IReadOnlyList<double>[] x) 
            => GetCoefficients(SplitAndCheckLambda(func).ToList(), y, x);

        private static IReadOnlyList<double> GetCoefficients(IReadOnlyList<LambdaExpression> func, IReadOnlyList<double> y, params IReadOnlyList<double>[] x)
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
        }

        private static LambdaExpression MultiplyLambdas(LambdaExpression func1, LambdaExpression func2) 
            => Expression.Lambda(Expression.Multiply(func1.Body, func2.Body), func1.Parameters);

        private static LambdaExpression MultiplyLambdaByY(LambdaExpression func)
        {
            var y = Expression.Parameter(typeof (double), "y");
            var body = Expression.Multiply(func.Body, y);
            var parameters = func.Parameters.ToList();
            parameters.Insert(0, y);
            return Expression.Lambda(body, parameters);
        }
    }
}
