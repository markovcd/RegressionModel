using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Markovcd.Classes
{
    public class NumberToken<T> : Token, IConstantExpressionConstructor where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
    {
        public IFormatProvider FormatProvider { get; }

        protected static T Parse(string value, IFormatProvider formatProvider = null)
        {
            var p = new List<object> {value, formatProvider ?? CultureInfo.InvariantCulture };

            var parse = typeof (T).GetMethod("Parse", new[] {typeof (string), typeof(IFormatProvider)});
            if (parse == null || !parse.IsStatic)
            {
                parse = typeof(T).GetMethod("Parse", new[] { typeof(string) });
                p.RemoveAt(1);
            }
            if (parse == null || !parse.IsStatic) throw new InvalidOperationException();
            
            return (T) parse.Invoke(null, p.ToArray());
        }

        private static string GroupName
            => typeof (NumberToken<T>).GetGenericArguments().First().Name + nameof(NumberToken<T>);

        public ConstantExpression ConstructExpression
            => Expression.Constant(Parse(Value, FormatProvider));

        public NumberToken(int index, int length, T value, IFormatProvider formatProvider)
            : base(GroupName, index, length, value.ToString(formatProvider))
        {
            FormatProvider = formatProvider;
        }

        public NumberToken(string rule)
            : base(GroupName, rule)
        { }

        public override Token MatchFromRule(int index, int length, string value)
            => new NumberToken<T>(index, length, Parse(value, FormatProvider), FormatProvider);
    }

    public class DoubleNumberToken : NumberToken<double>
    {
        public DoubleNumberToken(int index, int length, double value)
            : base(index, length, value, CultureInfo.InvariantCulture) { }
        

        public DoubleNumberToken()
            : base(@"([0-9]+\.?[0-9]*)|(\.[0-9]+)") { }

        public static readonly DoubleNumberToken Default = new DoubleNumberToken();
    }
}
