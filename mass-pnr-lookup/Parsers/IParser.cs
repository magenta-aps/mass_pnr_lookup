using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mass_pnr_lookup.Parsers
{
    public interface IParser : IEnumerable<Models.BatchLine>
    {

    }
}