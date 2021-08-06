using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

#nullable enable
namespace TestApp
{
    internal abstract record Input
    {
        public abstract bool TryWriteValues(Span<ushort> buffer, ushort upperLimit, out uint valsWritten);

        public abstract void Validate(ushort lowerLimit, ushort upperLimit);
    }

    internal record SingularInput(ushort Value) : Input
    {
        public override bool TryWriteValues(Span<ushort> buffer, ushort upperLimit, out uint valsWritten)
        {
            if (buffer.Length > 1)
            {
                buffer[0] = Value;
                valsWritten = 1;
                return true;
            }
            valsWritten = 0;
            return false;
        }

        public override void Validate(ushort lowerLimit, ushort upperLimit)
        {
            if (Value < lowerLimit || Value > upperLimit)
            {
                throw new ValidationException(typeof(SingularInput), Value, lowerLimit, upperLimit);
            }
        }

        public override string ToString() => Value.ToString();
    }

    internal abstract record RangeInput : Input;

    internal record AnyInput : RangeInput
    {
        public static AnyInput Any { get; } = new();

        public override bool TryWriteValues(Span<ushort> buffer, ushort upperLimit, out uint valsWritten)
        {
            if (buffer.Length >= upperLimit)
            {
                for (ushort i = 0; i < upperLimit; i++)
                {
                    buffer[i] = i;
                }

                valsWritten = upperLimit;
                return true;
            }

            valsWritten = 0;
            return false;
        }

        public override string ToString() => "*";

        public override void Validate(ushort lowerLimit, ushort upperLimit)
        {
            // This range is always valid
        }
    }
    internal record ValueRangeInput(ushort LowerLimit, ushort UpperLimit) : RangeInput
    {
        public override bool TryWriteValues(Span<ushort> buffer, ushort upperLimit, out uint valsWritten)
        {
            if (buffer.Length >= UpperLimit - LowerLimit)
            {
                for(var i = LowerLimit; i <= UpperLimit; i++)
                {
                    buffer[i - LowerLimit] = i;
                }

                valsWritten = (uint)(UpperLimit - LowerLimit + 1);
                return true;
            }

            valsWritten = 0;
            return false;
        }

        public override void Validate(ushort lowerLimit, ushort upperLimit)
        {
            if (LowerLimit < lowerLimit)
            {
                throw new ValidationException(typeof(ValueRangeInput), LowerLimit, lowerLimit, upperLimit);
            }
            
            if(UpperLimit > upperLimit)
            {
                throw new ValidationException(typeof(ValueRangeInput), UpperLimit, lowerLimit, upperLimit);
            }
        }

        public override string ToString() => $"{LowerLimit}-{UpperLimit}";
    }

    internal record StepByInput(RangeInput ValueRange, ushort StepBy) : Input
    {
        public override bool TryWriteValues(Span<ushort> buffer, ushort upperLimit, out uint valsWritten)
        {
            valsWritten = 0;
            (ushort from, ushort to) = ValueRange switch
            {
                AnyInput => (default, upperLimit),
                ValueRangeInput {LowerLimit : var lhs, UpperLimit : var rhs} => (lhs, (ushort)(rhs + 1)),
                _ => (default(ushort), default(ushort))
            };

            if (buffer.Length >= (to - from - 1) / StepBy)
            {
                var val = from;
                var i = 0;
                for(; val < to; val += StepBy, i++)
                {
                    buffer[i] = val;
                }

                valsWritten = (uint)i;
                return true;
            }

            return false;
        }

        public override void Validate(ushort lowerLimit, ushort upperLimit)
        {
            // Valid if inner range is valid
            try
            {
                ValueRange.Validate(lowerLimit, upperLimit);
            }
            catch (ValidationException vex)
            {
                // Modify exception type
                throw new ValidationException(
                    typeof(StepByInput), vex.Value, vex.TestedLowerLimit, vex.TestedUpperLimit
                );
            }
        }

        public override string ToString() => $"{ValueRange}/{StepBy}";
    }

    internal record ListInput : Input
    {
        public ImmutableArray<Input> Items { get; }

        public ListInput(ReadOnlySpan<Input> input)
        {
            var builder = ImmutableArray.CreateBuilder<Input>(input.Length);
            foreach (var item in input)
            {
                builder.Add(item);
            }

            Items = builder.ToImmutable();
        }

        public ListInput(ImmutableArray<Input>.Builder array) => Items = array.MoveToImmutable();

        public ListInput(params Input[] input) => Items = input.ToImmutableArray();

        public ListInput(IEnumerable<Input> input)
        {
            Items = input.ToImmutableArray();
        }

        public override bool TryWriteValues(Span<ushort> buffer, ushort upperLimit, out uint valsWritten)
        {
            valsWritten = 0;
            foreach (var item in Items)
            {
                if (!item.TryWriteValues(buffer, upperLimit, out var nVals))
                {
                    return false;
                }

                buffer = buffer.Slice((int)nVals);
                valsWritten += nVals;
            }

            return true;
        }

        public override void Validate(ushort lowerLimit, ushort upperLimit)
        {
            try
            {
                foreach (var item in Items)
                {
                    item.Validate(lowerLimit, upperLimit);
                }
            }
            catch (ValidationException vex)
            {
                throw new ValidationException(typeof(ListInput), vex.Value, vex.TestedLowerLimit, vex.TestedUpperLimit);
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder(Items.Length * 4);

            builder.AppendJoin(',', Items);
            
            return builder.ToString();
        }
    }
}