using System;
using System.Text.RegularExpressions;

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
