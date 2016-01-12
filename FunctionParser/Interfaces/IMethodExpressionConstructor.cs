using System.Collections.Generic;
using System.Linq.Expressions;

namespace Markovcd.Interfaces
{
    public interface IMethodExpressionConstructor : IMethodProvider
    {
        MethodCallExpression ConstructExpression(IEnumerable<Expression> arguments);
    }
}
