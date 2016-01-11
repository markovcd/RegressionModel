using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Markovcd.Classes
{
    public class ParameterToken<T> : NamedToken, IParameterExpressionConstructor
    {
        public ParameterToken(string name)
            : base(name) { }

        public ParameterToken(string name, int index)
            : base(name, index) { }

        public override Token MatchFromRule(int index, int length, string value)
            => new ParameterToken<T>(Name, index);

        public ParameterExpression ConstructExpression 
            => Expression.Parameter(typeof (T), Name);
    }
}
