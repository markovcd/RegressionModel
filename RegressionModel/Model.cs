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

        public override string ToString()
        {
            var sb = new StringBuilder();

            var func = SplitOriginalFunction.Select(f => f.Body.ToString())
                                            .Select(FunctionParser.TrimParentheses)
                                            .Zip(Coefficients, (s, d) => s.Equals("1") ? $"{d:0.00}" : $"{d:0.00} * {s}")
                                            .Aggregate((s1, s2) => $"{s1} + {s2}");
            

            sb.Append(func);

            return sb.ToString();
        }
    }



    
}
