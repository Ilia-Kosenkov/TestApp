#nullable enable
using System;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("TestApp.Test")]
namespace TestApp
{
    internal class InputParser
    {
        public void Parse(ReadOnlySpan<char> input)
        {
            
        }
        
        public Input ParseElement(ReadOnlySpan<char> input)
        {
            if (input.IsEmpty)
            {
                throw new ArgumentException("The input is empty");
            }
            
            Input? result = null;
            var prev = 0;
            for (var curr = 0; curr < input.Length; curr++)
            {
                var ch = input[curr];
                if (ch == ',')
                {
                    var temp = ParseElementListItem(input[prev..curr]);
                    prev = curr + 1;
                    result = result switch
                    {
                        ListInput li => li.Add(temp),
                        null => new ListInput(temp),
                        not null => throw new InvalidOperationException("Invalid parsing state, should not happen")
                    };
                }
            }

            var lastItem = ParseElementListItem(input[prev..]);
            return result switch
            {
                null => lastItem,
                ListInput li => li.Add(lastItem),
                _ => throw new InvalidOperationException("Invalid parsing state, should not happen")
            };
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
                    result = new AnyInput();
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