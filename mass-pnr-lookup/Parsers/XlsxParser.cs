using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using mass_pnr_lookup.Models;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace mass_pnr_lookup.Parsers
{
    public class XlsxParser : IParser
    {
        public byte[] Contents { get; private set; }

        public IEnumerator<BatchLine> GetEnumerator()
        {
            return new XlsxEnumerator(Contents);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public class XlsxEnumerator : BaseEnumerator, IEnumerator<BatchLine>
        {
            SpreadsheetDocument _SpreadsheetDocument;
            WorksheetPart wsPart;

            public XlsxEnumerator(byte[] contents)
                : base(contents)
            {

            }
            public override void CustomInit()
            {
                _SpreadsheetDocument = SpreadsheetDocument.Open(_MemoryStream, false);

                var wbPart = _SpreadsheetDocument.WorkbookPart;
                var firstSheetId = wbPart.Workbook.Descendants<Sheet>().FirstOrDefault().Id;

                wsPart = wbPart.GetPartById(firstSheetId) as WorksheetPart;
            }

            public override string[] ReadColumnNames()
            {
                var headerRow = wsPart.Worksheet
                    .Descendants<Row>()
                    .Where(r => r.Descendants<Cell>().Count() > 0)
                    .FirstOrDefault();

                if (headerRow != null)
                {
                    return headerRow
                        .Descendants<Cell>()
                        .Select(cell => GetCellValue(cell, _SpreadsheetDocument.WorkbookPart))
                        .ToArray();
                }
                else
                {
                    throw new ArgumentException("Invalid contents");
                }
            }

            public override string[] ReadCurrentValues()
            {
                throw new NotImplementedException();
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            public override void Dispose()
            {
                if (_SpreadsheetDocument != null)
                    _SpreadsheetDocument.Dispose();
                base.Dispose();
            }

            public bool MoveNext()
            {
                throw new NotImplementedException();
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            private static string GetCellValue(Cell theCell, WorkbookPart wbPart)
            {
                string value = "";
                if (theCell != null)
                {
                    value = theCell.InnerText;

                    // If the cell represents an integer number, you are done. 
                    // For dates, this code returns the serialized value that 
                    // represents the date. The code handles strings and 
                    // Booleans individually. For shared strings, the code 
                    // looks up the corresponding value in the shared string 
                    // table. For Booleans, the code converts the value into 
                    // the words TRUE or FALSE.
                    if (theCell.DataType != null)
                    {
                        switch (theCell.DataType.Value)
                        {
                            case DocumentFormat.OpenXml.Spreadsheet.CellValues.SharedString:

                                // For shared strings, look up the value in the
                                // shared strings table.
                                var stringTable =
                                    wbPart.GetPartsOfType<DocumentFormat.OpenXml.Packaging.SharedStringTablePart>()
                                    .FirstOrDefault();

                                // If the shared string table is missing, something 
                                // is wrong. Return the index that is in
                                // the cell. Otherwise, look up the correct text in 
                                // the table.
                                if (stringTable != null)
                                {
                                    value =
                                        stringTable.SharedStringTable
                                        .ElementAt(int.Parse(value)).InnerText;
                                }
                                break;

                            case DocumentFormat.OpenXml.Spreadsheet.CellValues.Boolean:
                                switch (value)
                                {
                                    case "0":
                                        value = "FALSE";
                                        break;
                                    default:
                                        value = "TRUE";
                                        break;
                                }
                                break;
                        }
                    }
                }
                return value;
            }
        }
    }
}