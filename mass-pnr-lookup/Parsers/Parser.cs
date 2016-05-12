using mass_pnr_lookup.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace mass_pnr_lookup.Parsers
{
    public abstract partial class Parser : IParser
    {
        public byte[] Contents { get; private set; }
        public DataTable ContentsTable { get; private set; }

        protected MemoryStream _MemoryStream;

        protected int _CurrentLineIndex;

        public Parser(byte[] contents)
        {
            if (contents == null)
                contents = new byte[0];

            Contents = contents;
            _MemoryStream = new MemoryStream();
            _MemoryStream.Write(Contents, 0, Contents.Length);
            _MemoryStream.Seek(0, SeekOrigin.Begin);

            // Perform any custom initialization in the sub class
            CustomInit();

            ContentsTable = new DataTable();
            foreach (var colName in GetColumnNames())
                ContentsTable.Columns.Add(colName, typeof(string));

            foreach (var lineData in GetData())
                ContentsTable.Rows.Add(lineData);
        }

        public abstract object[][] GetData();

        public virtual void CustomInit()
        { }

        public abstract string[] GetColumnNames();

        public ICollection<BatchLine> ToArray()
        {
            return ReadLines();
        }

        public List<BatchLine> ReadLines()
        {
            int i = 0;
            return ContentsTable.Rows
                .OfType<DataRow>()
                .Select(r => ToBatchLine(r, i++))
                .ToList();
        }

        public virtual void Dispose()
        {
            if (_MemoryStream != null)
                ((IDisposable)_MemoryStream).Dispose();
            if (ContentsTable != null)
                ContentsTable.Dispose();
        }

        public BatchLine ToBatchLine(DataRow row, int number)
        {
            if (row == null)
                return null;

            var values = row.ItemArray.Select(i => i as string).ToArray();

            return new BatchLine()
            {
                Name = row["EJER_NAVN"] as string,
                Address = string.Format("{0}, {1}",
                    row["EJER_ADR"],
                    row["EJER_POSTADR"]
                    ),
                Row = number,
                // TODO: This is unnecessary for Xlsx
                SourceContents = string.Join(";", values)
            };
        }

        public abstract byte[] SerializeContents();
    }
}