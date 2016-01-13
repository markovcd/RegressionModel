using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Markovcd.Interfaces;

namespace Markovcd.Classes
{
    public class ConstantToken<T> : NamedToken, IExpressionConstructor, IConstant<T> 
        where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
    {
        public T ConstantValue { get; }

        public virtual Expression ConstructExpression()
            => Expression.Constant(ConstantValue);

        public ConstantToken(string name, int index, T constantValue)
            : base(name, index)
        {
            ConstantValue = constantValue;
        }

        public ConstantToken(string name, T constantValue)
            : base(name)
        {
            ConstantValue = constantValue;
        }

        public override Token ToMatch(Match match)
            => new ConstantToken<T>(Name, match.Index, ConstantValue);
    }

    public class DoubleNamedConstantToken : ConstantToken<double>
    {
        public DoubleNamedConstantToken(string name, int index, double constantValue)
            : base(name, index, constantValue) { }

        public DoubleNamedConstantToken(string name, double constantValue)
            : base(name, constantValue) { }

        public static readonly DoubleNamedConstantToken Pi = new DoubleNamedConstantToken(nameof(Pi), Math.PI);
        public static readonly DoubleNamedConstantToken E = new DoubleNamedConstantToken(nameof(E), Math.E);
    }
}
