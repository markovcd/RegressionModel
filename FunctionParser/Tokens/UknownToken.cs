using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Markovcd.Classes;

namespace Markovcd.Classes
{
    public class UnkownToken : Token
    {
        public UnkownToken(int index, int length, string value) 
            : base(nameof(UnkownToken), index, length, value) { }

        public override Token ToMatch(Match match)
        {
            throw new InvalidOperationException();
        }
    }
}
