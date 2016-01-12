using System;
using System.Linq.Expressions;

namespace Markovcd.Interfaces
{
    public interface IUnaryExpressionConstructor
    {
        Func<Expression, UnaryExpression> ConstructExpression { get; }
    }

}
