using System;
using CprBroker.Schemas.Part;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace mass_pnr_lookup.Parsers
{
    public class DawaAddressParser : IAddressParser
    {
        public AdresseType ToAddressType(String addressString)
        {
            try
            {
                addressString = System.Web.HttpUtility.UrlEncode(addressString, System.Text.Encoding.UTF8);
                String urlString = "http://dawa.aws.dk/adresser?q=" + addressString;



                var client = new System.Net.Http.HttpClient();
                var response = client.GetStringAsync(urlString).ToString();

                var adresses = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(response);

                if (adresses.Count > 0)
                {
                    String streetName = null, houseNumber = null, floor = null, door = null, districtSubdivision = null, postCode = null, postDistrict = null;

                    streetName = GetString(adresses, "adgangsadresse/vejstykke/navn");
                    houseNumber = GetString(adresses, "adgangsadresse/husnr");
                    floor = GetString(adresses, "etage");
                    door = GetString(adresses, "dør");
                    districtSubdivision = GetString(adresses, "adgangsadresse/supplerendebynavn");
                    postCode = GetString(adresses, "adgangsadresse/postnummer/nr");
                    postDistrict = GetString(adresses, "adgangsadresse/postnummer/navn");

                    AdresseType ret = new AdresseType()
                    {
                        Item = new DanskAdresseType()
                        {
                            PostDistriktTekst = postDistrict,
                            AddressComplete = new AddressCompleteType()
                            {
                                AddressAccess = new AddressAccessType()
                                {
                                    StreetBuildingIdentifier = houseNumber
                                },
                                AddressPostal = new AddressPostalType()
                                {
                                    StreetName = streetName,
                                    FloorIdentifier = floor,
                                    SuiteIdentifier = door,
                                    DistrictSubdivisionIdentifier = districtSubdivision,
                                    PostCodeIdentifier = postCode,
                                    DistrictName = postDistrict,
                                }
                            }
                        }
                    };

                    return ret;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return null;
        }

        public String GetString(JToken obj, String path)
        {
            var names = path.Split('/');
            for (int i = 0; i < names.Length - 1; i++)
            {
                obj = obj[names[i]];
            }
            var ret = obj[names[names.Length - 1]];
            if (ret == null)
                return null;
            else
                return ret.ToString();
        }

        public String GetString(JArray array, String path)
        {
            // Read value from first address
            var first = array.First;

            String ret = GetString(first, path);

            // If null, then it was not specified in the query, return
            if (ret == null)
                return null;

            for (int i = 1; i < array.Count; i++)
            {
                var obj = array[i];
                String ret2 = GetString(obj, path);

                // If different from first, then no value was specified, return
                if (!ret.Equals(ret2))
                    return null;
            }
            // Value was either specified or there is only one possible value, return it
            return ret;
        }
    }
}