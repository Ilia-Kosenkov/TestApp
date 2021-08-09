#nullable enable
using System;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("TestApp.Test")]
namespace TestApp
{
    internal readonly struct Date : IEquatable<Date>
    {

        private static readonly ushort[] DaysInRegularYearMonth;

        private static readonly ushort[] DaysInLeapYearMonth;
        private static readonly byte[] FirstDayOfWeek = new byte[101 * 12];
        
        public const ushort YearOffset = 2000;
        public const ushort MonthOffset = 1;
        public const ushort DayOffset = 1;
        public byte Year { get; init; }
        public byte Month { get; init; }
        public byte Day { get; init; }
        public byte DayOfWeek => (byte)((FirstDayOfWeek[Year * 12 + Month] + Day) % 7);

        // Difference is guaranteed to be less than 32, so `byte` cast is safe
        public byte DaysInCurrentMonth => (byte)(DateTime.IsLeapYear(Year + YearOffset)
                ? DaysInLeapYearMonth[Month + 1] - DaysInLeapYearMonth[Month]
                : DaysInRegularYearMonth[Month + 1] - DaysInRegularYearMonth[Month]
            );
        
        static Date()
        {
            DaysInRegularYearMonth = new ushort[]
            {
                0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365
            };
            DaysInLeapYearMonth = new ushort[] {
                0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366
            };
            
            for (var i = 0; i < 101; i++)
            {
                var id = i * 12;
                var dow = (byte)new DateTime(i + YearOffset, 1, 1).DayOfWeek;
                FirstDayOfWeek[id] = dow;

                if (DateTime.IsLeapYear(i + YearOffset))
                {
                    for (var j = 1; j < 12; j++)
                    {
                        FirstDayOfWeek[id + j] = (byte)(
                            (
                                FirstDayOfWeek[id + j - 1] +
                                DaysInLeapYearMonth[j] - DaysInLeapYearMonth[j - 1]
                            ) % 7);
                    }
                }
                else
                {
                    for (var j = 1; j < 12; j++)
                    {
                        FirstDayOfWeek[id + j] = (byte)(
                            (
                                FirstDayOfWeek[id + j - 1] +
                                DaysInRegularYearMonth[j] - DaysInRegularYearMonth[j - 1]
                            ) % 7);
                    }
                }
            }
        }
        
        public void Deconstruct(out byte year, out byte month, out byte day) =>
            (year, month, day) = (Year, Month, Day);

        public override string ToString() => 
            $"{Year + YearOffset:D4}.{Month + MonthOffset:D2}.{Day + DayOffset:D2}";

        public bool Equals(Date other) => 
            Year == other.Year && Month == other.Month && Day == other.Day;

        public override bool Equals(object? obj) => 
            obj is Date other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Year, Month, Day);

        public static bool operator ==(Date left, Date right) => left.Equals(right);

        public static bool operator !=(Date left, Date right) => !left.Equals(right);

        public static explicit operator DateTime(Date @this) =>
            new(@this.Year + YearOffset, @this.Month + MonthOffset, @this.Day + DayOffset);

        public static byte DaysInMonth(byte year, byte month) =>
            (byte)(DateTime.IsLeapYear(year + YearOffset)
                ? DaysInLeapYearMonth[month + 1] - DaysInLeapYearMonth[month]
                : DaysInRegularYearMonth[month + 1] - DaysInRegularYearMonth[month]
            );

    }

    internal readonly struct Time : IEquatable<Time>
    {
        public byte Hour { get; init; }
        public byte Minute { get; init; }
        public byte Second { get; init; }
        public ushort Millisecond { get; init; }

        public void Deconstruct(out byte hour, out byte minute, out byte second, out ushort millisecond) =>
            (hour, minute, second, millisecond) = (Hour, Minute, Second, Millisecond);

        public override string ToString() => $"{Hour:D2}:{Minute:D2}:{Second:D2}.{Millisecond:D3}";
        public bool Equals(Time other) => 
            Hour == other.Hour && Minute == other.Minute && Second == other.Second && Millisecond == other.Millisecond;

        public override bool Equals(object? obj) => obj is Time other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Hour, Minute, Second, Millisecond);

        public static bool operator ==(Time left, Time right) => left.Equals(right);

        public static bool operator !=(Time left, Time right) => !left.Equals(right);
    }
}