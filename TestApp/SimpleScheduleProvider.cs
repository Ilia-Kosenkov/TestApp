#nullable enable

namespace TestApp
{
    public class SimpleScheduleProvider : IScheduleProvider
    {
        public ISchedule GetSchedule() => throw new System.NotImplementedException();

        public ISchedule GetSchedule(string scheduleString) => throw new System.NotImplementedException();
    }
}