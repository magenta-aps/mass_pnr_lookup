using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace mass_pnr_lookup
{
    public class Commons
    {
        public static readonly Encoding CsvEncoding = Encoding.GetEncoding(1252);

        public static bool CanAccessPath(string path)
        {
            return System.Web.Security.UrlAuthorizationModule.CheckUrlAccessForPrincipal(path, HttpContext.Current.User, "GET");
        }
    }
}