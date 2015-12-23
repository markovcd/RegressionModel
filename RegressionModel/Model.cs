using System;
using System.Collections.Generic;
using System.Linq;

namespace Markovcd.Classes
{
    public class Model
    {
        public delegate double Func(params double[] variables);

        public Func[] Terms { get; }
        public double[][] Data { get; }
        public double[] Coefficients { get; }
        public double RSquared { get; }
        public int ParameterCount { get; }

        public Model(double[][] data, params Func[] terms)
        {
            ParameterCount = data[0].Length - 1;
            Terms = terms;
            Data = data;
            Coefficients = GetCoefficients(Data, Terms);
            RSquared = CalculateRSquared(Data, Coefficients, Terms);
        }

        public double Calculate(params double[] parameters) => Calculate(Coefficients, parameters, Terms);

        public static double Mean(double[][] data) => data.Sum(d => d[0]) / data.Length;

        public static double ResidualSumOfSquares(double[][] data, double[] coefficients, params Func[] terms)
            => SumOfFunction(d => Math.Pow(d[0] - Calculate(coefficients, d.Skip(1).ToArray(), terms), 2), data);

        public static double TotalSumOfSquares(double[][] data)
        {
            var mean = Mean(data);
            return SumOfFunction(d => Math.Pow(d[0] - mean, 2), data);
        }

        public static double CalculateRSquared(double[][] data, double[] coefficients, params Func[] terms)
            => 1 - ResidualSumOfSquares(data, coefficients, terms) / TotalSumOfSquares(data);

        private static double SumOfFunction(Func f, IEnumerable<double[]> data)
            => data.Aggregate(0d, (current, t) => current + f(t));

        public static double Calculate(double[] coefficients, double[] parameters, params Func[] terms)
        {
            var p = new double[parameters.Length + 1];
            parameters.CopyTo(p, 1);

            return terms.Select((t, i) => coefficients[i + 1]*t(p)).Sum() + coefficients[0];
        }
 
        public static double[] GetCoefficients(double[][] data, params Func[] terms)
        {
            var c = new Matrix(terms.Length + 1) { [0] = SumOfFunction(d => d[0], data) };
            var a = new Matrix(terms.Length + 1, terms.Length + 1) {[0, 0] = data.Length };

            for (var i = 0; i < terms.Length; i++)
            {
                var term = terms[i];
                c[i + 1] = SumOfFunction(d => d[0] * term(d), data);
                a[0, i + 1] = a[i + 1, 0] = SumOfFunction(term, data);
            }

            for (var i = 1; i <= terms.Length; i++)
            {
                for (var j = 1; j <= terms.Length; j++)
                {
                    var term1 = terms[i - 1];
                    var term2 = terms[j - 1];
                    a[i, j] = SumOfFunction(d => term1(d) * term2(d), data);
                }
            }

            return (a.Invert()*c).GetColumn();
        }
        
    }
}
