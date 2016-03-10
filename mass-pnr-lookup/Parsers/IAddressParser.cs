using System;
using CprBroker.Schemas.Part;

namespace mass_pnr_lookup.Parsers
{
    public interface IAddressParser
    {
        AdresseType ToAddressType(String addressString);
    }
}