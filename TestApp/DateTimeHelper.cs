using System;
using System.Collections;

#nullable enable
namespace TestApp
{
    internal static class Helper
    {
        public static int AsInt(this DayOfWeek dow) =>
            dow switch
            {
                DayOfWeek.Sunday => 0,
                DayOfWeek.Monday => 1,
                DayOfWeek.Tuesday => 2,
                DayOfWeek.Wednesday => 3,
                DayOfWeek.Thursday => 4,
                DayOfWeek.Friday => 5,
                DayOfWeek.Saturday => 6,
                _ => throw new InvalidOperationException()
            };

        public static (ushort Next, ushort CarryOver, ushort Min) NextFlag(this BitArray array, int offset, int length, int searchFrom)
        {
            var min = -1;
            for (var i = offset; i < offset + length; i++)
            {
                if (!array[i])
                {
                    continue;
                }
                min = i - offset;
                break;
            }

            if (min == -1)
            {
                throw new InvalidOperationException("Unable to find any scheduled time");
            }

            for (var i = offset + searchFrom; i < offset + length; i++)
            {
                if (array[i])
                {
                    return ((ushort)(i - offset), 0, (ushort)min);
                }
            }

            return ((ushort)min, 1, (ushort)min);
        }
    }
}