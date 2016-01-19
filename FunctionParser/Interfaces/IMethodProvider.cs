using System;
using System.Reflection;

namespace Markovcd.Interfaces
{
    public interface IMethodProvider
    {
        MethodInfo Function { get; }
        int ParameterCount { get; }
    }

    public interface IMethodProvider<in T, out TResult>
    {
        Func<T, TResult> Function { get; }
    }

    public interface IMethodProvider<in T1, in T2, out TResult>
    {
        Func<T1, T2, TResult> Function { get; }
    }

    public interface IMethodProvider<in T1, in T2, in T3, out TResult>
    {
        Func<T1, T2, T3, TResult> Function { get; }
    }
}
