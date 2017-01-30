using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using mass_pnr_lookup.Models;

namespace mass_pnr_lookup.tests
{
    namespace BatchLineTests
    {
        [TestClass]
        public class FillFrom
        {
            [TestMethod]
            public void FillFrom_First_Found()
            {
                FillFrom_Success(0);
            }

            [TestMethod]
            public void FillFrom_Second_Found()
            {
                FillFrom_Success(1);
            }

            [TestMethod]
            public void FillFrom_Third_Found()
            {
                FillFrom_Success(2);
            }

            public void FillFrom_Success(int index)
            {
                var factory = new SoegListOutputTypeFactory();
                var names = new string[] { "first1 middle1 last1", "first1 middle1 middle12 last1", "first2 middle2 last2" };
                var expected = names.First();
                
                var names2 = names.Skip(1).ToList();
                names2.Insert(index, expected);

                var sr = factory.Create(names2.ToArray());
                var bl = new BatchLine() { Name = expected };
                var success = bl.FillFrom(sr);
                Assert.IsTrue(success);
                Assert.AreEqual(expected, bl.MatchedName);
            }

            [TestMethod]
            public void FillFrom_UnmatchedName_False()
            {
                var factory = new SoegListOutputTypeFactory();
                var names = new string[] { "first1 middle1 last1", "first1 middle1 middle12 last1", "first2 middle2 last2" };
                var expected = "first1 middle1 middle2 last1";

                var sr = factory.Create(names);
                var bl = new BatchLine() { Name = expected };
                var success = bl.FillFrom(sr);
                Assert.IsFalse(success);

            }
        }
    }
}