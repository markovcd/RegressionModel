using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Markovcd.Interfaces;

namespace Markovcd.Classes
{
    public abstract class NumberToken<T> : Token, IExpressionConstructor where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
    {
        public IFormatProvider FormatProvider { get; }

        public abstract T Parse(string value, IFormatProvider formatProvider = null);

        private static string GroupName
            => typeof (NumberToken<T>).GetGenericArguments().First().Name + nameof(NumberToken<T>);

        public virtual Expression ConstructExpression()
            => Expression.Constant(Parse(Value, FormatProvider));

        public NumberToken(int index, int length, T value, IFormatProvider formatProvider)
            : base(GroupName, index, length, value.ToString(formatProvider))
        {
            FormatProvider = formatProvider;
        }

        public NumberToken(string rule, IFormatProvider formatProvider)
            : base(GroupName, rule)
        {
            FormatProvider = formatProvider;
        }       
    }

    public class DoubleNumberToken : NumberToken<double>
    {
        public DoubleNumberToken(int index, int length, double value)
            : base(index, length, value, CultureInfo.InvariantCulture) { }
        
        public DoubleNumberToken()
            : base(@"([0-9]+\.?[0-9]*)|(\.[0-9]+)", CultureInfo.InvariantCulture) { }

        public override double Parse(string value, IFormatProvider formatProvider = null)
            => double.Parse(value, formatProvider);

        public override Token ToMatch(Match match)
            => new DoubleNumberToken(match.Index, match.Length, Parse(match.Value, FormatProvider));

        public static readonly DoubleNumberToken Default = new DoubleNumberToken();
    }
}
