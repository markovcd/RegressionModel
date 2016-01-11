using System;
using System.Linq.Expressions;

namespace Markovcd.Classes
{
    public interface IBinaryExpressionConstructor
    {
        Func<Expression, Expression, BinaryExpression> ConstructExpression { get; }
    }
}
