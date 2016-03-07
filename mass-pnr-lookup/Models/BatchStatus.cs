using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mass_pnr_lookup.Models
{
    public enum BatchStatus
    {
        Created,
        Uploaded,
        Extracted,
        Processing,
        Completed,
        Notified
    }
}