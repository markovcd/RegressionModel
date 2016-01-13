using System;

namespace Markovcd.Interfaces
{
    public interface IConstant<out T>
        where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
    {
        T ConstantValue { get; }
    }
}
