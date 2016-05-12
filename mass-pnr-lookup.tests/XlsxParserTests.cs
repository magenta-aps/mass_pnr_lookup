using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mass_pnr_lookup;
using mass_pnr_lookup.Parsers;
using mass_pnr_lookup.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Data;

namespace mass_pnr_lookup.tests
{
    [TestClass]
    public class XlsxParserTests
    {
        [TestMethod]
        public void Create_OK()
        {
            var bytes = Properties.Resources.Eksempel_Liste;
            using (var parser = new XlsxParser(bytes))
            { }
        }

        [TestMethod]
        public void AddColumn_ContainsNewColumn()
        {
            byte[] generatedBytes;
            using (var parser = new XlsxParser(Properties.Resources.Eksempel_Liste))
            {
                parser.ContentsTable.Columns.Add("PNR", typeof(string));
                int i = 1;
                foreach (DataRow row in parser.ContentsTable.Rows)
                {
                    row["PNR"] = i++.ToString().PadLeft(10, '0');
                }
                generatedBytes = parser.SerializeContents();
            }
            using (var newParser = new XlsxParser(generatedBytes))
            {
                Assert.IsTrue(newParser.GetColumnNames().Contains("PNR"));
            }
        }
    }
}
