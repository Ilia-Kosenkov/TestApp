#nullable enable
using System;

namespace TestApp
{
    public interface IParser
    {
        ScheduleRep Parse(ReadOnlySpan<char> input);
    }
}