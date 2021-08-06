#nullable enable
using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("TestApp.Test")]
namespace TestApp
{
    internal class InputParser
    {
        public ScheduleRep Parse(ReadOnlySpan<char> input)
        {
            throw new NotImplementedException();
        }
        
        public Input ParseElement(ReadOnlySpan<char> input)
        {
            if (input.IsEmpty)
            {
                throw new ArgumentException("The input is empty");
            }

            var nElems = 1;
            foreach (var ch in input)
            {
                if (ch == ',')
                {
                    nElems++;
                }
            }

            if (nElems == 1)
            {
                return ParseElementListItem(input);
            }

            var builder = ImmutableArray.CreateBuilder<Input>(nElems);
            
            var prev = 0;
            for (var curr = 0; curr < input.Length; curr++)
            {
                var ch = input[curr];
                if (ch == ',')
                {
                    builder.Add(ParseElementListItem(input[prev..curr]));
                    prev = curr + 1;
                }
            }

            builder.Add(ParseElementListItem(input[prev..]));

            return new ListInput(builder);
        }

        private Input ParseElementListItem(ReadOnlySpan<char> input)
        {
            if (input.IsEmpty)
            {
                throw new ArgumentException("The input is empty");
            }

            var rngPos = -1;
            var stepPos = input.Length;

            for (var i = 0; i < input.Length; i++)
            {
                var ch = input[i];
                if (ch is '-')
                {
                    rngPos = i;
                } 
                else if (ch is '/')
                {
                    stepPos = i;
                }
            }

            // Handles single value input, like `42`
            if (rngPos == -1 && stepPos == input.Length && ushort.TryParse(input[..stepPos], out var val))
            {
                return new SingularInput(val);
            }
            
            RangeInput? result;
            
            // Handles 'any' input, like `*` and `*/123`
            if (rngPos == -1)
            {
                if (input[0] is '*' && stepPos == 1)
                {
                    result = AnyInput.Any;
                }
                else
                {
                    throw new ArgumentException("Failed to parse '*' range");
                }
            }
            // Handles a case of range, e.g. `5-10` or `5-10/2`
            else
            {
                if (
                    ushort.TryParse(input[..rngPos], out var lhs) &&
                    ushort.TryParse(input[(rngPos + 1)..stepPos], out var rhs) &&
                    lhs <= rhs
                )
                {
                    result = new ValueRangeInput(lhs, rhs);
                }
                else
                {
                    throw new ArgumentException("Failed to parse 'x-y' range");
                }
            }

            if (stepPos == input.Length)
            {
                return result;
            }

            if (stepPos < input.Length && ushort.TryParse(input[(stepPos + 1)..], out var stepBy) && stepBy > 0)
            {
                return new StepByInput(result, stepBy);
            }
            
            throw new ArgumentException();
        }
    }
}