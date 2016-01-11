using System;
using System.Linq.Expressions;

namespace Markovcd.Classes
{
    public interface IUnaryExpressionConstructor
    {
        Func<Expression, UnaryExpression> ConstructExpression { get; }
    }

}
