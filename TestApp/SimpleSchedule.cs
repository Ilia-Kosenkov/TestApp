#nullable enable

using System;
using System.Collections;

namespace TestApp
{
    public class SimpleSchedule : ISchedule
    {
        private const int YearOffset = 0;
        private const int MonthOffset = YearOffset + 101;
        private const int DayOffset = MonthOffset + 12;
        private const int WeekDayOffset = DayOffset + 32;
        private const int HourOffset = WeekDayOffset + 7;
        private const int MinuteOffset = HourOffset + 24;
        private const int SecondOffset = MinuteOffset + 60;
        private const int MillisecondOffset = SecondOffset + 60;
        private const int BitLength = MillisecondOffset + 1000;
        
        private readonly BitArray _bitRep = new(BitLength);
        
        public SimpleSchedule() => _bitRep.SetAll(true);

        public SimpleSchedule(ScheduleRep rep)
        {
            _ = rep ?? throw new ArgumentNullException(nameof(rep));
            
            rep.Years.SetBits(_bitRep, YearOffset, Date.YearOffset, 2100);
            rep.Months.SetBits(_bitRep, MonthOffset, Date.MonthOffset, 12);
            rep.Days.SetBits(_bitRep, DayOffset, Date.DayOffset, 32);
            rep.WeekDays.SetBits(_bitRep, WeekDayOffset, 0, 6);
            rep.Hours.SetBits(_bitRep, HourOffset, 0, 23);
            rep.Minutes.SetBits(_bitRep, MinuteOffset, 0, 59);
            rep.Seconds.SetBits(_bitRep, SecondOffset, 0, 59);
            rep.Milliseconds.SetBits(_bitRep, MillisecondOffset, 0, 999);
        }

        public DateTime NearestEvent(DateTime t1) => IsOnSchedule(t1) ? t1 : NextEvent(t1);

        public DateTime NearestPrevEvent(DateTime t1) => IsOnSchedule(t1) ? t1 : PrevEvent(t1);

        public DateTime NextEvent(DateTime t1)
        {
            var (date, time) = t1.AddMilliseconds(1);
            // Find scheduled date. Can be this (provided) day.
            var scheduledDate =  GetThisOrNextScheduledDay(date);
            
            // If input date is not on schedule, then return next scheduled date 
            // with the first available time slot.
            if (scheduledDate != date)
            {
                return scheduledDate.WithTime(GetFirstTimeSlotInADay());
            }
            
            // Search for next time slot
            var (scheduledTime, nextDay) = GetThisOrNextScheduledTime(time);
                
            // Time slot is found within the day
            if (!nextDay)
            {
                return scheduledDate.WithTime(scheduledTime);
            }
            // Otherwise, find next day
            date = date.IncDay();
            scheduledDate = GetThisOrNextScheduledDay(date);

            // At this point, `scheduledTime` is the first time slot within a day.
            // Return next scheduled day with the first time slot.
            return scheduledDate.WithTime(scheduledTime);

        }

        public DateTime PrevEvent(DateTime t1)
        {
            var (date, time) = t1.AddMilliseconds(-1);
            // Find scheduled date. Can be this (provided) day.
            var scheduledDate =  GetThisOrPrevScheduledDay(date);
            
            // If input date is not on schedule, then return next scheduled date 
            // with the first available time slot.
            if (scheduledDate != date)
            {
                return scheduledDate.WithTime(GetLastTimeSlotInADay());
            }
            
            // Search for next time slot
            var (scheduledTime, prevDay) = GetThisOrPrevScheduledTime(time);
                
            // Time slot is found within the day
            if (!prevDay)
            {
                return scheduledDate.WithTime(scheduledTime);
            }
            // Otherwise, find previous day
            date = date.DecDay();
            
            scheduledDate = GetThisOrPrevScheduledDay(date);

            // At this point, `scheduledTime` is the first time slot within a day.
            // Return next scheduled day with the first time slot.
            return scheduledDate.WithTime(scheduledTime);
        }


        public bool IsOnSchedule(DateTime @event)
        {
            return _bitRep.Get(YearOffset + @event.Year - Date.YearOffset) &&
                   _bitRep.Get(MonthOffset + @event.Month - Date.MonthOffset) &&
                   (
                        _bitRep.Get(DayOffset + @event.Day - Date.DayOffset) || 
                        _bitRep.Get(DayOffset + 31) && @event.Day == DateTime.DaysInMonth(@event.Year, @event.Month)
                    ) && 
                   _bitRep.Get(WeekDayOffset + (int)@event.DayOfWeek) &&
                   _bitRep.Get(HourOffset + @event.Hour) &&
                   _bitRep.Get(MinuteOffset + @event.Minute) &&
                   _bitRep.Get(SecondOffset + @event.Second) &&
                   _bitRep.Get(MillisecondOffset + @event.Millisecond);
        }
       
        /// <summary>
        /// Finds next scheduled day within a month.
        /// Takes into account Day of Week and Last Day of Month (32d day) conditions.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="thisDay"></param>
        /// <returns><c>0</c>-based day if it was found, <c>-1</c> if no scheduled days can be found in this month.</returns>
        private int GetThisOrNextValidDayInMonth(ushort year, ushort month, int thisDay)
        {
            var day = new Date { Year = (byte)year, Month = (byte)month, Day = (byte)thisDay };
            var nDays = day.DaysInCurrentMonth;
            for (var i = thisDay; i < nDays; i++)
            {
                day = day.WithDay((sbyte)i); 
                // Match day of week
                if (!_bitRep.Get(WeekDayOffset + day.DayOfWeek))
                {
                    continue;
                }
                // Match exact day or last day of month if 32-d bit is set
                if (_bitRep.Get(DayOffset + i) || i + Date.DayOffset == nDays && _bitRep.Get(DayOffset + 31))
                {
                    return i;
                }
            }
            return -1;
        }
        
        private int GetThisOrPrevValidDayInMonth(ushort year, ushort month, int thisDay)
        {
            var day = new Date { Year = (byte)year, Month = (byte)month, Day = (byte)thisDay };
            var nDays = day.DaysInCurrentMonth;
            for (var i = thisDay; i >= 0; i--)
            {
                day = day.WithDay((sbyte)i); 
                // Match day of week
                if (!_bitRep.Get(WeekDayOffset + day.DayOfWeek))
                {
                    continue;
                }
                // Match exact day or last day of month if 32-d bit is set
                if (_bitRep.Get(DayOffset + i) || i + Date.DayOffset == nDays && _bitRep.Get(DayOffset + 31))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Finds next scheduled date (Year/Month/Day), ignoring time.
        /// Takes into account Day of Week and Last Day of Month (32d day) conditions.
        /// </summary>
        /// <param name="date">Date from which the search is performed</param>
        /// <returns>The next scheduled date. Can be <c>date</c> if it is on schedule.</returns>
        /// <exception cref="System.InvalidOperationException">Functions called by this method throw if bit representation is invalid (e.g. when no hours are set in the schedule).
        /// Throwing functions are <see cref="TestApp.Helper.ThisOrNextValue">ThisOrNextValue</see> and <see cref="TestApp.Helper.MinValue">MinValue</see> 
        /// </exception>
        /// <exception cref="TestApp.NoNextScheduledSlotException">Thrown if there are no slots available on or after provided <c>date</c>.</exception>
        private Date GetThisOrNextScheduledDay(Date date)
        {
            // This gives the first (year, month) pair to start search from.
            var (month, mCarry) = _bitRep.ThisOrNextValue(MonthOffset, DayOffset - MonthOffset, date.Month);
            var (year, yCarry) = _bitRep.ThisOrNextValue(YearOffset, MonthOffset - YearOffset, date.Year + mCarry);
            
            if (yCarry == 1)
            {
                throw new NoNextScheduledSlotException();
            }
            
            // If month or year are 'carried over', set `day` to `0`.
            var day = (mCarry, yCarry) switch
            {
                (0, 0) => date.Day,
                _ => 0
            };

            while ((day = GetThisOrNextValidDayInMonth(year, month, day)) == -1)
            {
                
                // Find next month
                (month, mCarry) = _bitRep.ThisOrNextValue(MonthOffset, DayOffset - MonthOffset, month + 1);
                // If no valid months are found in this year, `month` is the first valid month in a year,
                // and next year should be selected
                if (mCarry == 1)
                {
                    (year, yCarry) = _bitRep.ThisOrNextValue(YearOffset, MonthOffset - YearOffset, year + 1);
                    if (yCarry == 1)
                    {
                        throw new NoNextScheduledSlotException();
                    }
                }
                day = 0;
            }

            return new Date { Year = (byte)year, Month = (byte)month, Day = (byte)day };
        }
        
        private Date GetThisOrPrevScheduledDay(Date date)
        {
            // This gives the first (year, month) pair to start search from.
            var (month, mCarry) = _bitRep.ThisOrPrevValue(MonthOffset, DayOffset - MonthOffset, date.Month);
            var (year, yCarry) = _bitRep.ThisOrPrevValue(YearOffset, MonthOffset - YearOffset, date.Year - mCarry);
            
            if (yCarry == 1)
            {
                throw new NoPreviousScheduledSlotException();
            }
            
            // If month or year are 'carried over', set `day` to `0`.
            var day = year == date.Year && month == date.Month
                ? date.Day
                : Date.DaysInMonth((byte)year, (byte)month) - 1;

            while ((day = GetThisOrPrevValidDayInMonth(year, month, day)) == -1)
            {
                
                // Find next month
                (month, mCarry) = _bitRep.ThisOrPrevValue(MonthOffset, DayOffset - MonthOffset, month - 1);
                // If no valid months are found in this year, `month` is the first valid month in a year,
                // and next year should be selected
                if (mCarry == 1)
                {
                    (year, yCarry) = _bitRep.ThisOrPrevValue(YearOffset, MonthOffset - YearOffset, year - 1);
                    if (yCarry == 1)
                    {
                        throw new NoPreviousScheduledSlotException();
                    }
                }

                day = Date.DaysInMonth((byte)year, (byte)month) - 1;
            }

            return new Date { Year = (byte)year, Month = (byte)month, Day = (byte)day };
        }
        
        /// <summary>
        /// Finds next scheduled time slot within a day
        /// </summary>
        /// <param name="time">Time from which the search is performed</param>
        /// <returns>Either <c>(NextSuitableTime, false)</c> if slot is available during the day or
        /// <c>(FirstSlotInADay, true)</c> if no slots are available.</returns>
        /// <exception cref="System.InvalidOperationException">Functions called by this method throw if bit representation is invalid (e.g. when no hours are set in the schedule).
        /// Throwing functions are <see cref="TestApp.Helper.ThisOrNextValue">ThisOrNextValue</see> and <see cref="TestApp.Helper.MinValue">MinValue</see> 
        /// </exception>
        private (Time Time, bool NextDay) GetThisOrNextScheduledTime(Time time)
        {
            // Find next scheduled millisecond
            var (mSec, mSecCarry) = _bitRep.ThisOrNextValue(
                MillisecondOffset, BitLength - MillisecondOffset, time.Millisecond
            );
            // Find next scheduled minute (accounting for carried over unit)
            var (sec, secCarry) = _bitRep.ThisOrNextValue(
                SecondOffset, MillisecondOffset - SecondOffset, time.Second + mSecCarry
            );
            // Same for minutes
            var (min, minCarry) = _bitRep.ThisOrNextValue(
                MinuteOffset, SecondOffset - MinuteOffset, time.Minute + secCarry
            );
            // And hours
            var (hour, hourCarry) = _bitRep.ThisOrNextValue(
                HourOffset, MinuteOffset - HourOffset, time.Hour + minCarry
            );

            // Construct result in the reverse order
            return (
                new Time
                {
                    // As is
                    Hour = (byte)hour,
                    // Either scheduled minutes or first scheduled minute in a day if 
                    // hour was carried over (== next day)
                    Minute = (byte)(
                        hourCarry is 1 
                        ? _bitRep.MinValue(MinuteOffset, SecondOffset - MinuteOffset)
                        : min
                    ),
                    // Same
                    Second = (byte) (
                        hourCarry is 1 || minCarry is 1
                        ? _bitRep.MinValue(SecondOffset, MillisecondOffset - SecondOffset)
                        : sec
                    ),
                    // Same
                    Millisecond = hourCarry is 1 || minCarry is 1 || secCarry is 1
                        ? _bitRep.MinValue(MillisecondOffset, BitLength - MillisecondOffset)
                        : mSec
                },
                // Request for the next day
                hourCarry == 1
            );
        }

        private (Time Time, bool PrevDay) GetThisOrPrevScheduledTime(Time time)
        {
            // Find next scheduled millisecond
            var (mSec, mSecCarry) = _bitRep.ThisOrPrevValue(
                MillisecondOffset, BitLength - MillisecondOffset, time.Millisecond
            );
            // Find next scheduled minute (accounting for carried over unit)
            var (sec, secCarry) = _bitRep.ThisOrNextValue(
                SecondOffset, MillisecondOffset - SecondOffset, time.Second - mSecCarry
            );
            // Same for minutes
            var (min, minCarry) = _bitRep.ThisOrPrevValue(
                MinuteOffset, SecondOffset - MinuteOffset, time.Minute - secCarry
            );
            // And hours
            var (hour, hourCarry) = _bitRep.ThisOrPrevValue(
                HourOffset, MinuteOffset - HourOffset, time.Hour - minCarry
            );

            // Construct result in the reverse order
            return (
                new Time
                {
                    // As is
                    Hour = (byte)hour,
                    // Either scheduled minutes or first scheduled minute in a day if 
                    // hour was carried over (== next day)
                    Minute = (byte)(
                        hour < time.Hour
                        ? _bitRep.MaxValue(MinuteOffset, SecondOffset - MinuteOffset)
                        : min
                    ),
                    // Same
                    Second = (byte) (
                        hour < time.Hour || min < time.Minute
                        ? _bitRep.MaxValue(SecondOffset, MillisecondOffset - SecondOffset)
                        : sec
                    ),
                    // Same
                    Millisecond =   hour < time.Hour || min < time.Minute || sec < time.Second
                        ? _bitRep.MaxValue(MillisecondOffset, BitLength - MillisecondOffset)
                        : mSec
                },
                // Request for the previous day
                hourCarry == 1
            );
        }

        private Time GetFirstTimeSlotInADay() =>
            new()
            {
                Hour = (byte)_bitRep.MinValue(HourOffset, MinuteOffset - HourOffset),
                Minute = (byte)_bitRep.MinValue(MinuteOffset, SecondOffset - MinuteOffset),
                Second = (byte) _bitRep.MinValue(SecondOffset, MillisecondOffset - SecondOffset),
                Millisecond = _bitRep.MinValue(MillisecondOffset, BitLength - MillisecondOffset),
            };
        
        private Time GetLastTimeSlotInADay() =>
            new()
            {
                Hour = (byte)_bitRep.MaxValue(HourOffset, MinuteOffset - HourOffset),
                Minute = (byte)_bitRep.MaxValue(MinuteOffset, SecondOffset - MinuteOffset),
                Second = (byte) _bitRep.MaxValue(SecondOffset, MillisecondOffset - SecondOffset),
                Millisecond = _bitRep.MaxValue(MillisecondOffset, BitLength - MillisecondOffset),
            };
    }
}