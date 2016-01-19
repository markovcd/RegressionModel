using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Markovcd.Classes
{
    public class Tokenizer : IEnumerable<Token>
    {
        private readonly ExtendedRegex regex;
        private IEnumerable<Token> lastResult;

        public string LastExpression { get; protected set; }
        public IEnumerable<Token> Rules => rules.Values;
        public IEnumerable<ParameterToken> Parameters => rules.Values.Where(t => t is ParameterToken).Cast<ParameterToken>().ToList();
        private readonly Dictionary<string, Token> rules;  

        public Tokenizer(IEnumerable<Token> rules, IEnumerable<ParameterToken> parameters = null)
        {
            parameters = parameters ?? Enumerable.Empty<ParameterToken>();
        
            this.rules = rules.Concat(parameters).ToDictionary(t => t.Name, t => t);;
            regex = new ExtendedRegex(Rules);
        }

        public IEnumerable<Token> Tokenize(string expression)
        {
            lastResult = regex.MatchesEx(expression)
                              .Select(item => rules[item.Value].ToMatch(item.Key));

            lastResult = IncludeUnknownTokens(expression, lastResult).ToList();

            LastExpression = expression;
            return lastResult;
        }

        private static IEnumerable<Token> IncludeUnknownTokens(string expression, IEnumerable<Token> tokens)
        {
            var lastIndex = 0;

            foreach (var token in tokens)
            {
                var gap = "";

                if (token.Index > lastIndex) 
                    gap = expression.Substring(lastIndex, token.Index - lastIndex);

                var index = gap.Length - gap.TrimStart().Length + lastIndex;
                gap = gap.Trim();
                
                if (gap != "") yield return new UnkownToken(index, gap.Length, gap);

                yield return token;

                lastIndex = token.Index + token.Length;
            }
        }

        public static readonly IEnumerable<Token> DefaultTokens = new Token[]
        {
            LeftBracketToken.Default, RightBracketToken.Default, ArgumentSeparatorToken.Default,
            BinaryOperatorToken.Addition, BinaryOperatorToken.Subtraction,
            BinaryOperatorToken.Multiplication, BinaryOperatorToken.Division,
            BinaryOperatorToken.Power, DoubleLiteralToken.Default,
            FunctionToken.Sin, FunctionToken.Cos, FunctionToken.Sqrt, FunctionToken.Tan,
            DoubleConstantToken.E, DoubleConstantToken.Pi, 
        };

        public static Tokenizer Default(params ParameterToken[] parameters) 
            => new Tokenizer(DefaultTokens, parameters);

        public static Tokenizer Default(params string[] parameters)
            => new Tokenizer(DefaultTokens, parameters.Select(s => new ParameterToken(s)));

        public IEnumerator<Token> GetEnumerator() 
            => lastResult.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() 
            => GetEnumerator();
    }

    
}
