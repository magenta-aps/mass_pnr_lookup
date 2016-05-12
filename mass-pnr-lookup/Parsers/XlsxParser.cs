using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using mass_pnr_lookup.Models;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data;

namespace mass_pnr_lookup.Parsers
{
    public class XlsxParser : Parser
    {
        SpreadsheetDocument _SpreadsheetDocument;
        WorkbookPart wbPart;
        WorksheetPart wsPart;
        Row headerRow;

        public XlsxParser(byte[] contents)
            : base(contents)
        {

        }

        public override void CustomInit()
        {
            _SpreadsheetDocument = SpreadsheetDocument.Open(_MemoryStream, true);

            wbPart = _SpreadsheetDocument.WorkbookPart;
            var firstSheetId = wbPart.Workbook.Descendants<Sheet>().FirstOrDefault().Id;

            wsPart = wbPart.GetPartById(firstSheetId) as WorksheetPart;
        }

        public override string[] GetColumnNames()
        {
            headerRow = wsPart.Worksheet
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

        IEnumerable<Row> DataRows()
        {
            return wsPart.Worksheet
                .Descendants<Row>()
                .Where(r => r.Descendants<Cell>().Count() > 0)
                // Skip the header row
                .Skip(1);
        }

        public override object[][] GetData()
        {
            return DataRows()
                .Select(r =>
                    r.Descendants<Cell>()
                    .Select(c => GetCellValue(c, this.wbPart) as object)
                    .ToArray())
                .ToArray();
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

        private int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart)
        {
            // If the part does not contain a SharedStringTable, create one.
            if (shareStringPart.SharedStringTable == null)
            {
                shareStringPart.SharedStringTable = new SharedStringTable();
            }

            int i = 0;

            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (SharedStringItem item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == text)
                {
                    return i;
                }

                i++;
            }

            // The text does not exist in the part. Create the SharedStringItem and return its index.
            shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(text)));
            shareStringPart.SharedStringTable.Save();

            return i;
        }

        void SetCellValue(SharedStringTablePart stringTable, Cell cell, string value)
        {
            int sharedStringIndex = InsertSharedStringItem(value, stringTable);

            if (cell.DataType == null)
            {
                cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.SharedString);
            }
            cell.DataType.Value = CellValues.SharedString;

            string cellValueText = sharedStringIndex.ToString();
            CellValue cellValue = new CellValue(cellValueText);
            cell.RemoveAllChildren();
            cell.AppendChild<CellValue>(cellValue);
        }

        public override byte[] SerializeContents()
        {
            // Cache the string table object
            var stringTable = wbPart.GetPartsOfType<DocumentFormat.OpenXml.Packaging.SharedStringTablePart>()
                                .FirstOrDefault();

            // Add column headers if needed
            foreach (var column in ContentsTable.Columns.OfType<DataColumn>())
            {
                if (headerRow.Descendants<Cell>().FirstOrDefault(cell => GetCellValue(cell, wbPart).ToUpper() == column.ColumnName.ToUpper()) == null)
                {
                    var cell = new Cell();
                    SetCellValue(stringTable, cell, column.ColumnName);
                    headerRow.AppendChild<Cell>(cell);
                }
            }

            // Set row values
            var excelRows = DataRows().ToArray();

            for (int iRow = 0; iRow < ContentsTable.Rows.Count; iRow++)
            {
                var dataRow = ContentsTable.Rows[iRow];
                var excelRow = excelRows[iRow];

                for (int iCell = 0; iCell < dataRow.ItemArray.Length; iCell++)
                {
                    var cellValue = dataRow.ItemArray[iCell] as string;
                    Cell excelCell = excelRow.Descendants<Cell>().Skip(iCell).FirstOrDefault();
                    if (excelCell == null)
                    {
                        excelCell = new Cell();
                        excelRow.AppendChild<Cell>(excelCell);
                    }
                    SetCellValue(stringTable, excelCell, cellValue);
                }
            }

            // Save
            _SpreadsheetDocument.Close();
            var ret = new byte[_MemoryStream.Length];

            _MemoryStream.Seek(0, SeekOrigin.Begin);
            using (var memStr = new MemoryStream(ret))
            {
                _MemoryStream.CopyTo(memStr);
                memStr.Flush();
            }

            return ret;
        }

        public override void Dispose()
        {
            headerRow = null;
            wbPart = null;
            wsPart = null;

            if (_SpreadsheetDocument != null)
                _SpreadsheetDocument.Dispose();

            base.Dispose();
        }
    }
}
