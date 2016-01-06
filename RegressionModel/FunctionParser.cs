using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Linq.Expressions;
using JpLabs.DynamicCode;


namespace Markovcd.Classes
{
    public static class FunctionParser
    {
        public static string TrimParentheses(string func)
        {
            func = func.Trim();
            if (func[0] == '(' && func[func.Length - 1] == ')')
                func = func.Substring(1, func.Length - 2);

            return func;
        }

        // f(x,d,f) = x * d
        public static string ParseFunctionType(string func, string type = "double")
        {
            var args = func.Split('=')[0].Split(new[] {',', '(', ')', ' '}, StringSplitOptions.RemoveEmptyEntries);
            if (args[0].ToUpper() != "F") throw new InvalidOperationException();
            var p = Enumerable.Repeat(type, args.Length).Aggregate((s1, s2) => s1 + ',' + s2);
            return $"Func<{p}>";
        }

        public static string ParseToLambda(string func)
        {
            var fIndex = func.IndexOfAny(new[] {'f', 'F'});
            var lambda = func.Substring(fIndex + 1);
            var equalIndex = func.IndexOf('=');
            lambda = lambda.Substring(0, equalIndex) + '>' + lambda.Substring(equalIndex + 1);
            return '(' + lambda + ')';
        }

        public static LambdaExpression ParseFunction(Compiler compiler, string func, string type = "double")
            => compiler.ParseLambdaExpr(ParseToLambda(func), ParseFunctionType(func, type));
        
    }
}
