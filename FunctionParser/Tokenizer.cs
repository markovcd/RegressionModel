using System;
using System.Collections;
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
    public class Tokenizer : IEnumerable<Token>
    {
        private readonly ExtendedRegex regex;
        private IEnumerable<Token> lastResult;

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
            LeftBracketToken.Default, RightBracketToken.Default, ParameterSeparatorToken.Default,
            BinaryOperatorToken.Addition, BinaryOperatorToken.Subtraction,
            BinaryOperatorToken.Multiplication, BinaryOperatorToken.Division,
            BinaryOperatorToken.Power, DoubleNumberToken.Default,
            FunctionToken.Sin, FunctionToken.Cos, FunctionToken.Sqrt, FunctionToken.Tan
        };

        public static Tokenizer Default(params ParameterToken<double>[] parameters) 
            => new Tokenizer(DefaultTokens, parameters);

        public static Tokenizer Default(params string[] parameters)
            => new Tokenizer(DefaultTokens, parameters.Select(s => new ParameterToken<double>(s)));

        public IEnumerator<Token> GetEnumerator() 
            => lastResult.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() 
            => GetEnumerator();
    }

    
}
