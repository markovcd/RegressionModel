using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Markovcd.Interfaces;

namespace Markovcd.Classes
{
    public class NamedConstantToken<T> : Token, IExpressionConstructor
        where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
    {
        public IFormatProvider FormatProvider { get; }

        public Func<string, IFormatProvider, T> Parse { get; }

        public T ConstantValue { get; }

        public virtual Expression ConstructExpression()
            => Expression.Constant(Parse(Value, FormatProvider));

        public NamedConstantToken(string name, int index, int length, T value, IFormatProvider formatProvider, Func<string, IFormatProvider, T> parse)
            : base(name, index, length, value.ToString(formatProvider))
        {
            FormatProvider = formatProvider;
            Parse = parse;
        }

        public NamedConstantToken(string name, string rule, T constantValue, IFormatProvider formatProvider, Func<string, IFormatProvider, T> parse)
            : base(name, rule)
        {
            FormatProvider = formatProvider;
            Parse = parse;
            ConstantValue = constantValue;
        }

        public override Token ToMatch(Match match)
            => new NamedConstantToken<T>(Name, match.Index, match.Length, Parse(match.Value, FormatProvider), FormatProvider, Parse);
    }

    public class DoubleNamedConstantToken : NamedConstantToken<double>
    {
        public DoubleNamedConstantToken(string name, int index, int length, double value)
            : base(name, index, length, value, DoubleConstantToken.CultureInfo, double.Parse)
        { }

        public DoubleNamedConstantToken(string name)
            : base(name, DoubleConstantToken.Format, DoubleConstantToken.CultureInfo, double.Parse)
        { }

        public static readonly DoubleNamedConstantToken Pi = new DoubleNamedConstantToken(nameof(Pi));
    }
}
