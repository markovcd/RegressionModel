using System;
using System.Linq.Expressions;

namespace Markovcd.Interfaces
{
    public interface IBinaryExpressionConstructor
    {
        Func<Expression, Expression, BinaryExpression> ConstructExpression { get; }
    }
}
