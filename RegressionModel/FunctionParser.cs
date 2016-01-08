using System;
using System.Linq;
using System.Linq.Expressions;
using Simpro.Expr;

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


        public static string ParseToLambda(string func, Type type)
        {
            var args = func.Split('=')[0]
                           .Split(new[] {',', '(', ')', ' '}, StringSplitOptions.RemoveEmptyEntries)
                           .Skip(1)
                           .Select(s => $"{type.Name} {s}")
                           .Aggregate((s1, s2) => $"{s1}, {s2}");

            var body = func.Split('=')[1];
            return $"({args}) => {body}";
        }

        public static LambdaExpression ParseLambda(string func, Type type) 
            => new ExprParser().Parse(ParseToLambda(func, type));
    }
}
