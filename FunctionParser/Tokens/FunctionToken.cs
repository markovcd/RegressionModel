using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Markovcd.Interfaces;

namespace Markovcd.Classes
{
    public class FunctionToken : NamedToken, IMethodExpressionConstructor
    {
        public MethodInfo Function { get; }
        public int ParameterCount => Function.GetParameters().Length;

        public virtual MethodCallExpression ConstructExpression(IEnumerable<Expression> arguments)
            => Expression.Call(Function, arguments);

        public FunctionToken(string name, int index, MethodInfo function)
            : base(name, index)
        {
            Function = function;
        }

        public FunctionToken(string name, MethodInfo function)
            : base(name)
        {
            Function = function;
        }

        public override Token ToMatch(Match match) 
            => new FunctionToken(Name, match.Index, Function);

        public static readonly FunctionToken<double, double> Sin = new FunctionToken<double, double>(nameof(Sin), Math.Sin);
        public static readonly FunctionToken<double, double> Cos = new FunctionToken<double, double>(nameof(Cos), Math.Cos);
        public static readonly FunctionToken<double, double> Tan = new FunctionToken<double, double>(nameof(Tan), Math.Tan);
        public static readonly FunctionToken<double, double> Sqrt = new FunctionToken<double, double>(nameof(Sqrt), Math.Sqrt);
    }

    public class FunctionToken<T, TResult> : FunctionToken, IMethodProvider<T, TResult>
    {
        public FunctionToken(string name, int index, Func<T, TResult> function) : base(name, index, function.Method)
        {
            Function = function;
        }

        public FunctionToken(string name, Func<T, TResult> function) : base(name, function.Method)
        {
            Function = function;
        }

        public new Func<T, TResult> Function { get; }

        public override Token ToMatch(Match match) 
            => new FunctionToken<T, TResult>(Name, match.Index, Function);
    }

    public class FunctionToken<T1, T2, TResult> : FunctionToken, IMethodProvider<T1, T2, TResult>
    {
        public FunctionToken(string name, int index, Func<T1, T2, TResult> function) : base(name, index, function.Method)
        {
            Function = function;
        }

        public FunctionToken(string name, Func<T1, T2, TResult> function) : base(name, function.Method)
        {
            Function = function;
        }

        public new Func<T1, T2, TResult> Function { get; }

        public override Token ToMatch(Match match) 
            => new FunctionToken<T1, T2, TResult>(Name, match.Index, Function);
    }

    public class FunctionToken<T1, T2, T3, TResult> : FunctionToken, IMethodProvider<T1, T2, T3, TResult>
    {
        public FunctionToken(string name, int index, Func<T1, T2, T3, TResult> function) : base(name, index, function.Method)
        {
            Function = function;
        }

        public FunctionToken(string name, Func<T1, T2, T3, TResult> function) : base(name, function.Method)
        {
            Function = function;
        }

        public new Func<T1, T2, T3, TResult> Function { get; }

        public override Token ToMatch(Match match) 
            => new FunctionToken<T1, T2, T3, TResult>(Name, match.Index, Function);
    }
}
