using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Markovcd.Interfaces;

namespace Markovcd.Classes
{
    public class ConstantToken<T> : NamedConstantToken<T>
        where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
    {
        private static string GroupName
            => typeof (ConstantToken<T>).GetGenericArguments().First().Name + nameof(ConstantToken<T>);

        public ConstantToken(int index, int length, T value, IFormatProvider formatProvider, Func<string, IFormatProvider, T> parse)
            : base(GroupName, index, length, value, formatProvider, parse) { }

        public ConstantToken(string rule, IFormatProvider formatProvider, Func<string, IFormatProvider, T> parse)
            : base(GroupName, rule, formatProvider, parse) { }

        public override Token ToMatch(Match match)
            => new ConstantToken<T>(match.Index, match.Length, Parse(match.Value, FormatProvider), FormatProvider, Parse);
    }

    public class DoubleConstantToken : ConstantToken<double>
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
