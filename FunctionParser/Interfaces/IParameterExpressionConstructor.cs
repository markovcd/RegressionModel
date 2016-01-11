using System.Linq.Expressions;

namespace Markovcd.Classes
{
    public interface IParameterExpressionConstructor
    {
        ParameterExpression ConstructExpression { get; }
    }
}
