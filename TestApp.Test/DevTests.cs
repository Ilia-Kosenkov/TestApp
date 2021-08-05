#nullable enable
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestApp.Test
{
    [TestClass]
    public class DevTests
    {
        [DataTestMethod]
        [DataRow("*", new int[] {})]
        [DataRow("12", new[] { 12 })]
        [DataRow("3-7", new[] { 3, 4, 5, 6, 7 })]
        [DataRow("1,2,3-5,10-20/3",  new [] {1,2,3,4,5,10,13,16,19})]
        public void TestParsing(string strRep, int[] parsedValue)
        {
            var parser = new InputParser();

            var result = parser.Parse(strRep);
        }
    }
}