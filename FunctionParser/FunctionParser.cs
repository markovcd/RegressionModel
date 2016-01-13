using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Markovcd.Classes
{
    public class FunctionParser
    {
        public static LambdaExpression ParseExpression(string expression, IEnumerable<Token> rules, params string[] parameters)
        {
            var tokenizer = new Tokenizer(rules, parameters.Select(s => new ParameterToken(s)));
            tokenizer.Tokenize(expression);
            var postfix = new Postfix(tokenizer);
            var constructor = new ExpressionConstructor(postfix);
            return constructor.LambdaExpression;
        }

        // function format: f(parameter1, parameter2, ..., parameterN) = expression
        public static LambdaExpression ParseFunction(string function, IEnumerable<Token> rules)
        {
            var split = function.Split('=');
            if (split.Length != 2) throw new FormatException();
            var parameters = ParseFunctionDefinition(split[0]);
            return ParseExpression(split[1], rules, parameters.Select(p => p.Value).ToArray());
        }

        public static LambdaExpression ParseFunction(string function)
            => ParseFunction(function, Tokenizer.DefaultTokens);

        // format: f(parameter1, parameter2, ..., parameterN)
        public static IEnumerable<Token> ParseFunctionDefinition(string functionDef)
        {
            var rules = new Token[] { FunctionDefinitionToken.Default, LeftBracketToken.Default, RightBracketToken.Default, ParameterSeparatorToken.Default };
            var tokenizer = new Tokenizer(rules);
            var tokens = tokenizer.Tokenize(functionDef).ToList();
            VerifyFunctionDefinition(tokens);
            return tokens.Where(t => t is UnkownToken);
        }

        internal class FunctionDefinitionToken : NamedToken
        {
            public FunctionDefinitionToken() 
                : base("f") { }

            public FunctionDefinitionToken(int index)
                : base("f", index) { }

            public static FunctionDefinitionToken Default => new FunctionDefinitionToken();

            public override Token ToMatch(Match match)
                => new FunctionDefinitionToken(match.Index);
        }

        private static void VerifyFunctionDefinition(IList<Token> tokens)
        {
            if (tokens.Count() < 3) throw new FormatException();
            if (!(tokens[0] is FunctionDefinitionToken)) throw new FormatException();
            if (!(tokens[1] is LeftBracketToken)) throw new FormatException();

            for (var i = 2; i < tokens.Count; i+=2)
            {
                if (!(tokens[i] is UnkownToken)) throw new FormatException();
                if (i == tokens.Count - 2 && !(tokens[i + 1] is RightBracketToken)) throw new FormatException();
                if (i < tokens.Count - 2 && !(tokens[i + 1] is ParameterSeparatorToken)) throw new FormatException();
            }

            
        }
    }
}
