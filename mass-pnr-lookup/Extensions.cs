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
                addr.StreetName,
                addr.StreetBuildingIdentifier,
                addr.FloorIdentifier,
                addr.SuiteIdentifier,
                ",",
                addr.PostCodeIdentifier,
                addr.DistrictName
            };
            return string.Join(" ",
                parts
                .Where(p => !string.IsNullOrEmpty(p))
                .ToArray());
        }
    }
}