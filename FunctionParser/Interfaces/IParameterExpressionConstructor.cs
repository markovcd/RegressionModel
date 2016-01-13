using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Markovcd.Interfaces
{
    public interface IParameterExpressionConstructor : IExpressionConstructor
    {
        ParameterExpression ConstructExpression(IEnumerable<ParameterExpression> expressions);
    }
}
