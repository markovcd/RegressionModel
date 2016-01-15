using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Markovcd.Classes
{
    public class Model<T, TResult> : Model<Func<T, TResult>>
        where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
        where TResult : struct, IComparable, IFormattable, IConvertible, IComparable<TResult>, IEquatable<TResult>
    {
        public Model(Expression<Func<T, TResult>> func, IReadOnlyList<double> y, IReadOnlyList<double> x)
            : base(func, y, x) { }
    }

    public class Model<T1, T2, TResult> : Model<Func<T1, T2, TResult>>
        where T1 : struct, IComparable, IFormattable, IConvertible, IComparable<T1>, IEquatable<T1>
        where T2 : struct, IComparable, IFormattable, IConvertible, IComparable<T2>, IEquatable<T2>
        where TResult : struct, IComparable, IFormattable, IConvertible, IComparable<TResult>, IEquatable<TResult>
    {
        public Model(Expression<Func<T1, T2, TResult>> func, IReadOnlyList<double> y, IReadOnlyList<double> x1,
            IReadOnlyList<double> x2)
            : base(func, y, x1, x2) { }
    }

    public class Model<T1, T2, T3, TResult> : Model<Func<T1, T2, T3, TResult>>
        where T1 : struct, IComparable, IFormattable, IConvertible, IComparable<T1>, IEquatable<T1>
        where T2 : struct, IComparable, IFormattable, IConvertible, IComparable<T2>, IEquatable<T2>
        where T3 : struct, IComparable, IFormattable, IConvertible, IComparable<T3>, IEquatable<T3>
        where TResult : struct, IComparable, IFormattable, IConvertible, IComparable<TResult>, IEquatable<TResult>
    {
        public Model(Expression<Func<T1, T2, T3, TResult>> func, IReadOnlyList<double> y, IReadOnlyList<double> x1,
            IReadOnlyList<double> x2, IReadOnlyList<double> x3)
            : base(func, y, x1, x2, x3) { }
    }

    public class Model<T1, T2, T3, T4, TResult> : Model<Func<T1, T2, T3, T4, TResult>>
        where T1 : struct, IComparable, IFormattable, IConvertible, IComparable<T1>, IEquatable<T1>
        where T2 : struct, IComparable, IFormattable, IConvertible, IComparable<T2>, IEquatable<T2>
        where T3 : struct, IComparable, IFormattable, IConvertible, IComparable<T3>, IEquatable<T3>
        where T4 : struct, IComparable, IFormattable, IConvertible, IComparable<T4>, IEquatable<T4>
        where TResult : struct, IComparable, IFormattable, IConvertible, IComparable<TResult>, IEquatable<TResult>
    {
        public Model(Expression<Func<T1, T2, T3, T4, TResult>> func, IReadOnlyList<double> y, IReadOnlyList<double> x1,
            IReadOnlyList<double> x2, IReadOnlyList<double> x3, IReadOnlyList<double> x4)
            : base(func, y, x1, x2, x3, x4) { }
    }

    public class Model<TFunc> : Model where TFunc : class
    {
        public new Expression<TFunc> OriginalFunction 
            => (Expression<TFunc>)base.OriginalFunction;

        public new IReadOnlyList<Expression<TFunc>> SplitOriginalFunction
            => base.SplitOriginalFunction.Cast<Expression<TFunc>>().ToList();

        public new Expression<TFunc> Function
            => (Expression<TFunc>)base.Function;

        public new IReadOnlyList<Expression<TFunc>> SplitFunction
            => base.SplitFunction.Cast<Expression<TFunc>>().ToList();

        public new TFunc CompiledFunction => (TFunc)(object)base.CompiledFunction;

        public Model(Expression<TFunc> func, IReadOnlyList<double> y, params IReadOnlyList<double>[] x)
            : base(func, y, x) { }
    }

    public class Model : ModelBase
    {      
        public LambdaExpression OriginalFunction { get; }
        public IReadOnlyList<LambdaExpression> SplitOriginalFunction { get; }
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
            Assert(func.Type);

            X = x;
            Y = y;
            ParameterCount = x.Length;
            OriginalFunction = func;
            SplitOriginalFunction = SplitAndCheckLambda(OriginalFunction).ToList();
            Function = func.ToParams<double>();
            SplitFunction = SplitAndCheckLambda(Function).ToList();
            Coefficients = GetCoefficients(SplitFunction, y, x);
            
            CompiledFunction = Compile(SplitFunction, Coefficients);
            RSquared = CalculateRSquared(CompiledFunction, y, x);
        }

        public double Calculate(params double[] x) 
            => Calculate(CompiledFunction, x);

        private static string TrimParentheses(string func)
        {
            func = func.Trim();
            if (func[0] == '(' && func[func.Length - 1] == ')')
                func = func.Substring(1, func.Length - 2);

            return func;
        }

        public override string ToString()
        {
            var parameters = OriginalFunction.Parameters.Select(p => p.Name).Aggregate((s1, s2) => $"{s1}, {s2}");

            var terms = SplitOriginalFunction.Select(f => f.Body.ToString())
                                             .Select(TrimParentheses)
                                             .Select(s => new string(s.Where(c => !char.IsWhiteSpace(c)).ToArray()))
                                             .ToArray();


            var coeffs = Coefficients.Select((d, i) => $"b{i + 1} = {d}").Aggregate((s1, s2) => $"{s1}\n{s2}");
            var b = Coefficients.Select((d, i) => $"b{i + 1}"); 

            var originalFunc = terms.Zip(b, (s, d) => s.Equals("1") ? $"{d}" : $"{d}*{s}")
                                    .Aggregate((s1, s2) => $"{s1} + {s2}");

            var func = terms.Zip(Coefficients, (s, d) => s.Equals("1") ? $"{d:0.000}" : $"{d:0.000}*{s}")
                            .Aggregate((s1, s2) => $"{s1} + {s2}");

            var sb = new StringBuilder();

            sb.AppendLine("Given function:");
            sb.AppendLine($"f({parameters}) = {originalFunc}");
            sb.AppendLine();
            sb.AppendLine("Coefficients:");
            sb.AppendLine(coeffs);
            sb.AppendLine();
            sb.AppendLine($"R squared: {RSquared}");
            sb.AppendLine();
            sb.AppendLine("Resulting function:");
            sb.Append($"f({parameters}) = {func}");

            return sb.ToString();
        }

        private static void Assert(Type type)
        {
            var doubl = typeof(double);

            if (!type.IsSubclassOf(typeof(Delegate)) || !type.Name.Contains("Func`"))
                throw new InvalidOperationException(type.Name + " is not a Func<> type");

            if (type.GetGenericArguments().Any(t => !t.Equals(doubl)))
                throw new ArgumentException("Generic arguments should be of type double.");
        }

        public static Model<double, double> One(Expression<Func<double, double>> func,
            IReadOnlyList<double> y, IReadOnlyList<double> x)
            => new Model<double, double>(func, y, x);

        public static Model<double, double, double> Two(Expression<Func<double, double, double>> func,
            IReadOnlyList<double> y, IReadOnlyList<double> x1, IReadOnlyList<double> x2)
            => new Model<double, double, double>(func, y, x1, x2);

        public static Model<double, double, double, double> Three(Expression<Func<double, double, double, double>> func,
            IReadOnlyList<double> y, IReadOnlyList<double> x1, IReadOnlyList<double> x2, IReadOnlyList<double> x3)
            => new Model<double, double, double, double>(func, y, x1, x2, x3);

        public static Model<double, double, double, double, double> Four(
            Expression<Func<double, double, double, double, double>> func,
            IReadOnlyList<double> y, IReadOnlyList<double> x1, IReadOnlyList<double> x2, IReadOnlyList<double> x3,
            IReadOnlyList<double> x4)
            => new Model<double, double, double, double, double>(func, y, x1, x2, x3, x4);
    }



    
}
