#nullable enable
using System;

namespace TestApp
{
    public interface IParser
    {
        /// <summary>
        /// Parses input string and produces a representation of schedule.
        /// </summary>
        /// <param name="input">A string representing a valid schedule.</param>
        /// <returns>Representation of schedule.</returns>
        /// <exception cref="System.ArgumentException">Thrown if input is malformed string.</exception>
        /// <exception cref="TestApp.ValidationException">Thrown if schedule is invalid.</exception>
        ScheduleRep Parse(ReadOnlySpan<char> input);
    }
}