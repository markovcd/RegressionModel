using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Markovcd.Classes
{
    public class Tokenizer
    {
        private readonly ExtendedRegex regex;

        public IEnumerable<Token> LastResult { get; protected set; }
        public string LastExpression { get; protected set; }
        public IEnumerable<Token> Rules => rules.Values;
        public IEnumerable<ParameterToken<double>> Parameters => rules.Values.Where(t => t is ParameterToken<double>).Cast<ParameterToken<double>>();
        private readonly Dictionary<string, Token> rules;  

        public Tokenizer(IEnumerable<Token> rules, IEnumerable<ParameterToken<double>> parameters = null)
        {
            parameters = parameters ?? Enumerable.Empty<ParameterToken<double>>();
        
            this.rules = rules.Concat(parameters).ToDictionary(t => t.Name, t => t);;
            regex = new ExtendedRegex(Rules);
        }

        public IEnumerable<Token> Tokenize(string expression)
        {
            LastResult = regex.MatchesEx(expression)
                              .Select(item => rules[item.Value].MatchFromRule(item.Key.Index, item.Key.Length, item.Key.Value))
                              .ToList();

            LastExpression = expression;
            return LastResult;
        }

        private static IEnumerable<Token> IncludeUnknownTokens(string expression, IEnumerable<Token> tokens)
        {
            var results = tokens.ToList();

            var lastIndex = 0;

            foreach (var token in results)
            {
                var gap = "";

                if (token.Index <= lastIndex) 
                    gap = expression.Substring(lastIndex, token.Index - lastIndex);

                var gapIndex = 0;
                var gapLength = 0;

                for (var i = 0; i < gap.Length; i++)
                {
                    if (!char.IsWhiteSpace(gap[i])) continue;
                    gapIndex = i + lastIndex;
                    break;
                }

                for (var i = gap.Length - 1; i >= 0; i++)
                {
                    if (!char.IsWhiteSpace(gap[i])) continue;
                    gapLength = i;
                }

                lastIndex = token.Index + token.Length;
            }

            return results;
        }

        public static readonly IEnumerable<Token> DefaultTokens = new Token[]
        {
            LeftBracketToken.Default, RightBracketToken.Default,
            BinaryOperatorToken.Addition, BinaryOperatorToken.Subtraction,
            BinaryOperatorToken.Multiplication, BinaryOperatorToken.Division,
            BinaryOperatorToken.Power, DoubleNumberToken.Default,
            FunctionToken.Sin, FunctionToken.Cos, FunctionToken.Sqrt, FunctionToken.Tan
        };

        public static Tokenizer Default(params ParameterToken<double>[] parameters) 
            => new Tokenizer(DefaultTokens, parameters);

        public static Tokenizer Default(params string[] parameters)
            => new Tokenizer(DefaultTokens, parameters.Select(s => new ParameterToken<double>(s)));
    }

    
}
