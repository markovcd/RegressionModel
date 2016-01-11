using System.Collections.Generic;
using System.Linq.Expressions;

namespace Markovcd.Classes
{
    public interface IMethodExpressionConstructor
    {
        MethodCallExpression ConstructExpression(IEnumerable<Expression> arguments);
    }
}
