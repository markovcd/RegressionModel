using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Markovcd.Classes;
using Microsoft.Office.Interop.Excel;

namespace ExcelAddin
{
    public partial class MainForm : Form
    {
        private Range _dataRange, _termsRange;

        public MainForm()
        {
            InitializeComponent();
        }

        public Range CoeffsRange => TermsRange?.Offset[0, 1];

        public Range TermsRange
        {
            get
            {
                return _termsRange;
            }
            set
            {
                _termsRange = value;

                if (_termsRange == null) termsText.Text = "";
                else if (_termsRange.Areas.Count != 1) throw new Exception("Range should be one contiguous area.");
                else termsText.Text = _termsRange.Address;
            }
        }

        public Range DataRange
        {
            get
            {
                return _dataRange;
            }
            set
            {
                _dataRange = value;

                if (_dataRange == null) dataText.Text = "";
                else if (_dataRange.Areas.Count < 2) throw new Exception("There should be at least two areas selected (one for Y and one for X).");
                else dataText.Text = _dataRange.Address;
            }
        }

        private static IEnumerable<string> EnumerateVariables(Range r)
        {
            return r.Areas.Cast<Range>().Skip(1).Select(r2 => (r2[1, 1] as Range).Value.ToString()).Cast<string>();
        }

        private static IEnumerable<string> EnumerateTerms(Range r)
        {
            return r.Cast<Range>().Select(r2 => r2.Value.ToString()).Cast<string>();
        }

        private static IEnumerable<IEnumerable<double>> EnumerateValues(Range r)
        {
            return r.Areas.Cast<Range>().Select(r2 => r2.Cells.Cast<Range>().Skip(1).Select(r3 => double.Parse(r3.Value.ToString())).Cast<double>());
        } 

        public IEnumerable<IEnumerable<double>> Values => EnumerateValues(DataRange);

        public string FunctionText
        {
            get
            {
                var variables = EnumerateVariables(DataRange).Aggregate((s1, s2) => $"{s1},{s2}");
                var terms = EnumerateTerms(TermsRange).Aggregate((s1, s2) => $"{s1} + {s2}");
                return $"f({variables}) = {terms}";
            }
        }

        private void dataButton_Click(object sender, EventArgs e)
        {
            dynamic r = Globals.ThisAddIn.Application.InputBox(dataLabel.Text, Text, Type: 8);
            if (r is Range) DataRange = r;
        }

        private void termsButton_Click(object sender, EventArgs e)
        {
            dynamic r = Globals.ThisAddIn.Application.InputBox(termsLabel.Text, Text, Type: 8);
            if (r is Range) TermsRange = r;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            var model = new ModelParser(FunctionText, Values.First().ToList(), Values.Skip(1).Select(d => d.ToList()));

            for (var i = 0; i < model.Coefficients.Count; i++)
            {
                CoeffsRange.Cells[i + 1].Value = model.Coefficients[i];
            }
            
            Close();
        }
    }
}
