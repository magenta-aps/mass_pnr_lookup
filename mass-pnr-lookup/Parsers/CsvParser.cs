using System;
using System.Collections.Generic;
using System.Linq;
using mass_pnr_lookup.Models;
using System.IO;
using System.Collections;

namespace mass_pnr_lookup.Parsers
{
    public class CsvParser : Parser
    {
        string[] _Lines;

        public CsvParser(byte[] contents)
            : base(contents)
        {

        }

        public override void CustomInit()
        {
            using (var _StreamReader = new StreamReader(this._MemoryStream, Commons.CsvEncoding))
            {
                _Lines = _StreamReader.ReadToEnd().Split(Environment.NewLine.ToArray(), StringSplitOptions.RemoveEmptyEntries)
                    .Where(l => l.Trim().Length > 0)
                    .ToArray();
            }
        }

        public override string[] GetColumnNames()
        {
            var firstLine = _Lines.FirstOrDefault();
            if (firstLine != null)
            {
                var firstLineValues = firstLine.Split(';');
                return firstLineValues;
            }
            else
            {
                throw new ArgumentException("Invalid contents");
            }
        }

        public override object[][] GetData()
        {
            return _Lines
                .Skip(1)
                .Select(l =>
                    l.Split(';')
                    .Select(v => v as object)
                    .ToArray()
                ).ToArray();
        }
    }
}