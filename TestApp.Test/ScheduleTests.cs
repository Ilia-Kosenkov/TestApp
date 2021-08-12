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
				yield return
					("2100.12.31 23:59:59.999", "2000-01-01 00:00:00.000",
						"2100-12-31 23:59:59.999"); // макс. количество итераций при проверке
				yield return
					("*.*.* * *:*:*.*", "2000-01-01 00:00:00.001",
						"2000-01-01 00:00:00.001"); // "*.*.* * *:*:*.*" (раз в 1 мс)
				yield return ("*.*.* * *:*:*.*", "2100-12-31 23:59:59.999", "2100-12-31 23:59:59.999");
				yield return
					("*.*.* 1,3,5 *:*:*.*", "2021-08-02 00:00:00.500",
						"2021-08-02 00:00:00.500"); // 1,2,3-5,10-20/3 означает список 1,2,3,4,5,10,13,16,19
				yield return ("*.*.* 1,3,5 *:*:*.*", "2021-08-05 00:00:00.500", "2021-08-06 00:00:00.000");
				yield return ("*.*.* * *:*:*.1,2,3-5,10-20/3", "2020-01-01 00:00:00.011", "2020-01-01 00:00:00.013");
				yield return ("*.*.* * *:*:*.1,2,3-5,10-20/3", "2020-12-31 23:59:59.020", "2021-01-01 00:00:00.001");
				yield return
					("*.*.* * */4:*:*", "2020-01-01 00:00:00.000",
						"2020-01-01 00:00:00.000"); // (для часов) */4 означает 0,4,8,12,16,20
				yield return ("*.*.* * */4:*:*", "2020-12-31 21:00:00.000", "2021-01-01 00:00:00.000");
				yield return
					("*.9.*/2 1-5 10:00:00.000", "2020-09-03 10:00:00.000",
						"2020-09-03 10:00:00.000"); // *.9.*/2 1-5 10:00:00.000 означает 10:00 во все дни с пн. по пт. по нечетным числам в сентябре
				yield return ("*.9.*/2 1-5 10:00:00.000", "2020-09-30 12:00:00.000", "2021-09-01 10:00:00.000");
				yield return
					("*:00:00", "2020-01-01 00:00:00.000",
						"2020-01-01 00:00:00.000"); // *:00:00 означает начало любого часа
				yield return ("*:00:00", "2020-12-31 23:59:59.999", "2021-01-01 00:00:00.000");
				yield return
					("*.*.01 01:30:00", "2020-01-01 01:30:00.000",
						"2020-01-01 01:30:00.000"); // *.*.01 01:30:00 означает 01:30 по первым числам каждого месяца
				yield return ("*.*.01 01:30:00", "2020-12-31 01:30:00.001", "2021-01-01 01:30:00.000");
				yield return
					("*.*.32 12:00:00", "2020-01-31 12:00:00.000",
						"2020-01-31 12:00:00.000"); // 32-й день означает последнее число месяца
				yield return ("*.*.32 12:00:00", "2020-01-31 12:00:00.001", "2020-02-29 12:00:00.000");
			}
		}

		private static IEnumerable<(string Schedule, string Time, string Result)> NextEventSource
		{
			get
			{
				yield return
					("2100.12.31 23:59:59.999", "2000-01-01 00:00:00.000",
						"2100-12-31 23:59:59.999"); // макс. количество итераций при проверке
				yield return
					("*.*.* * *:*:*.*", "2000-01-01 00:00:00.001",
						"2000-01-01 00:00:00.002"); // "*.*.* * *:*:*.*" (раз в 1 мс)
				yield return ("*.*.* * *:*:*.*", "2100-12-31 23:59:59.998", "2100-12-31 23:59:59.999");
				yield return
					("*.*.* 1,3,5 *:*:*.*", "2021-08-02 23:59:59.999",
						"2021-08-04 00:00:00.000"); // 1,2,3-5,10-20/3 означает список 1,2,3,4,5,10,13,16,19
				yield return ("*.*.* 1,3,5 *:*:*.*", "2021-08-05 00:00:00.500", "2021-08-06 00:00:00.000");
				yield return ("*.*.* * *:*:*.1,2,3-5,10-20/3", "2020-01-01 00:00:00.011", "2020-01-01 00:00:00.013");
				yield return ("*.*.* * *:*:*.1,2,3-5,10-20/3", "2020-12-31 23:59:59.020", "2021-01-01 00:00:00.001");
				yield return
					("*.*.* * */4:*:*", "2020-01-01 00:00:00.000",
						"2020-01-01 00:00:01.000"); // (для часов) */4 означает 0,4,8,12,16,20
				yield return ("*.*.* * */4:*:*", "2020-12-31 21:00:00.000", "2021-01-01 00:00:00.000");
				yield return
					("*.9.*/2 1-5 10:00:00.000", "2020-09-03 00:00:00.000",
						"2020-09-03 10:00:00.000"); // *.9.*/2 1-5 10:00:00.000 означает 10:00 во все дни с пн. по пт. по нечетным числам в сентябре
				yield return ("*.9.*/2 1-5 10:00:00.000", "2020-09-30 12:00:00.000", "2021-09-01 10:00:00.000");
				yield return
					("*:00:00", "2020-01-01 00:00:00.000",
						"2020-01-01 01:00:00.000"); // *:00:00 означает начало любого часа
				yield return ("*:00:00", "2020-12-31 23:59:59.999", "2021-01-01 00:00:00.000");
				yield return
					("*.*.01 01:30:00", "2020-01-01 01:00:00.000",
						"2020-01-01 01:30:00.000"); // *.*.01 01:30:00 означает 01:30 по первым числам каждого месяца
				yield return ("*.*.01 01:30:00", "2020-12-31 01:30:00.000", "2021-01-01 01:30:00.000");
				yield return
					("*.*.32 12:00:00", "2020-01-31 11:00:00.000",
						"2020-01-31 12:00:00.000"); // 32-й день означает последнее число месяца
				yield return ("*.*.32 12:00:00", "2020-01-31 12:00:00.000", "2020-02-29 12:00:00.000");
			}
		}

		private static IEnumerable<(string Schedule, string Time, string Result)> NearestPrevEventSource
		{
			get
			{
				yield return
					("2000.01.01 00:00:00.000", "2100-12-31 23:59:59.999",
						"2000-01-01 00:00:00.000"); // макс. количество итераций при проверке
				yield return
					("*.*.* * *:*:*.*", "2000-01-01 00:00:00.001",
						"2000-01-01 00:00:00.001"); // "*.*.* * *:*:*.*" (раз в 1 мс)
				yield return ("*.*.* * *:*:*.*", "2100-12-31 23:59:59.999", "2100-12-31 23:59:59.999");
				yield return
					("*.*.* 1,3,5 *:*:*.*", "2021-08-02 00:00:00.500",
						"2021-08-02 00:00:00.500"); // 1,2,3-5,10-20/3 означает список 1,2,3,4,5,10,13,16,19
				yield return ("*.*.* 1,3,5 *:*:*.*", "2021-08-05 00:00:00.500", "2021-08-04 23:59:59.999");
				yield return ("*.*.* * *:*:*.1,2,3-5,10-20/3", "2020-01-01 00:00:00.013", "2020-01-01 00:00:00.013");
				yield return ("*.*.* * *:*:*.1,2,3-5,10-20/3", "2021-01-01 00:00:00.000", "2020-12-31 23:59:59.019");
				yield return
					("*.*.* * */4:*:*", "2020-01-01 00:00:00.000",
						"2020-01-01 00:00:00.000"); // (для часов) */4 означает 0,4,8,12,16,20
				yield return ("*.*.* * */4:*:*", "2021-01-01 05:50:00.000", "2021-01-01 04:59:59.000");
				yield return
					("*.9.*/2 1-5 10:00:00.000", "2020-09-03 10:00:00.000",
						"2020-09-03 10:00:00.000"); // *.9.*/2 1-5 10:00:00.000 означает 10:00 во все дни с пн. по пт. по нечетным числам в сентябре
				yield return ("*.9.*/2 1-5 10:00:00.000", "2021-09-01 09:59:59.999", "2020-09-29 10:00:00.000");
				yield return
					("*:00:00", "2020-01-01 00:00:00.000",
						"2020-01-01 00:00:00.000"); // *:00:00 означает начало любого часа
				yield return ("*:00:00", "2021-01-01 23:59:59.999", "2021-01-01 23:00:00.000");
				yield return
					("*.*.01 01:30:00", "2020-01-01 01:30:00.000",
						"2020-01-01 01:30:00.000"); // *.*.01 01:30:00 означает 01:30 по первым числам каждого месяца
				yield return ("*.*.01 01:30:00", "2021-01-01 01:29:59.999", "2020-12-01 01:30:00.000");
				yield return
					("*.*.32 12:00:00", "2020-01-31 12:00:00.000",
						"2020-01-31 12:00:00.000"); // 32-й день означает последнее число месяца
				yield return ("*.*.32 12:00:00", "2020-03-29 00:00:00.000", "2020-02-29 12:00:00.000");
			}
		}

		private static IEnumerable<(string Schedule, string Time, string Result)> PrevEventSource
		{
			get
			{
				yield return
					("2000.01.01 00:00:00.000", "2100-12-31 23:59:59.999",
						"2000-01-01 00:00:00.000"); // макс. количество итераций при проверке
				yield return
					("*.*.* * *:*:*.*", "2000-01-01 00:00:00.001",
						"2000-01-01 00:00:00.000"); // "*.*.* * *:*:*.*" (раз в 1 мс)
				yield return ("*.*.* * *:*:*.*", "2021-01-01 00:00:00.000", "2020-12-31 23:59:59.999");
				yield return
					("*.*.* 1,3,5 *:*:*.*", "2021-08-04 00:00:00.000",
						"2021-08-02 23:59:59.999"); // 1,2,3-5,10-20/3 означает список 1,2,3,4,5,10,13,16,19
				yield return ("*.*.* 1,3,5 *:*:*.*", "2021-08-05 00:00:00.500", "2021-08-04 23:59:59.999");
				yield return ("*.*.* * *:*:*.1,2,3-5,10-20/3", "2020-01-01 00:00:00.013", "2020-01-01 00:00:00.010");
				yield return ("*.*.* * *:*:*.1,2,3-5,10-20/3", "2021-01-01 00:00:00.000", "2020-12-31 23:59:59.019");
				yield return
					("*.*.* * */4:*:*", "2021-01-01 00:00:00.000",
						"2020-12-31 20:59:59.000"); // (для часов) */4 означает 0,4,8,12,16,20
				yield return ("*.*.* * */4:*:*", "2021-01-01 05:50:00.000", "2021-01-01 04:59:59.000");
				yield return
					("*.9.*/2 1-5 10:00:00.000", "2020-09-01 10:00:00.000",
						"2019-09-27 10:00:00.000"); // *.9.*/2 1-5 10:00:00.000 означает 10:00 во все дни с пн. по пт. по нечетным числам в сентябре
				yield return ("*.9.*/2 1-5 10:00:00.000", "2021-09-01 09:59:59.999", "2020-09-29 10:00:00.000");
				yield return
					("*:00:00", "2020-01-01 00:00:00.000",
						"2019-12-31 23:00:00.000"); // *:00:00 означает начало любого часа
				yield return ("*:00:00", "2021-01-01 23:59:59.999", "2021-01-01 23:00:00.000");
				yield return
					("*.*.01 01:30:00", "2020-01-01 01:30:00.000",
						"2019-12-01 01:30:00.000"); // *.*.01 01:30:00 означает 01:30 по первым числам каждого месяца
				yield return ("*.*.01 01:30:00", "2021-01-01 01:29:59.999", "2020-12-01 01:30:00.000");
				yield return
					("*.*.32 12:00:00", "2021-03-31 12:00:00.000",
						"2021-02-28 12:00:00.000"); // 32-й день означает последнее число месяца
				yield return ("*.*.32 12:00:00", "2020-03-29 00:00:00.000", "2020-02-29 12:00:00.000");
			}
		}

		private static IEnumerable<(string InvalidInput, bool IgnoreByOld)> InvalidInput_Malformed
		{
			get
			{
				yield return ("", false);
				yield return ("*", false);
				// Two unlimited stars is not allowed
				yield return ("*,*:*:*", true);

				yield return ("*.*:*", false);
			}
		}

		
		private static IEnumerable<(string InvalidInput, bool IgnoreByOld)> InvalidInput_Time
		{
			get
			{
				yield return ("-10:00:00", true);
				yield return ("10:-10:00", true);
				yield return ("10:10:-10", true);
				yield return ("10:10:10.-10", true);

				yield return ("24:00:00", false);
				yield return ("10:60:00", false);
				yield return ("10:10:60", false);
			}
		}

		private static IEnumerable<(string InvalidInput, bool IgnoreByOld)> InvalidInput_Date
		{
			get
			{
				yield return ("1999.01.01 *:*:*", false);
				yield return ("2101.01.01 *:*:*", false);
				yield return ("-2101.01.01 *:*:*", false);

				yield return ("2000.13.01 *:*:*", false);
				yield return ("2000.00.01 *:*:*", false);
				yield return ("2000.-1.01 *:*:*", false);

				yield return ("2000.01.00 *:*:*", false);
				yield return ("2000.01.33 *:*:*", false);
				yield return ("2000.01.-1 *:*:*", false);

			}
		}

		private static IEnumerable<(string InvalidInput, bool IgnoreByOld)> InvalidInput_DayOfWeek
		{
			get
			{
				yield return ("*.*.* -1 *.*.*", false);
				yield return ("*.*.* 7 *:*:*", false);
			}
		}


		private static IEnumerable<(string InvalidInput, bool IgnoreByOld)> InvalidInputSource =>
			InvalidInput_Malformed
				.Concat(InvalidInput_Date)
				.Concat(InvalidInput_Time)
				.Concat(InvalidInput_DayOfWeek);
		


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

		public static IEnumerable ScheduleTests_NoNextEventAvailable_Source =>
			ScheduleProviderSource.Select(x => new TestCaseData(
					x.GetSchedule("2020.*.* *:*:*"),
					new DateTime(2021, 1, 1)
				));

		public static IEnumerable ScheduleTests_NoPrevEventAvailable_Source =>
			ScheduleProviderSource.Select(x => new TestCaseData(
				x.GetSchedule("2020.*.* *:*:*"),
				new DateTime(2019, 1, 1)
			));


		public static IEnumerable ScheduleTests_InvalidInput_Source =>
			from sp in ScheduleProviderSource
			from testCase in InvalidInputSource
			select new TestCaseData(sp, testCase.InvalidInput, testCase.IgnoreByOld);

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

		[Test]
		[TestCaseSource(typeof(ScheduleTests_DataProvider), nameof(ScheduleTests_DataProvider.ScheduleTests_NoNextEventAvailable_Source))]
		public void NoNextEventAvailable(ISchedule schedule, DateTime searchForwardFrom)
		{
			Assert.That(() => schedule.NextEvent(searchForwardFrom), Throws.Exception);
			Assert.That(() => schedule.NearestEvent(searchForwardFrom), Throws.Exception);
		}
		
		[Test]
		[TestCaseSource(typeof(ScheduleTests_DataProvider), nameof(ScheduleTests_DataProvider.ScheduleTests_NoPrevEventAvailable_Source))]
		public void NoPrevEventAvailable(ISchedule schedule, DateTime searchForwardFrom)
		{
			Assert.That(() => schedule.PrevEvent(searchForwardFrom), Throws.Exception);
			Assert.That(() => schedule.NearestPrevEvent(searchForwardFrom), Throws.Exception);
		}

		[Test]
		[TestCaseSource(
			typeof(ScheduleTests_DataProvider),
			nameof(ScheduleTests_DataProvider.ScheduleTests_InvalidInput_Source)
		)]
		public void InvalidInput(IScheduleProvider provider, string invalidInput, bool ignoreByOld)
		{
			if (!ignoreByOld || provider is not OldScheduleImplementationProvider)
			{
				Assert.That(() => provider.GetSchedule(invalidInput), Throws.Exception);
			}
			else
			{
				Assert.Inconclusive(
					"This input combination is handled incorrectly by the original author's implementation."
				);
			}
		}
		
	}
}
