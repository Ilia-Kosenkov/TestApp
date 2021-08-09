using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace TestApp.Test
{
	public static class ScheduleTests_DataProvider
	{
		private static IEnumerable<IScheduleProvider> ScheduleProviderSource
		{
			get
			{
				yield return new OldScheduleImplementationProvider();
				yield return new SimpleScheduleProvider(new InputParser());
			}
		}

		private static IEnumerable<(string Schedule, string Time, string Result)> NearestEventSource
		{
			get
			{
				yield return ("2100.12.31 23:59:59.999", "2000-01-01 00:00:00.000", "2100-12-31 23:59:59.999"); // макс. количество итераций при проверке
				yield return ("*.*.* * *:*:*.*", "2000-01-01 00:00:00.001", "2000-01-01 00:00:00.001"); // "*.*.* * *:*:*.*" (раз в 1 мс)
				yield return ("*.*.* * *:*:*.*", "2100-12-31 23:59:59.999", "2100-12-31 23:59:59.999");
				yield return ("*.*.* 1,3,5 *:*:*.*", "2021-08-02 00:00:00.500", "2021-08-02 00:00:00.500"); // 1,2,3-5,10-20/3 означает список 1,2,3,4,5,10,13,16,19
				yield return ("*.*.* 1,3,5 *:*:*.*", "2021-08-05 00:00:00.500", "2021-08-06 00:00:00.000");
				yield return ("*.*.* * *:*:*.1,2,3-5,10-20/3", "2020-01-01 00:00:00.011", "2020-01-01 00:00:00.013");
				yield return ("*.*.* * *:*:*.1,2,3-5,10-20/3", "2020-12-31 23:59:59.020", "2021-01-01 00:00:00.001");
				yield return ("*.*.* * */4:*:*", "2020-01-01 00:00:00.000", "2020-01-01 00:00:00.000"); // (для часов) */4 означает 0,4,8,12,16,20
				yield return ("*.*.* * */4:*:*", "2020-12-31 21:00:00.000", "2021-01-01 00:00:00.000");
				yield return ("*.9.*/2 1-5 10:00:00.000", "2020-09-03 10:00:00.000", "2020-09-03 10:00:00.000"); // *.9.*/2 1-5 10:00:00.000 означает 10:00 во все дни с пн. по пт. по нечетным числам в сентябре
				yield return ("*.9.*/2 1-5 10:00:00.000", "2020-09-30 12:00:00.000", "2021-09-01 10:00:00.000");
				yield return ("*:00:00", "2020-01-01 00:00:00.000", "2020-01-01 00:00:00.000"); // *:00:00 означает начало любого часа
				yield return ("*:00:00", "2020-12-31 23:59:59.999", "2021-01-01 00:00:00.000");
				yield return ("*.*.01 01:30:00", "2020-01-01 01:30:00.000", "2020-01-01 01:30:00.000"); // *.*.01 01:30:00 означает 01:30 по первым числам каждого месяца
				yield return ("*.*.01 01:30:00", "2020-12-31 01:30:00.001", "2021-01-01 01:30:00.000");
				yield return ("*.*.32 12:00:00", "2020-01-31 12:00:00.000", "2020-01-31 12:00:00.000"); // 32-й день означает последнее число месяца
				yield return ("*.*.32 12:00:00", "2020-01-31 12:00:00.001", "2020-02-29 12:00:00.000");
			}
		}

		private static IEnumerable<(string Schedule, string Time, string Result)> NextEventSource
		{
			get
			{
				yield return ("2100.12.31 23:59:59.999", "2000-01-01 00:00:00.000", "2100-12-31 23:59:59.999"); // макс. количество итераций при проверке
				yield return ("*.*.* * *:*:*.*", "2000-01-01 00:00:00.001", "2000-01-01 00:00:00.002"); // "*.*.* * *:*:*.*" (раз в 1 мс)
				yield return ("*.*.* * *:*:*.*", "2100-12-31 23:59:59.998", "2100-12-31 23:59:59.999");
				yield return ("*.*.* 1,3,5 *:*:*.*", "2021-08-02 23:59:59.999", "2021-08-04 00:00:00.000"); // 1,2,3-5,10-20/3 означает список 1,2,3,4,5,10,13,16,19
				yield return ("*.*.* 1,3,5 *:*:*.*", "2021-08-05 00:00:00.500", "2021-08-06 00:00:00.000");
				yield return ("*.*.* * *:*:*.1,2,3-5,10-20/3", "2020-01-01 00:00:00.011", "2020-01-01 00:00:00.013");
				yield return ("*.*.* * *:*:*.1,2,3-5,10-20/3", "2020-12-31 23:59:59.020", "2021-01-01 00:00:00.001");
				yield return ("*.*.* * */4:*:*", "2020-01-01 00:00:00.000", "2020-01-01 00:00:01.000"); // (для часов) */4 означает 0,4,8,12,16,20
				yield return ("*.*.* * */4:*:*", "2020-12-31 21:00:00.000", "2021-01-01 00:00:00.000");
				yield return ("*.9.*/2 1-5 10:00:00.000", "2020-09-03 00:00:00.000", "2020-09-03 10:00:00.000"); // *.9.*/2 1-5 10:00:00.000 означает 10:00 во все дни с пн. по пт. по нечетным числам в сентябре
				yield return ("*.9.*/2 1-5 10:00:00.000", "2020-09-30 12:00:00.000", "2021-09-01 10:00:00.000");
				yield return ("*:00:00", "2020-01-01 00:00:00.000", "2020-01-01 01:00:00.000"); // *:00:00 означает начало любого часа
				yield return ("*:00:00", "2020-12-31 23:59:59.999", "2021-01-01 00:00:00.000");
				yield return ("*.*.01 01:30:00", "2020-01-01 01:00:00.000", "2020-01-01 01:30:00.000"); // *.*.01 01:30:00 означает 01:30 по первым числам каждого месяца
				yield return ("*.*.01 01:30:00", "2020-12-31 01:30:00.000", "2021-01-01 01:30:00.000");
				yield return ("*.*.32 12:00:00", "2020-01-31 11:00:00.000", "2020-01-31 12:00:00.000"); // 32-й день означает последнее число месяца
				yield return ("*.*.32 12:00:00", "2020-01-31 12:00:00.000", "2020-02-29 12:00:00.000");
			}
		}

		private static IEnumerable<(string Schedule, string Time, string Result)> NearestPrevEventSource
		{
			get
			{
				yield return ("2000.01.01 00:00:00.000", "2100-12-31 23:59:59.999", "2000-01-01 00:00:00.000"); // макс. количество итераций при проверке
				yield return ("*.*.* * *:*:*.*", "2000-01-01 00:00:00.001", "2000-01-01 00:00:00.001"); // "*.*.* * *:*:*.*" (раз в 1 мс)
				yield return ("*.*.* * *:*:*.*", "2100-12-31 23:59:59.999", "2100-12-31 23:59:59.999");
				yield return ("*.*.* 1,3,5 *:*:*.*", "2021-08-02 00:00:00.500", "2021-08-02 00:00:00.500"); // 1,2,3-5,10-20/3 означает список 1,2,3,4,5,10,13,16,19
				yield return ("*.*.* 1,3,5 *:*:*.*", "2021-08-05 00:00:00.500", "2021-08-04 23:59:59.999");
				yield return ("*.*.* * *:*:*.1,2,3-5,10-20/3", "2020-01-01 00:00:00.013", "2020-01-01 00:00:00.013");
				yield return ("*.*.* * *:*:*.1,2,3-5,10-20/3", "2021-01-01 00:00:00.000", "2020-12-31 23:59:59.019");
				yield return ("*.*.* * */4:*:*", "2020-01-01 00:00:00.000", "2020-01-01 00:00:00.000"); // (для часов) */4 означает 0,4,8,12,16,20
				yield return ("*.*.* * */4:*:*", "2021-01-01 05:50:00.000", "2021-01-01 04:59:59.000");
				yield return ("*.9.*/2 1-5 10:00:00.000", "2020-09-03 10:00:00.000", "2020-09-03 10:00:00.000"); // *.9.*/2 1-5 10:00:00.000 означает 10:00 во все дни с пн. по пт. по нечетным числам в сентябре
				yield return ("*.9.*/2 1-5 10:00:00.000", "2021-09-01 09:59:59.999", "2020-09-29 10:00:00.000");
				yield return ("*:00:00", "2020-01-01 00:00:00.000", "2020-01-01 00:00:00.000"); // *:00:00 означает начало любого часа
				yield return ("*:00:00", "2021-01-01 23:59:59.999", "2021-01-01 23:00:00.000");
				yield return ("*.*.01 01:30:00", "2020-01-01 01:30:00.000", "2020-01-01 01:30:00.000"); // *.*.01 01:30:00 означает 01:30 по первым числам каждого месяца
				yield return ("*.*.01 01:30:00", "2021-01-01 01:29:59.999", "2020-12-01 01:30:00.000");
				yield return ("*.*.32 12:00:00", "2020-01-31 12:00:00.000", "2020-01-31 12:00:00.000"); // 32-й день означает последнее число месяца
				yield return ("*.*.32 12:00:00", "2020-03-29 00:00:00.000", "2020-02-29 12:00:00.000");
			}
		}

		private static IEnumerable<(string Schedule, string Time, string Result)> PrevEventSource
		{
			get
			{
				yield return ("2000.01.01 00:00:00.000", "2100-12-31 23:59:59.999", "2000-01-01 00:00:00.000"); // макс. количество итераций при проверке
				yield return ("*.*.* * *:*:*.*", "2000-01-01 00:00:00.001", "2000-01-01 00:00:00.001"); // "*.*.* * *:*:*.*" (раз в 1 мс)
				yield return ("*.*.* * *:*:*.*", "2100-12-31 23:59:59.999", "2100-12-31 23:59:59.999");
				yield return ("*.*.* 1,3,5 *:*:*.*", "2021-08-02 00:00:00.500", "2021-08-02 00:00:00.500"); // 1,2,3-5,10-20/3 означает список 1,2,3,4,5,10,13,16,19
				yield return ("*.*.* 1,3,5 *:*:*.*", "2021-08-05 00:00:00.500", "2021-08-04 23:59:59.999");
				yield return ("*.*.* * *:*:*.1,2,3-5,10-20/3", "2020-01-01 00:00:00.013", "2020-01-01 00:00:00.013");
				yield return ("*.*.* * *:*:*.1,2,3-5,10-20/3", "2021-01-01 00:00:00.000", "2020-12-31 23:59:59.019");
				yield return ("*.*.* * */4:*:*", "2020-01-01 00:00:00.000", "2020-01-01 00:00:00.000"); // (для часов) */4 означает 0,4,8,12,16,20
				yield return ("*.*.* * */4:*:*", "2021-01-01 05:50:00.000", "2021-01-01 04:59:59.000");
				yield return ("*.9.*/2 1-5 10:00:00.000", "2020-09-03 10:00:00.000", "2020-09-03 10:00:00.000"); // *.9.*/2 1-5 10:00:00.000 означает 10:00 во все дни с пн. по пт. по нечетным числам в сентябре
				yield return ("*.9.*/2 1-5 10:00:00.000", "2021-09-01 09:59:59.999", "2020-09-29 10:00:00.000");
				yield return ("*:00:00", "2020-01-01 00:00:00.000", "2020-01-01 00:00:00.000"); // *:00:00 означает начало любого часа
				yield return ("*:00:00", "2021-01-01 23:59:59.999", "2021-01-01 23:00:00.000");
				yield return ("*.*.01 01:30:00", "2020-01-01 01:30:00.000", "2020-01-01 01:30:00.000"); // *.*.01 01:30:00 означает 01:30 по первым числам каждого месяца
				yield return ("*.*.01 01:30:00", "2021-01-01 01:29:59.999", "2020-12-01 01:30:00.000");
				yield return ("*.*.32 12:00:00", "2020-01-31 12:00:00.000", "2020-01-31 12:00:00.000"); // 32-й день означает последнее число месяца
				yield return ("*.*.32 12:00:00", "2020-03-29 00:00:00.000", "2020-02-29 12:00:00.000");
			}
		}

		
		
		public static IEnumerable ScheduleTests_NearestEvent_Source =>
			from sp in ScheduleProviderSource
			from testCase in NearestEventSource
			select new TestCaseData(
				sp.GetSchedule(testCase.Schedule), 
				DateTime.ParseExact(testCase.Time, "yyyy-MM-dd HH:mm:ss.fff",  DateTimeFormatInfo.InvariantInfo),
				DateTime.ParseExact(testCase.Result, "yyyy-MM-dd HH:mm:ss.fff",  DateTimeFormatInfo.InvariantInfo)
			);
		
		public static IEnumerable ScheduleTests_NextEvent_Source =>
			from sp in ScheduleProviderSource
			from testCase in NextEventSource
			select new TestCaseData(
				sp.GetSchedule(testCase.Schedule), 
				DateTime.ParseExact(testCase.Time, "yyyy-MM-dd HH:mm:ss.fff",  DateTimeFormatInfo.InvariantInfo),
				DateTime.ParseExact(testCase.Result, "yyyy-MM-dd HH:mm:ss.fff",  DateTimeFormatInfo.InvariantInfo)
			);
		
		public static IEnumerable ScheduleTests_NearestPrevEvent_Source =>
			from sp in ScheduleProviderSource
			from testCase in NearestPrevEventSource
			select new TestCaseData(
				sp.GetSchedule(testCase.Schedule), 
				DateTime.ParseExact(testCase.Time, "yyyy-MM-dd HH:mm:ss.fff",  DateTimeFormatInfo.InvariantInfo),
				DateTime.ParseExact(testCase.Result, "yyyy-MM-dd HH:mm:ss.fff",  DateTimeFormatInfo.InvariantInfo)
			);
		public static IEnumerable ScheduleTests_PrevEvent_Source =>
			from sp in ScheduleProviderSource
			from testCase in PrevEventSource
			select new TestCaseData(
				sp.GetSchedule(testCase.Schedule), 
				DateTime.ParseExact(testCase.Time, "yyyy-MM-dd HH:mm:ss.fff",  DateTimeFormatInfo.InvariantInfo),
				DateTime.ParseExact(testCase.Result, "yyyy-MM-dd HH:mm:ss.fff",  DateTimeFormatInfo.InvariantInfo)
			);
	}
	
	[TestFixture]
	public class ScheduleTests
	{
		
		[Test]
		[TestCaseSource(typeof(ScheduleTests_DataProvider), nameof(ScheduleTests_DataProvider.ScheduleTests_NearestEvent_Source))]
		public void NearestEvent (ISchedule schedule, DateTime time, DateTime expectedResult)
		{
			Assert.AreEqual(expectedResult, schedule.NearestEvent(time));
		}

		[Test]
		[TestCaseSource(typeof(ScheduleTests_DataProvider), nameof(ScheduleTests_DataProvider.ScheduleTests_NextEvent_Source))]
		public void NextEvent (ISchedule schedule, DateTime time, DateTime expectedResult)
		{
			Assert.AreEqual (expectedResult, schedule.NextEvent (time));
		}

		[Test]
		[TestCaseSource(typeof(ScheduleTests_DataProvider), nameof(ScheduleTests_DataProvider.ScheduleTests_NearestPrevEvent_Source))]
		public void NearestPrevEvent (ISchedule schedule, DateTime time, DateTime expectedResult)
		{
			Assert.AreEqual (expectedResult, schedule.NearestPrevEvent (time));
		}

		[Test]
		[TestCaseSource(typeof(ScheduleTests_DataProvider), nameof(ScheduleTests_DataProvider.ScheduleTests_PrevEvent_Source))]
		public void PrevEvent (ISchedule schedule, DateTime time, DateTime expectedResult)
		{
			Assert.AreEqual (expectedResult, schedule.PrevEvent (time));
		}
	}
}
