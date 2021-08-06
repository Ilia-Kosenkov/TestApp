using System;

#nullable enable

namespace TestApp
{
    public class SimpleScheduleProvider : IScheduleProvider
    {
        private readonly IParser _parser;
        public SimpleScheduleProvider(IParser parser) => _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        public ISchedule GetSchedule() => new SimpleSchedule();
        public ISchedule GetSchedule(string scheduleString) => new SimpleSchedule(
            _parser.Parse(
                scheduleString ?? throw new ArgumentNullException(nameof(scheduleString))
            )
        );
    }
}