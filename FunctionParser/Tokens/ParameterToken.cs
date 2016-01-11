using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Markovcd.Classes
{
    public class ParameterToken<T> : NamedToken, IExpressionConstructor
    {
        public ParameterToken(string name)
            : base(name) { }

        public ParameterToken(string name, int index)
            : base(name, index) { }

        public override Token ToMatch(Match match)
            => new ParameterToken<T>(Name, match.Index);

        public Expression ConstructExpression 
            => Expression.Parameter(typeof (T), Name);
    }
}
