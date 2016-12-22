using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Markovcd.Classes
{
    public class ModelParser : Model
    {
        public string FunctionText { get; }

        /*public ModelParser(string func, IEnumerable<Token> rules, IReadOnlyList<double> y, params IReadOnlyList<double>[] x)
            : base(FunctionParser.ParseFunction(func, rules), y, x)
        {
            FunctionText = func;
        }*/

        private ModelParser(string func, IReadOnlyList<double> y, params IReadOnlyList<double>[] x)
            : base(FunctionParser.ParseFunction(func), y, x)
        {
            FunctionText = func;
        }

        public ModelParser(string func, IReadOnlyList<double> y, IEnumerable<IReadOnlyList<double>> x)
            : this(func, y, x.ToArray()) { }
    }
}
