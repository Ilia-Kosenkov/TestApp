#nullable enable

using System;

namespace TestApp
{
    public class SimpleSchedule : ISchedule
    {
        private readonly ScheduleRep _scheduleRepresentation;
        public SimpleSchedule() => _scheduleRepresentation = new ScheduleRep();

        public SimpleSchedule(ScheduleRep rep) => _scheduleRepresentation = rep ?? throw new ArgumentNullException(nameof(rep));

        public DateTime NearestEvent(DateTime t1) => throw new NotImplementedException();

        public DateTime NearestPrevEvent(DateTime t1) => throw new NotImplementedException();

        public DateTime NextEvent(DateTime t1) => throw new NotImplementedException();

        public DateTime PrevEvent(DateTime t1) => throw new NotImplementedException();
    }
}