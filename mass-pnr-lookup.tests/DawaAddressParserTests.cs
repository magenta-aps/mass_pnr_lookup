using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mass_pnr_lookup.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CprBroker.Schemas.Part;

namespace mass_pnr_lookup.tests
{
    namespace DawaAddressParserTests
    {
        [TestClass]
        public class GetString
        {
            [TestMethod]
            public void GetString_CorrectStreet()
            {
                var response = Properties.Resources.AddressSample;
                var adresses = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(response);
                var parser = new DawaAddressParser();
                var street = parser.GetString(adresses, "adgangsadresse/vejstykke/navn");
                Assert.AreEqual("Studiestræde", street);
            }

            [TestMethod]
            public void GetString_CorrectHouse()
            {
                var response = Properties.Resources.AddressSample;
                var adresses = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(response);
                var parser = new DawaAddressParser();
                var house = parser.GetString(adresses, "adgangsadresse/husnr");
                Assert.AreEqual("14", house);
            }

            [TestMethod]
            public void GetString_CorrectFloor()
            {
                var response = Properties.Resources.AddressSample;
                var adresses = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(response);
                var parser = new DawaAddressParser();
                var floor = parser.GetString(adresses, "etage");
                Assert.IsNull(floor);
            }
        }

        [TestClass]
        public class ToAddressType
        {
            const string MagentaBuilding = "Studiestræde 14, 1455 København K";
            const string MagentaAddress = "Studiestræde 14 1., 1455 København K";// Does not work with 1.sal

            [TestMethod]
            public void ToAddressType_Magenta_NotNull()
            {
                var parser = new DawaAddressParser();
                var ret = parser.ToAddressType(MagentaBuilding);
                Assert.IsNotNull(ret);
            }

            [TestMethod]
            public void ToAddressType_Magenta_CorrectFloor()
            {
                var parser = new DawaAddressParser();
                var ret = parser.ToAddressType(MagentaAddress);
                var val = (ret.Item as DanskAdresseType).AddressComplete.AddressPostal.FloorIdentifier;
                Assert.AreEqual("1", val);
            }
        }
    }
}
