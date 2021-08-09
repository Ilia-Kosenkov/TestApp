using System;

#nullable enable
namespace TestApp.Test
{
    internal class OldScheduleImplementationProvider : IScheduleProvider
    {
        public ISchedule GetSchedule() => new OldScheduleAdapter();

        public ISchedule GetSchedule(string scheduleString) => new OldScheduleAdapter(scheduleString);
    }
    
    internal class OldScheduleAdapter : ISchedule
    {
        private readonly Schedule _oldSchedule;

        public OldScheduleAdapter() => _oldSchedule = new Schedule();
        public OldScheduleAdapter(string input) => _oldSchedule = new Schedule(input);

        public DateTime NearestEvent(DateTime t1) => _oldSchedule.NearestEvent(t1);

        public DateTime NearestPrevEvent(DateTime t1) => _oldSchedule.NearestPrevEvent(t1);

        public DateTime NextEvent(DateTime t1) => _oldSchedule.NextEvent(t1);

        public DateTime PrevEvent(DateTime t1) => _oldSchedule.PrevEvent(t1);
    }
}