using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Markovcd.Interfaces;

namespace Markovcd.Classes
{
    public class ConstantToken<T> : NamedToken
    {
        public T ConstantValue

        public ConstantToken(string name, Type type = null)
            : base(name)
        {
            Type = type ?? typeof(double);
        }

        public ParameterToken(string name, int index, Type type = null)
            : base(name, index)
        {
            Type = type ?? typeof(double);
        }

        public override Token ToMatch(Match match)
            => new ParameterToken(Name, match.Index, Type);

        public virtual ParameterExpression ConstructExpression()
            => Expression.Parameter(Type, Name);

        public virtual ParameterExpression ConstructExpression(IEnumerable<ParameterExpression> parameters)
            => parameters.Single(p => p.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase));
    }
}
