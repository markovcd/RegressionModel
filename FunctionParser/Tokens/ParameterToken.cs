using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Markovcd.Interfaces;

namespace Markovcd.Classes
{
    public class ParameterToken<T> : NamedToken, IParameterExpressionConstructor
    {
        public ParameterToken(string name)
            : base(name) { }

        public ParameterToken(string name, int index)
            : base(name, index) { }

        public override Token ToMatch(Match match)
            => new ParameterToken<T>(Name, match.Index);

        public virtual ParameterExpression ConstructExpression()
            => Expression.Parameter(typeof (T), Name);

        public virtual ParameterExpression ConstructExpression(IEnumerable<ParameterExpression> parameters)
            => parameters.Single(p => p.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase));
    }
}
