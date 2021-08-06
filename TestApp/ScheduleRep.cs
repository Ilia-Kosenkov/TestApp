using System;
using System.Text;

#nullable enable

namespace TestApp
{
    internal record ScheduleRep()
    {
        public Input Years { get; init; } = AnyInput.Any;
        public Input Months { get; init; } = AnyInput.Any;
        public Input Days { get; init; } = AnyInput.Any;
        
        public Input WeekDays { get; init; } = AnyInput.Any;
        
        public Input Hours { get; init; } = AnyInput.Any;
        public Input Minutes { get; init; } = AnyInput.Any;
        public Input Seconds { get; init; } = AnyInput.Any;

        public Input Milliseconds { get; init; } = AnyInput.Any;

        public void Validate()
        {
            Years.Validate(2000, 2100);
            Months.Validate(1, 12);
            Days.Validate(1, 32);
            
            WeekDays.Validate(0, 6);
            
            Hours.Validate(0, 23);
            Minutes.Validate(0, 59);
            Seconds.Validate(0, 59);
            Milliseconds.Validate(0, 999);
        }
        
        public override string ToString() =>
            $"{Years}.{Months}.{Days} {WeekDays} {Hours}:{Minutes}:{Seconds}.{Milliseconds}";

        public string ToString(bool expand)
        {
            if (expand)
            {
                return new StringBuilder(128)
                    .Append(Years.ToString(4, 2000, 2100)).Append('.')
                    .Append(Months.ToString(2, 1, 12)).Append('.')
                    .Append(Days.ToString(2, 1, 32)).Append(' ')
                    .Append(WeekDays.ToString(1, 0, 6)).Append(' ')
                    .Append(Hours.ToString(2, 0, 23)).Append(':')
                    .Append(Minutes.ToString(2, 0, 59)).Append(':')
                    .Append(Seconds.ToString(2, 0, 59)).Append('.')
                    .Append(Milliseconds.ToString(3, 0, 999))
                    .ToString();
            }

            return ToString();
        }
    }
}