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
    }
}