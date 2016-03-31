using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CprBroker.Schemas.Part;

namespace mass_pnr_lookup
{
    public static class Extensions
    {
        public static string ToAddressString(this AddressPostalType addr)
        {
            var parts = new string[]{
                addr.StreetName.TrimLeadingZeros(),
                addr.StreetBuildingIdentifier.TrimLeadingZeros(),
                addr.FloorIdentifier.TrimLeadingZeros(),
                addr.SuiteIdentifier.TrimLeadingZeros(),
                ",",
                addr.PostCodeIdentifier.TrimLeadingZeros(),
                addr.DistrictName.TrimLeadingZeros()
            };

            var ret = string.Join(" ",
                parts
                .Where(p => !string.IsNullOrEmpty(p))
                .ToArray()).Trim();

            if (ret.EndsWith(","))
                ret = ret.Substring(0, ret.Length - 1).Trim();

            return ret;
        }

        public static string TrimLeadingZeros(this string s)
        {
            return string.Format("{0}", s).Trim().TrimStart('0');
        }
    }
}