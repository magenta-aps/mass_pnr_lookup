using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CprBroker.Engine.local;

namespace mass_pnr_lookup
{
    public class CprBrokerLogger
    {
        public static void LogSuccess(string msg)
        {
            Admin.LogSuccess(msg);
        }

        public static void LogError(string msg)
        {
            Admin.LogError(msg);
        }

        public static void LogException(Exception ex, string msg)
        {
            Admin.LogException(ex, msg);
        }
    }
}