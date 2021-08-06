#nullable enable
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestApp.Test
{
    [TestClass]
    public class DevTests
    {
        [DataTestMethod]
        [DataRow("*", new ushort[] {0, 1, 2}, 3)]
        [DataRow("12", new ushort[] { 12 }, 3)]
        [DataRow("3-7", new ushort[] { 3, 4, 5, 6, 7 }, 3)]
        [DataRow("*/4", new ushort[] {0, 4, 8, 12, 16, 20}, 24)]
        [DataRow("1,2,3-5,10-20/3",  new ushort[] {1,2,3,4,5,10,13,16,19}, 32)]
        public void TestParsing(string strRep, ushort[] parsedValue, int limit)
        {
            var parser = new InputParser();

            Span<ushort> buff = stackalloc ushort[128];
            
            var result = parser.ParseElement(strRep);

            Assert.IsTrue(result.TryWriteValues(buff, (ushort)limit, out var nWritten));
            Assert.AreEqual(parsedValue.Length, (int)nWritten);
            Assert.IsTrue(parsedValue.AsSpan().SequenceEqual(buff.Slice(0, (int)nWritten)));
        }
    }
}