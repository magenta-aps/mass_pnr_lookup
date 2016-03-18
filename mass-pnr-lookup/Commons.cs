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

        public static bool CanAccessUrl(string url)
        {
            return false;

            SiteMapProvider provider = SiteMap.Provider;
            HttpContext current = HttpContext.Current;
            string rawUrl = VirtualPathUtility.ToAbsolute(url);
            SiteMapNode node = provider.FindSiteMapNode(rawUrl);
            return (node != null &&
             provider.IsAccessibleToUser(HttpContext.Current, node));
        }
    }
}