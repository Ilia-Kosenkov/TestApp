#nullable enable

using System;

namespace TestApp
{
    public class SimpleSchedule : ISchedule
    {
        public DateTime NearestEvent(DateTime t1) => throw new NotImplementedException();

        public DateTime NearestPrevEvent(DateTime t1) => throw new NotImplementedException();

        public DateTime NextEvent(DateTime t1) => throw new NotImplementedException();

        public DateTime PrevEvent(DateTime t1) => throw new NotImplementedException();
    }
}