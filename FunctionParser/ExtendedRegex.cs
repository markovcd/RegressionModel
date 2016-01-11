using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Markovcd.Classes
{
    public class ExtendedRegex : Regex
    {
        public const RegexOptions DefaultOptions =
            RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.Compiled |
            RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant;

        public static string MakeNamedGroups(IEnumerable<Token> rules)
            => rules.Select(rule => rule.ToRegularExpression()).Aggregate((s1, s2) => $"{s1}|{s2}");

        public ExtendedRegex(IEnumerable<Token> rules) : base(MakeNamedGroups(rules), DefaultOptions) { }

        public Dictionary<Match, string> MatchCollectionToGroupNames(MatchCollection matches)
            => matches.Cast<Match>().ToDictionary(m => m, m =>
                GroupNameFromNumber(m.Groups.Cast<Group>()
                    .Select((g, i) => new { g, i })
                    .Skip(1)
                    .First(a => a.g.Success).i));

        public Dictionary<Match, string> MatchesEx(string expression)
            => MatchCollectionToGroupNames(Matches(expression));
    }
}
