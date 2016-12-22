using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;


namespace Markovcd.Classes
{
    public class ExcelModel : ExcelBase
    {
        public ModelParser Model { get; }

        public ExcelModel(string filename, string sheetname, string func, IEnumerable<Token> rules, string addressY, string addressX)
            : base(filename)
        {
            var y = GetArray<double>(sheetname, addressY);
            var x = GetArrays<double>(sheetname, addressX);

            Model = new ModelParser(func, rules, y, x);
        }

        public ExcelModel(string filename, string sheetname, string func, string addressY, string addressX)
            : base(filename)
        {
            var y = GetArray<double>(sheetname, addressY);
            var x = GetArrays<double>(sheetname, addressX);

            Model = new ModelParser(func, y, x);
        }

        public void WriteResults(string sheetname, string address)
        {
            var range = GetRange(sheetname, address);
            range.WrapText = false;
            range.Value = "Aproksymacja danych:";
            range.Font.Bold = true;

            range = GetRange(sheetname, range.Row + 1, range.Column);
            range.WrapText = false;
            range.Value = Model.FunctionString.Parametrized;

            range = GetRange(sheetname, range.Row + 1, range.Column);

            var range2 = GetRange(sheetname, range.Row + 1, range.Column + 3);
            range2.WrapText = false;
            range2.Value = "R2 = ";
            range2.Characters[2, 1].Font.Superscript = true;
            range2.Font.Bold = true;

            range2 = GetRange(sheetname, range2.Row, range2.Column + 1);
            range2.WrapText = false;
            range2.Value = Model.RSquared;

            for (var i = 0; i < Model.Coefficients.Count; i++)
            {
                range = GetRange(sheetname, range.Row + 1, range.Column);

                range.WrapText = false;
                range.Value = $"b{i+1} = ";
                range.Font.Bold = true;
                range.Characters[2, 1].Font.Subscript = true;

                var range3 = GetRange(sheetname, range.Row, range.Column + 1);

                range3.WrapText = false;
                range3.Value = Model.Coefficients[i];
            }

            

        }

        public IReadOnlyList<T> GetArray<T>(string sheetname, string address, IFormatProvider formatProvider = null) 
            => GetArrays<T>(sheetname, address, false, formatProvider).SelectMany(l => l).ToList();

        public IReadOnlyList<T>[] GetArrays<T>(string sheetname, string address, bool horizontal = false, IFormatProvider formatProvider = null)
        {
            formatProvider = formatProvider ?? CultureInfo.InvariantCulture;

            var range = GetRange(sheetname, address);
            if (!range.Value2.GetType().IsArray) throw new InvalidOperationException($"{address} is not range address.");

            var arr = (object[,])range.Value2;
            var l1 = arr.GetLength(horizontal ? 0 : 1);
            var l2 = arr.GetLength(horizontal ? 1 : 0);

            var result = new T[l1][];
            for (var i = 0; i < l1; i++)
            {
                result[i] = new T[l2];
                for (var j = 0; j < l2; j++)
                {
                    var v = horizontal ? arr[i + 1, j + 1] : arr[j + 1, i + 1];
                    if (v is IConvertible)
                        result[i][j] = (T)Convert.ChangeType(v, typeof(T), formatProvider);
                    else
                        result[i][j] = (T)v;
                }
            }

            return result;
        }

         

    }
}
