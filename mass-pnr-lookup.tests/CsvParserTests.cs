using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using mass_pnr_lookup.Models;
using mass_pnr_lookup.Parsers;
using System.Collections.Generic;
using System.Linq;

namespace mass_pnr_lookup.tests
{
    [TestClass]
    public class CsvParserTests
    {
        byte[] Create(string contents)
        {
            return System.Text.Encoding.UTF8.GetBytes(contents);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Null_Zero()
        {
            var parser = new CsvParser(null);
            var lines = parser.ToArray();
            Assert.AreEqual(0, lines.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NoLines_Zero()
        {
            var parser = new CsvParser(new byte[0]);
            var lines = parser.ToArray();
            Assert.AreEqual(0, lines.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyLines_Zero()
        {
            var parser = new CsvParser(Create(Environment.NewLine + Environment.NewLine + Environment.NewLine));
            var lines = parser.ToArray();
            Assert.AreEqual(0, lines.Count);
        }

        const string FirstLine = "EJER_NAVN;EJER_ADR;EJER_POSTADR\r\n";

        [TestMethod]
        public void OneLine_One()
        {
            var parser = new CsvParser(Create(FirstLine + "person,address in denmark"));
            var lines = parser.ToArray();
            Assert.AreEqual(1, lines.Count);
        }

        [TestMethod]
        public void OneLine_CorrectData()
        {
            var parser = new CsvParser(Create(FirstLine + "person;address in denmark;post"));
            var line = parser.ReadLines().First();
            Assert.AreEqual("person", line.Name);
            Assert.AreEqual("address in denmark, post", line.Address);
        }

        [TestMethod]
        public void AddressWithComma_CorrectData()
        {
            var parser = new CsvParser(Create(FirstLine + "person;address,in , denmark;"));
            var line = parser.ReadLines().First();
            Assert.AreEqual("person", line.Name);
            Assert.AreEqual("address,in , denmark, ", line.Address);
        }

        [TestMethod]
        public void TestEncoding()
        {
            var parser = new CsvParser(System.IO.File.ReadAllBytes(@"C:\MagentaWorkspace\Naturstyrelsen\Mass PNR lookup\Test_Opslag.csv"));
            var lines = parser.ToArray();

            var line = lines.Where(l => l.Name == "Aksel Daugård").FirstOrDefault();
            DawaAddressParser p = new DawaAddressParser();
            var adr = p.ToAddressType(line.Address);

            Console.WriteLine();
        }
    }
}
