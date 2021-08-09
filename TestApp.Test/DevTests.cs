#nullable enable
using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestApp.Test
{
    [TestClass]
    public class DevTests
    {
        
        [DataTestMethod]
        // [DataRow("2022.9-11.28 3 10:00:00.000")]
        [DataRow("2020,2022.1,2.31,1 * 10:00-30:00.000")]
        public void TestBitMap(string input)
        
        {
            IParser parser = new InputParser();
            IScheduleProvider provider = new SimpleScheduleProvider(parser);

            var schedule = provider.GetSchedule(input) as SimpleSchedule ?? throw new Exception();
            var dt = new DateTime(2022, 1, 31, 10, 29, 59, 999);
            // var next = schedule.NextEvent(new DateTime(2021, 2, 2, 9, 00, 00));
            var next = schedule.NextEvent(dt);
            var nextStr = next.ToString("O");
            
        }
    }
}