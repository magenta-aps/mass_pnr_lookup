using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using mass_pnr_lookup.Models;
using System.IO;

namespace mass_pnr_lookup.Parsers
{
    public interface IParser : IDisposable
    {
        string[] GetColumnNames();
        ICollection<BatchLine> ToArray();
        List<BatchLine> ReadLines();
    }
}