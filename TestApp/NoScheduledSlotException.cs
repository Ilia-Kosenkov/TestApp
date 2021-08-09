#nullable enable
using System;

namespace TestApp
{
    public abstract class NoScheduledSlotException : Exception
    {
        protected NoScheduledSlotException(string message) : base(message)
        {
        }
    }

    public class NoNextScheduledSlotException : NoScheduledSlotException
    {
        public NoNextScheduledSlotException() : base("The schedule has no slots after the specified date.")
        {
        }
    }
    
    public class NoPreviousScheduledSlotException : NoScheduledSlotException
    {
        public NoPreviousScheduledSlotException() : base("The schedule has no slots before the specified date.")
        {
        }
    }
}