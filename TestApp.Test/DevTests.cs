#nullable enable
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestApp.Test
{
    [TestClass]
    public class DevTests
    {
        [DataTestMethod]
        [DataRow("*", new uint[] {0, 1, 2}, 3)]
        [DataRow("12", new uint[] { 12 }, 3)]
        [DataRow("3-7", new uint[] { 3, 4, 5, 6, 7 }, 3)]
        [DataRow("*/4", new uint[] {0, 4, 8, 12, 16, 20}, 24)]
        [DataRow("1,2,3-5,10-20/3",  new uint[] {1,2,3,4,5,10,13,16,19}, 32)]
        public void TestParsing(string strRep, uint[] parsedValue, int limit)
        {
            var parser = new InputParser();

            var buff = new uint [512];
            
            var result = parser.Parse(strRep);

            Assert.IsTrue(result.TryWriteValues(buff, (uint)limit, out var nWritten));
            Assert.AreEqual(parsedValue.Length, (int)nWritten);
            Assert.IsTrue(parsedValue.AsSpan().SequenceEqual(buff.AsSpan(0, (int)nWritten)));
        }
    }
}