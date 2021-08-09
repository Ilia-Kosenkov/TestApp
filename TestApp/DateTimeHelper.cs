using System;
using System.Collections;

#nullable enable
namespace TestApp
{
    internal static class Helper
    {
        public static int AsInt(this DayOfWeek dow) =>
            dow switch
            {
                DayOfWeek.Sunday => 0,
                DayOfWeek.Monday => 1,
                DayOfWeek.Tuesday => 2,
                DayOfWeek.Wednesday => 3,
                DayOfWeek.Thursday => 4,
                DayOfWeek.Friday => 5,
                DayOfWeek.Saturday => 6,
                _ => throw new InvalidOperationException()
            };

        public static void Deconstruct(this DateTime @this, out Date date, out Time time) =>
            (date, time) = (
                new Date
                {
                    Year = (byte)(@this.Year - Date.YearOffset), 
                    Month = (byte)(@this.Month - Date.MonthOffset), 
                    Day = (byte)(@this.Day - Date.DayOffset)
                },
                new Time
                {
                    Hour = (byte)@this.Hour, 
                    Minute = (byte)@this.Minute, 
                    Second = (byte)@this.Second,
                    Millisecond = (ushort)@this.Millisecond
                }
            );

        public static DateTime WithTime(this Date date, Time time) =>
            new(
                date.Year + Date.YearOffset, 
                date.Month + Date.MonthOffset, 
                date.Day + Date.DayOffset, 
                time.Hour, time.Minute, time.Second, time.Millisecond
            );
        
        public static DateTime WithDate(this Time time, Date date) =>
            new(
                date.Year + Date.YearOffset, 
                date.Month + Date.MonthOffset, 
                date.Day + Date.DayOffset, 
                time.Hour, time.Minute, time.Second, time.Millisecond
            );

        public static ushort MinValue(this BitArray array, int offset, int length)
        {
            for (var i = offset; i < length + offset; i++)
            {
                if (array[i])
                {
                    return (ushort)(i - offset);
                }
            }

            throw new InvalidOperationException("The range is empty -- the event schedule is invalid");
        }
        
        public static ushort MaxValue(this BitArray array, int offset, int length)
        {
            for (var i = offset + length - 1; i >= offset; i--)
            {
                if (array[i])
                {
                    return (ushort)(i - offset);
                }
            }

            throw new InvalidOperationException("The range is empty -- the event schedule is invalid");
        }

        public static (ushort Value, byte CarryOver) ThisOrNextValue(this BitArray array, int offset, int length, int searchFrom)
        {
            for (var i = offset + searchFrom; i < offset + length; i++)
            {
                if (array[i])
                {
                    return ((ushort)(i - offset), 0);
                }
            }

            for (var i = offset; i < offset + searchFrom; i++)
            {
                if (array[i])
                {
                    return ((ushort)(i - offset), 1);
                }
            }
            
            throw new InvalidOperationException("The range is empty -- the event schedule is invalid");
        }
        
        public static (ushort Value, byte CarryOver) ThisOrPrevValue(this BitArray array, int offset, int length, int searchFrom)
        {
            
            for (var i = offset + searchFrom; i >= offset; i--)
            {
                if (array[i])
                {
                    return ((ushort)(i - offset), 0);
                }
            }

            
            for (var i = offset + searchFrom + 1; i < offset + length; i++)
            {
                if (array[i])
                {
                    return ((ushort)(i - offset), 1);
                }
            }
            
            throw new InvalidOperationException("The range is empty -- the event schedule is invalid");
        }
    }
}