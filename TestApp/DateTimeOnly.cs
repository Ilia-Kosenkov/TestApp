#nullable enable
using System;

namespace TestApp
{
    internal readonly struct Date : IEquatable<Date>
    {
        public const ushort YearOffset = 2000;
        public const ushort MonthOffset = 1;
        public const ushort DayOffset = 1;
        public byte Year { get; init; }
        public byte Month { get; init; }
        public byte Day { get; init; }

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