using System.Linq.Expressions;

namespace Markovcd.Classes
{
    public interface IConstantExpressionConstructor
    {
        ConstantExpression ConstructExpression { get; }
    }
}
