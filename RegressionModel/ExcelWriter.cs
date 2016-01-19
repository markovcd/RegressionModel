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

        public Excel.Range GetCell(int fromRow, int fromColumn, int toRow, int toColumn, Excel._Workbook workbook, string sheetname)
        {
            Excel._Worksheet sheet = workbook.Sheets[sheetname];
            var range = sheet.Range[sheet.Cells[fromRow, fromColumn], sheet.Cells[toRow, toColumn]];
            return range;
        }

        public Excel.Range GetCell(int fromRow, int fromColumn, int toRow, int toColumn, Excel.Application excel, string filename, string sheetname)
        {
            
            Excel._Workbook workbook = excel.Workbooks.Open(filename);
            return GetCell(fromRow, fromColumn, toRow, toColumn, workbook, sheetname);
           
        }

        public Excel.Range GetCell(int fromRow, int fromColumn, int toRow, int toColumn, string filename, string sheetname)
        {
            var excel = new Excel.Application();

            try
            {
                return GetCell(fromRow, fromColumn, toRow, toColumn, excel, filename, sheetname);
            }
            finally 
            {
                excel.Quit();
            }
        }

    }
}
