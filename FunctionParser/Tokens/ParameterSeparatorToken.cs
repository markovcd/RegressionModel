using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Markovcd.Classes
{
    class ParameterSeparatorToken : OperatorToken
    {
        public ParameterSeparatorToken(int index)
            : base(nameof(ParameterSeparatorToken), index, ',', int.MinValue, Associativity.None)
        { }

        public ParameterSeparatorToken() :
            base(nameof(ParameterSeparatorToken), ',', int.MinValue, Associativity.None)
        { }

        public override Token ToMatch(Match match)
            => new ParameterSeparatorToken(match.Index);

        public static readonly ParameterSeparatorToken Default = new ParameterSeparatorToken();
    }
}
