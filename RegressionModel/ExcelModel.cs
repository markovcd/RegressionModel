using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegressionModel
{
    public class ExcelModel : ExcelBase
    {
        public ExcelModel(string filename)
            : base(filename)
        { }

        public IReadOnlyList<T> GetArray<T>(string sheetname, string address, bool horizontal = false, IFormatProvider formatProvider = null) 
            => GetArrays<T>(sheetname, address, horizontal, formatProvider).First();

        public IReadOnlyList<IReadOnlyList<T>> GetArrays<T>(string sheetname, string address, bool horizontal = false, IFormatProvider formatProvider = null)
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
