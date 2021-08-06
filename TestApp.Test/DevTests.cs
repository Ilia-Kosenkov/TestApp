#nullable enable
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestApp.Test
{
    [TestClass]
    public class DevTests
    {
        [DataTestMethod]
        [DataRow("*", "00,01,02", 2)]
        [DataRow("12", "12", 3)]
        [DataRow("3-7", "03,04,05,06,07", 3)]
        [DataRow("*/4", "00,04,08,12,16,20", 23)]
        [DataRow("1,2,3-5,10-20/3",  "01,02,03,04,05,10,13,16,19", 32)]
        public void TestParsing(string strRep, string unwrappedValue, int limit)
        {
            var parser = new InputParser();

            
            var result = parser.ParseElement(strRep);

            var parsedStr = result.ToString(2, 0, (ushort)limit);
            Assert.IsTrue(string.Equals(parsedStr, unwrappedValue, StringComparison.Ordinal));
        }

        
        [DataTestMethod]
        [DataRow("10:20:30")]
        [DataRow("10:20:30.450")]
        [DataRow("2021.11.05 10:20:30")]
        [DataRow("2021.11.05 10:20:30.420")]
        [DataRow("2021.11.05 0 10:20:30")]
        [DataRow("2021.11.05 3 10:20:30.998,999,005")]
        public void TestWholeStringParsing(string rep)
        {
            var parser = new InputParser();

            var result = parser.Parse(rep);
            
        }
    }
}