using System.Collections.Generic;
using JpLabs.DynamicCode;

namespace Markovcd.Classes
{
    public class ModelCompiler : Model
    {
        public string FunctionText { get; }

        public ModelCompiler(Compiler compiler, string func, IReadOnlyList<double> y, params IReadOnlyList<double>[] x)
            : base(FunctionParser.ParseFunction(compiler, func), y, x)
        {
            FunctionText = func;
        }
    }
}
