using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using mass_pnr_lookup;

namespace mass_pnr_lookup.tests
{
    [TestClass]
    public class ExtensionsTests
    {
        [TestMethod]
        public void TrimLeadingZeros_Null_Empty()
        {
            var ret = (null as string).TrimLeadingZeros();
            Assert.AreEqual("", ret);
        }

        [TestMethod]
        public void TrimLeadingZeros_Empty_Empty()
        {
            var ret = ("").TrimLeadingZeros();
            Assert.AreEqual("", ret);
        }

        [TestMethod]
        public void TrimLeadingZeros_Value_Correct()
        {
            var ret = ("02").TrimLeadingZeros();
            Assert.AreEqual("2", ret);
        }

        [TestMethod]
        public void TrimLeadingZeros_Value2_Correct()
        {
            var ret = ("0002").TrimLeadingZeros();
            Assert.AreEqual("2", ret);
        }
    }
}
