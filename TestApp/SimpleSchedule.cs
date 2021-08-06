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
            
            rep.Years.SetBits(_bitRep, YearOffset, 2000, 2100);
            rep.Months.SetBits(_bitRep, MonthOffset, 1, 12);
            rep.Days.SetBits(_bitRep, DayOffset, 1, 32);
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
            Span<ushort> inputs = stackalloc ushort[]
                { (ushort)t1.Year, (ushort)t1.Month, (ushort)t1.Day, (ushort)t1.Hour, (ushort)t1.Minute, (ushort)t1.Second, (ushort)t1.Millisecond };
            Span<ushort> vals = stackalloc ushort[7];
            Span<ushort> carryOvers = stackalloc ushort[7];
            Span<ushort> mins = stackalloc ushort[7];

            
            
            (vals[6], carryOvers[6], mins[6]) = _bitRep.NextFlag(MillisecondOffset, BitLength - MillisecondOffset, t1.Millisecond + 1);
            
            (vals[5], carryOvers[5], mins[5]) = _bitRep.NextFlag(
                SecondOffset, MillisecondOffset - SecondOffset, t1.Second + carryOvers[6]
            );
            (vals[4], carryOvers[4], mins[4]) = _bitRep.NextFlag(MinuteOffset, SecondOffset - MinuteOffset, t1.Minute + carryOvers[5]);
            (vals[3], carryOvers[3], mins[3]) = _bitRep.NextFlag(HourOffset, MinuteOffset - HourOffset, t1.Hour + carryOvers[4]);
            
            
            // This is suboptimal, should think of something else
            // This needs a first month/year approximation, then iterate over months/years that match condition, not all month/years
            var monthIterator = t1.Month - 1;
            var dayAfter = t1.Day + carryOvers[3];
            int day;
            while ((day = GetNextValidDayInMonth(t1.Year + monthIterator / 12, monthIterator % 12 + 1, dayAfter)) == -1)
            {
                monthIterator++;
                dayAfter = 0;
            }

            vals[2] = (ushort)day;
            carryOvers[2] = (ushort)(monthIterator != t1.Month ? 1 : 0);
            mins[2] = (ushort)GetNextValidDayInMonth(t1.Year + monthIterator / 12, monthIterator % 12 + 1, 0);
            
            (vals[1], carryOvers[1], mins[1]) = _bitRep.NextFlag(MonthOffset, DayOffset - MonthOffset, monthIterator % 12);
            vals[1] += 1;
            mins[1] += 1;
            
            (vals[0], carryOvers[0], mins[0]) = _bitRep.NextFlag(YearOffset, MonthOffset - YearOffset, t1.Year + monthIterator / 12 + carryOvers[1] - 2000);
            vals[0] += 2000;
            mins[0] += 2000;

            mins[2] = (ushort)GetNextValidDayInMonth(vals[0], vals[1], 0);
            
            
            var wasModified = false;
            for (var i = 0; i < vals.Length; i++)
            {
                if (wasModified)
                {
                    vals[i] = mins[i];
                }
                else
                {
                    if (vals[i] != inputs[i])
                    {
                        wasModified = true;
                    }
                }
            }

            return new DateTime(vals[0], vals[1], vals[2], vals[3], vals[4], vals[5], vals[6]);
        }

        public DateTime PrevEvent(DateTime t1) => throw new NotImplementedException();
        public bool IsOnSchedule(DateTime @event)
        {
            return _bitRep.Get(YearOffset + @event.Year - 2000) &&
                   _bitRep.Get(MonthOffset + @event.Month - 1) &&
                   _bitRep.Get(DayOffset + @event.Day - 1) && 
                   _bitRep.Get(WeekDayOffset + @event.DayOfWeek.AsInt()) &&
                   _bitRep.Get(HourOffset + @event.Hour) &&
                   _bitRep.Get(MinuteOffset + @event.Minute) &&
                   _bitRep.Get(SecondOffset + @event.Second) &&
                   _bitRep.Get(MillisecondOffset + @event.Millisecond);
        }


        private int GetNextValidDayInMonth(int year, int month, int nextAfter)
        {
            var nDays = DateTime.DaysInMonth(year, month);
            for (var i = nextAfter + 1; i <= nDays; i++)
            {
                var day = new DateTime(year, month, i);
                // Match day of week
                if (_bitRep.Get(WeekDayOffset + day.DayOfWeek.AsInt()))
                {
                    // Match exact day or last day of month if 32-d bit is set
                    if (_bitRep.Get(DayOffset + i - 1) || i == nDays && _bitRep.Get(DayOffset + 31))
                    {
                       return i;
                    }

                }
            }
            return -1;
            
        }
    }
}