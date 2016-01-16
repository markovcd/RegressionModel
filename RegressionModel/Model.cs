using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Markovcd.Classes
{
    public class Model<T, TResult> : Model<Func<T, TResult>>
        where T : IConvertible
        where TResult : IConvertible
    {
        public Model(Expression<Func<T, TResult>> func, IReadOnlyList<TResult> y, IReadOnlyList<T> x, IFormatProvider formatProvider = null)
            : base(func, Convert(y), Convert(x)) { }
    }

    public class Model<T1, T2, TResult> : Model<Func<T1, T2, TResult>>
        where T1 : IConvertible
        where T2 : IConvertible
        where TResult : IConvertible
    {
        public Model(Expression<Func<T1, T2, TResult>> func, IReadOnlyList<TResult> y, IReadOnlyList<T1> x1,
            IReadOnlyList<T2> x2)
            : base(func, Convert(y), Convert(x1), Convert(x2)) { }
    }

    public class Model<T1, T2, T3, TResult> : Model<Func<T1, T2, T3, TResult>>
        where T1 : IConvertible
        where T2 : IConvertible
        where T3 : IConvertible
        where TResult : IConvertible
    {
        public Model(Expression<Func<T1, T2, T3, TResult>> func, IReadOnlyList<TResult> y, IReadOnlyList<T1> x1,
            IReadOnlyList<T2> x2, IReadOnlyList<T3> x3)
            : base(func, Convert(y), Convert(x1), Convert(x2), Convert(x3)) { }
    }

    public class Model<T1, T2, T3, T4, TResult> : Model<Func<T1, T2, T3, T4, TResult>>
        where T1 : IConvertible
        where T2 : IConvertible
        where T3 : IConvertible
        where T4 : IConvertible
        where TResult : IConvertible
    {
        public Model(Expression<Func<T1, T2, T3, T4, TResult>> func, IReadOnlyList<TResult> y, IReadOnlyList<T1> x1,
            IReadOnlyList<T2> x2, IReadOnlyList<T3> x3, IReadOnlyList<T4> x4)
            : base(func, Convert(y), Convert(x1), Convert(x2), Convert(x3), Convert(x4)) { }
    }

    public class Model<TFunc> : Model where TFunc : class
    {
        public new Expression<TFunc> InputFunction => (Expression<TFunc>)base.InputFunction;
        public new Expression<TFunc> OutputFunction => (Expression<TFunc>)base.OutputFunction;

        public Model(Expression<TFunc> func, IReadOnlyList<double> y, params IReadOnlyList<double>[] x)
            : base(func, y, x) { }

        protected static double Convert<T>(T value, IFormatProvider formatProvider = null)
            where T : IConvertible
        {
            formatProvider = formatProvider ?? CultureInfo.InvariantCulture;
            return value.ToDouble(formatProvider);
        }

        protected static IReadOnlyList<double> Convert<T>(IReadOnlyList<T> values, IFormatProvider formatProvider = null)
            where T : IConvertible => values.Select(value => Convert(value, formatProvider)).ToList();
    }

    public class Model : ModelBase
    {
        protected readonly IReadOnlyList<LambdaExpression> SplitFunction;
        protected readonly IReadOnlyList<LambdaExpression> SplitOriginalFunction;
        protected readonly LambdaExpression ParamsFunction;
        protected readonly LambdaExpression OutputParamsFunction;
        
        public LambdaExpression InputFunction { get; }
        public LambdaExpression OutputFunction { get; }      

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
            
            ParamsFunction = func.ToParams<double>();
            SplitFunction = SplitAndCheckLambda(ParamsFunction).ToList();
            Coefficients = GetCoefficients(SplitFunction, y, x);
            
            OutputParamsFunction = JoinFunction(SplitFunction, Coefficients);
            RSquared = CalculateRSquared(OutputParamsFunction.Compile(), y, x);

            InputFunction = func;
            SplitOriginalFunction = SplitAndCheckLambda(InputFunction).ToList();
            OutputFunction = JoinFunction(SplitOriginalFunction, Coefficients);
        }

        public double Calculate(params double[] x) 
            => Calculate(OutputParamsFunction, x);

        private static string TrimParentheses(string func)
        {
            func = func.Trim();
            if (func[0] == '(' && func[func.Length - 1] == ')')
                func = func.Substring(1, func.Length - 2);

            return func;
        }

        public override string ToString()
        {
            var parameters = InputFunction.Parameters.Select(p => p.Name).Aggregate((s1, s2) => $"{s1}, {s2}");

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

            /*if (type.GetGenericArguments().Any(t => !t.Equals(doubl)))
                throw new ArgumentException("Generic arguments should be of type double.");*/
        }

        public static Model<T, TResult> Create<T, TResult>(Expression<Func<T, TResult>> func,
            IReadOnlyList<TResult> y, IReadOnlyList<T> x)
            where T : IConvertible
            where TResult : IConvertible
            => new Model<T, TResult>(func, y, x);

        public static Model<T1, T2, TResult> Create<T1, T2, TResult>(Expression<Func<T1, T2, TResult>> func,
            IReadOnlyList<TResult> y, IReadOnlyList<T1> x1, IReadOnlyList<T2> x2)
            where T1 : IConvertible
            where T2 : IConvertible
            where TResult : IConvertible
            => new Model<T1, T2, TResult>(func, y, x1, x2);

        public static Model<T1, T2, T3, TResult> Create<T1, T2, T3, TResult>(Expression<Func<T1, T2, T3, TResult>> func,
            IReadOnlyList<TResult> y, IReadOnlyList<T1> x1, IReadOnlyList<T2> x2, IReadOnlyList<T3> x3)
            where T1 : IConvertible
            where T2 : IConvertible
            where T3 : IConvertible
            where TResult : IConvertible
            => new Model<T1, T2, T3, TResult>(func, y, x1, x2, x3);

        public static Model<T1, T2, T3, T4, TResult> Create<T1, T2, T3, T4, TResult>(
            Expression<Func<T1, T2, T3, T4, TResult>> func,
            IReadOnlyList<TResult> y, IReadOnlyList<T1> x1, IReadOnlyList<T2> x2, IReadOnlyList<T3> x3,
            IReadOnlyList<T4> x4)
            where T1 : IConvertible
            where T2 : IConvertible
            where T3 : IConvertible
            where T4 : IConvertible
            where TResult : IConvertible
            => new Model<T1, T2, T3, T4, TResult>(func, y, x1, x2, x3, x4);
    }



    
}
