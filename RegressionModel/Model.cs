﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace Markovcd.Classes
{
    public sealed class Model<T, TResult> : Model<Func<T, TResult>>
        where T : IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
        where TResult : IComparable, IFormattable, IConvertible, IComparable<TResult>, IEquatable<TResult>
    {
        public Model(Expression<Func<T, TResult>> func, IReadOnlyList<TResult> y, IReadOnlyList<T> x, IFormatProvider formatProvider = null)
            : base(func, Convert(y, formatProvider), Convert(x, formatProvider))
        { }

        public TResult CalculateFunction(T x1)
            => CalculateFunction<TResult>(OutputFunction.Compile(), new object[] { x1 });
    }

    public sealed class Model<T1, T2, TResult> : Model<Func<T1, T2, TResult>>
        where T1 : IComparable, IFormattable, IConvertible, IComparable<T1>, IEquatable<T1>
        where T2 : IComparable, IFormattable, IConvertible, IComparable<T2>, IEquatable<T2>
        where TResult : IComparable, IFormattable, IConvertible, IComparable<TResult>, IEquatable<TResult>
    {
        public Model(Expression<Func<T1, T2, TResult>> func, IReadOnlyList<TResult> y, IReadOnlyList<T1> x1,
            IReadOnlyList<T2> x2, IFormatProvider formatProvider = null)
            : base(func, Convert(y, formatProvider), Convert(x1, formatProvider), Convert(x2, formatProvider))
        { }

        public TResult CalculateFunction(T1 x1, T2 x2)
            => CalculateFunction<TResult>(OutputFunction.Compile(), new object[] { x1, x2 });
    }

    public sealed class Model<T1, T2, T3, TResult> : Model<Func<T1, T2, T3, TResult>>
        where T1 : IComparable, IFormattable, IConvertible, IComparable<T1>, IEquatable<T1>
        where T2 : IComparable, IFormattable, IConvertible, IComparable<T2>, IEquatable<T2>
        where T3 : IComparable, IFormattable, IConvertible, IComparable<T3>, IEquatable<T3>
        where TResult : IComparable, IFormattable, IConvertible, IComparable<TResult>, IEquatable<TResult>
    {
        public Model(Expression<Func<T1, T2, T3, TResult>> func, IReadOnlyList<TResult> y, IReadOnlyList<T1> x1,
            IReadOnlyList<T2> x2, IReadOnlyList<T3> x3, IFormatProvider formatProvider = null)
            : base(
                func, Convert(y, formatProvider), Convert(x1, formatProvider), Convert(x2, formatProvider),
                Convert(x3, formatProvider))
        { }

        public TResult CalculateFunction(T1 x1, T2 x2, T3 x3) 
            => CalculateFunction<TResult>(OutputFunction.Compile(), new object[] {x1, x2, x3});
    }

    public sealed class Model<T1, T2, T3, T4, TResult> : Model<Func<T1, T2, T3, T4, TResult>>
        where T1 : IComparable, IFormattable, IConvertible, IComparable<T1>, IEquatable<T1>
        where T2 : IComparable, IFormattable, IConvertible, IComparable<T2>, IEquatable<T2>
        where T3 : IComparable, IFormattable, IConvertible, IComparable<T3>, IEquatable<T3>
        where T4 : IComparable, IFormattable, IConvertible, IComparable<T4>, IEquatable<T4>
        where TResult : IComparable, IFormattable, IConvertible, IComparable<TResult>, IEquatable<TResult>
    {
        public Model(Expression<Func<T1, T2, T3, T4, TResult>> func, IReadOnlyList<TResult> y, IReadOnlyList<T1> x1,
            IReadOnlyList<T2> x2, IReadOnlyList<T3> x3, IReadOnlyList<T4> x4, IFormatProvider formatProvider = null)
            : base(
                func, Convert(y, formatProvider), Convert(x1, formatProvider), Convert(x2, formatProvider),
                Convert(x3, formatProvider), Convert(x4, formatProvider))
        { }

        public TResult CalculateFunction(T1 x1, T2 x2, T3 x3, T4 x4)
            => CalculateFunction<TResult>(OutputFunction.Compile(), new object[] { x1, x2, x3, x4 });
    }

    public class Model<TFunc> : Model where TFunc : class
    {
        public new Expression<TFunc> InputFunction => (Expression<TFunc>)base.InputFunction;
        public new Expression<TFunc> OutputFunction => (Expression<TFunc>)base.OutputFunction;

        public Model(Expression<TFunc> func, IReadOnlyList<double> y, params IReadOnlyList<double>[] x)
            : base(func, y, x) { }

        protected static IReadOnlyList<TResult> Convert<T, TResult>(IReadOnlyList<T> values, IFormatProvider formatProvider = null)
            where T : IConvertible 
            => values.Select(value => (TResult)System.Convert.ChangeType(value, typeof(TResult), formatProvider ?? CultureInfo.InvariantCulture)).ToList();

        protected static IReadOnlyList<double> Convert<T>(IReadOnlyList<T> values, IFormatProvider formatProvider = null)
            where T : IConvertible => Convert<T, double>(values, formatProvider);
    }

    public class Model : ModelBase
    {
        private readonly IReadOnlyList<LambdaExpression> splitFunction;
        
        public LambdaExpression InputFunction { get; }
        public LambdaExpression OutputFunction { get; }      

        public IReadOnlyList<IReadOnlyList<double>> X { get; }
        public IReadOnlyList<double> Y { get; }
        public IReadOnlyList<double> Coefficients { get; }
        public double RSquared { get; }
        public int ParameterCount => X.Count;

        public Model(LambdaExpression func, IReadOnlyList<double> y, params IReadOnlyList<double>[] x)
        {
            Assert(func.Type);

            X = x;
            Y = y;
            InputFunction = func;

            splitFunction = SplitAndCheckLambda(InputFunction).ToList();
            Coefficients = GetCoefficients(splitFunction, y, x);
            
            splitFunction = SplitAndCheckLambda(InputFunction).ToList();
            OutputFunction = JoinFunction(splitFunction, Coefficients);
            RSquared = CalculateRSquared(OutputFunction.Compile(), y, x);
        }

        private static string TrimParentheses(string func)
        {
            func = func.Trim();
            if (func[0] == '(' && func[func.Length - 1] == ')')
                func = func.Substring(1, func.Length - 2);

            return func;
        }

        public FunctionStringClass FunctionString => new FunctionStringClass(this);

        public sealed class FunctionStringClass
        {
            private readonly Model parent;

            internal FunctionStringClass(Model parent)
            {
                this.parent = parent;
            }

            private string Parameters
                => parent.InputFunction.Parameters.Select(p => p.Name).Aggregate((s1, s2) => $"{s1}, {s2}");

            private string[] Terms
                => parent.splitFunction.Select(f => f.Body.ToString())
                                       .Select(TrimParentheses)
                                       .Select(s => new string(s.Where(c => !char.IsWhiteSpace(c)).ToArray()))
                                       .ToArray();

            public string Parametrized 
            {
                get
                {
                    var b = parent.Coefficients.Select((d, i) => $"b{i + 1}");

                    var originalFunc = Terms.Zip(b, (s, d) => s.Equals("1") ? $"{d}" : $"{d}*{s}")
                                    .Aggregate((s1, s2) => $"{s1} + {s2}");

                    return $"f({Parameters}) = {originalFunc}";
                }
            }

            public string Final
            {
                get
                {
                    var func = Terms.Zip(parent.Coefficients, (s, d) => s.Equals("1") ? $"{d:0.000}" : $"{d:0.000}*{s}")
                            .Aggregate((s1, s2) => $"{s1} + {s2}");

                    return $"f({Parameters}) = {func}";
                }
            }

            public string Coefficients
                => parent.Coefficients.Select((d, i) => $"b{i + 1} = {d}").Aggregate((s1, s2) => $"{s1}\n{s2}");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Given function:");
            sb.AppendLine(FunctionString.Parametrized);
            sb.AppendLine();
            sb.AppendLine("Coefficients:");
            sb.AppendLine(FunctionString.Coefficients);
            sb.AppendLine();
            sb.AppendLine($"R squared: {RSquared}");
            sb.AppendLine();
            sb.AppendLine("Resulting function:");
            sb.Append(FunctionString.Final);

            return sb.ToString();
        }

        private static void Assert(Type type)
        {
            if (!type.IsSubclassOf(typeof(Delegate)) || !type.Name.Contains("Func`"))
                throw new InvalidOperationException(type.Name + " is not a Func<> type");
        }

        public static Model<T, TResult> Create<T, TResult>(Expression<Func<T, TResult>> func,
            IReadOnlyList<TResult> y, IReadOnlyList<T> x, IFormatProvider formatProvider = null)
            where T : IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
            where TResult : IComparable, IFormattable, IConvertible, IComparable<TResult>, IEquatable<TResult>
            => new Model<T, TResult>(func, y, x, formatProvider);

        public static Model<T1, T2, TResult> Create<T1, T2, TResult>(Expression<Func<T1, T2, TResult>> func,
            IReadOnlyList<TResult> y, IReadOnlyList<T1> x1, IReadOnlyList<T2> x2, IFormatProvider formatProvider = null)
            where T1 : IComparable, IFormattable, IConvertible, IComparable<T1>, IEquatable<T1>
            where T2 : IComparable, IFormattable, IConvertible, IComparable<T2>, IEquatable<T2>
            where TResult : IComparable, IFormattable, IConvertible, IComparable<TResult>, IEquatable<TResult>
            => new Model<T1, T2, TResult>(func, y, x1, x2, formatProvider);

        public static Model<T1, T2, T3, TResult> Create<T1, T2, T3, TResult>(Expression<Func<T1, T2, T3, TResult>> func,
            IReadOnlyList<TResult> y, IReadOnlyList<T1> x1, IReadOnlyList<T2> x2, IReadOnlyList<T3> x3,
            IFormatProvider formatProvider = null)
            where T1 : IComparable, IFormattable, IConvertible, IComparable<T1>, IEquatable<T1>
            where T2 : IComparable, IFormattable, IConvertible, IComparable<T2>, IEquatable<T2>
            where T3 : IComparable, IFormattable, IConvertible, IComparable<T3>, IEquatable<T3>
            where TResult : IComparable, IFormattable, IConvertible, IComparable<TResult>, IEquatable<TResult>
            => new Model<T1, T2, T3, TResult>(func, y, x1, x2, x3, formatProvider);

        public static Model<T1, T2, T3, T4, TResult> Create<T1, T2, T3, T4, TResult>(
            Expression<Func<T1, T2, T3, T4, TResult>> func,
            IReadOnlyList<TResult> y, IReadOnlyList<T1> x1, IReadOnlyList<T2> x2, IReadOnlyList<T3> x3,
            IReadOnlyList<T4> x4, IFormatProvider formatProvider = null)
            where T1 : IComparable, IFormattable, IConvertible, IComparable<T1>, IEquatable<T1>
            where T2 : IComparable, IFormattable, IConvertible, IComparable<T2>, IEquatable<T2>
            where T3 : IComparable, IFormattable, IConvertible, IComparable<T3>, IEquatable<T3>
            where T4 : IComparable, IFormattable, IConvertible, IComparable<T4>, IEquatable<T4>
            where TResult : IComparable, IFormattable, IConvertible, IComparable<TResult>, IEquatable<TResult>
            => new Model<T1, T2, T3, T4, TResult>(func, y, x1, x2, x3, x4, formatProvider);
    }



    
}
