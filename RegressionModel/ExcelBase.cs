using System;
using Excel = Microsoft.Office.Interop.Excel;

namespace Markovcd.Classes
{
    abstract public class ExcelBase : IDisposable
    {
        private readonly Excel._Application excel;
        private readonly Excel._Workbook workbook;

        protected ExcelBase(string filename)
        {
            excel = new Excel.Application();
            workbook = excel.Workbooks.Open(filename);
        }

        public void Dispose()
        {
            try { workbook.Close(SaveChanges: true); }
            finally { excel.Quit(); }
        }

        protected Excel._Worksheet GetSheet(string sheetname)
            => workbook.Sheets[sheetname];

        protected Excel.Range GetRange(string sheetname, string address)
            => GetSheet(sheetname).Range[address];

        protected Excel.Range GetRange(string sheetname, int row, int column)
            => GetSheet(sheetname).Cells[row, column];
    }

    
}
