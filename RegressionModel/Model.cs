using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Markovcd.Classes
{
    public class Model : ModelBase
    {      
        public LambdaExpression OriginalFunction { get; }
        public IReadOnlyList<LambdaExpression> SplitOriginalFunction { get; }
        public LambdaExpression Function { get; }
        public IReadOnlyList<LambdaExpression> SplitFunction { get; } 
        public Delegate CompiledFunction { get; }

        public IReadOnlyList<IReadOnlyList<double>> X { get; }
        public IReadOnlyList<double> Y { get; }
        public IReadOnlyList<double> Coefficients { get; }
        public double RSquared { get; }
        public int ParameterCount { get; }

        public Model(LambdaExpression func, IReadOnlyList<double> y, params IReadOnlyList<double>[] x)
        {
            X = x;
            Y = y;
            ParameterCount = x.Length;
            OriginalFunction = func;
            SplitOriginalFunction = SplitAndCheckLambda(OriginalFunction).ToList();
            Function = func.ToParams<double>();
            SplitFunction = SplitAndCheckLambda(Function).ToList();
            
            Coefficients = GetCoefficients(SplitFunction, y, x);
            
            CompiledFunction = Compile(SplitFunction, Coefficients);
            RSquared = CalculateRSquared(CompiledFunction, y, x);
        }

        public double Calculate(params double[] x) 
            => Calculate(CompiledFunction, x);

        private static string TrimParentheses(string func)
        {
            func = func.Trim();
            if (func[0] == '(' && func[func.Length - 1] == ')')
                func = func.Substring(1, func.Length - 2);

            return func;
        }

        public override string ToString()
        {
            var parameters = OriginalFunction.Parameters.Select(p => p.Name).Aggregate((s1, s2) => $"{s1}, {s2}");

            var terms = SplitOriginalFunction.Select(f => f.Body.ToString())
                                             .Select(TrimParentheses)
                                             .Select(s => new string(s.Where(c => !char.IsWhiteSpace(c)).ToArray()))
                                             .ToArray();


            var coeffs = Coefficients.Select((d, i) => $"b{i + 1} = {d}").Aggregate((s1, s2) => $"{s1}\n{s2}");
            var b = Coefficients.Select((d, i) => $"b{i + 1}"); 

            var originalFunc = terms.Zip(b, (s, d) => s.Equals("1") ? $"{d}" : $"{d}*{s}")
                                    .Aggregate((s1, s2) => $"{s1} + {s2}");

            var func = terms.Zip(Coefficients, (s, d) => s.Equals("1") ? $"{d:0.000}" : $"{d:0.000}*{s}")
                            .Aggregate((s1, s2) => $"{s1} + {s2}");

            var sb = new StringBuilder();

            sb.AppendLine("Given function:");
            sb.AppendLine($"f({parameters}) = {originalFunc}");
            sb.AppendLine();
            sb.AppendLine("Coefficients:");
            sb.AppendLine(coeffs);
            sb.AppendLine();
            sb.AppendLine($"R squared: {RSquared}");
            sb.AppendLine();
            sb.AppendLine("Resulting function:");
            sb.Append($"f({parameters}) = {func}");

            return sb.ToString();
        }
    }



    
}
