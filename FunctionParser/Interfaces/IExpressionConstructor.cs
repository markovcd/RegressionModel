using System.Linq.Expressions;

namespace Markovcd.Classes
{
    public interface IExpressionConstructor
    {
        Expression ConstructExpression { get; }
    }
}
