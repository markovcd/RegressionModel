using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Markovcd.Interfaces;

namespace Markovcd.Classes
{
    public class LiteralToken<T> : Token, IExpressionConstructor
        where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
    {
        public IFormatProvider FormatProvider { get; }

        public Func<string, IFormatProvider, T> Parse { get; }

        public T ConstantValue { get; }

        public virtual Expression ConstructExpression()
            => Expression.Constant(Parse(Value, FormatProvider));

        public LiteralToken(int index, int length, T value, IFormatProvider formatProvider, Func<string, IFormatProvider, T> parse)
            : base(GroupName, index, length, value.ToString(formatProvider))
        {
            FormatProvider = formatProvider;
            Parse = parse;
            ConstantValue = value;
        }

        public LiteralToken(string rule, IFormatProvider formatProvider, Func<string, IFormatProvider, T> parse)
            : base(GroupName, rule)
        {
            FormatProvider = formatProvider;
            Parse = parse;
        }

        public override Token ToMatch(Match match)
            => new LiteralToken<T>(match.Index, match.Length, Parse(match.Value, FormatProvider), FormatProvider, Parse);

        internal static string GroupName
            => typeof (T).Name + nameof(LiteralToken<T>);
    }

    public class DoubleConstantToken : LiteralToken<double>
    {
        internal static readonly CultureInfo CultureInfo = CultureInfo.InvariantCulture;
        internal static readonly string Format = @"([0-9]+\.?[0-9]*)|(\.[0-9]+)";

        public DoubleConstantToken(int index, int length, double value)
            : base(index, length, value, CultureInfo, double.Parse) { }
        
        public DoubleConstantToken()
            : base(Format, CultureInfo, double.Parse) { }

        public static readonly DoubleConstantToken Default = new DoubleConstantToken();
    }

    
}
