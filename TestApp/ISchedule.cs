#nullable enable

using System;

namespace TestApp
{
    public interface ISchedule
    {
        /// <summary>
        /// Возвращает следующий ближайший к заданному времени момент в расписании или
        /// само заданное время, если оно есть в расписании.
        /// </summary>
        /// <param name="t1">Заданное время</param>
        /// <returns>Ближайший момент времени в расписании</returns>
        DateTime NearestEvent(DateTime t1);

        
        /// <summary>
        /// Возвращает предыдущий ближайший к заданному времени момент в расписании или
        /// само заданное время, если оно есть в расписании.
        /// </summary>
        /// <param name="t1">Заданное время</param>
        /// <returns>Ближайший момент времени в расписании</returns>
        DateTime NearestPrevEvent(DateTime t1);

        /// <summary>
        /// Возвращает следующий момент времени в расписании.
        /// </summary>
        /// <param name="t1">Время, от которого нужно отступить</param>
        /// <returns>Следующий момент времени в расписании</returns>
        DateTime NextEvent(DateTime t1);

        /// <summary>
        /// Возвращает предыдущий момент времени в расписании.
        /// </summary>
        /// <param name="t1">Время, от которого нужно отступить</param>
        /// <returns>Предыдущий момент времени в расписании</returns>
        DateTime PrevEvent(DateTime t1);

        bool IsOnSchedule(DateTime t1);
    }
}