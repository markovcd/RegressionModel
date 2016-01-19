using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace RegressionModel
{
    class ExcelWriter
    {
        public Excel.Range GetCell(int fromRow, int fromColumn, int toRow, int toColumn, string filename, string sheetname)
        {
            var excel = new Excel.Application();

            try
            {
                Excel._Workbook workbook = excel.Workbooks.Open(filename);
                Excel._Worksheet sheet = workbook.Sheets[sheetname];
                var range = sheet.Range[sheet.Cells[fromRow, fromColumn], sheet.Cells[toRow, toColumn]];
                return range;
            }
            finally 
            {
                excel.Quit();
            }
        }

        public Excel.Range GetCell(int row, int column, string filename, string sheetname)
        {
            var excel = new Excel.Application();

            try
            {
                Excel._Workbook workbook = excel.Workbooks.Open(filename);
                Excel._Worksheet sheet = workbook.Sheets[sheetname];
                var range = sheet.Cells[row, column];
                return range;
            }
            finally
            {
                excel.Quit();
            }
        }


    }
}
