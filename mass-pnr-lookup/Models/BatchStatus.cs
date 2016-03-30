using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mass_pnr_lookup.Models
{
    public enum BatchStatus
    {
        Created,
        Processing,
        Completed,
        Notified,
        Error,
        Paused
    }
}