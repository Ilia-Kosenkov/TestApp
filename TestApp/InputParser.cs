#nullable enable
using System;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("TestApp.Test")]
namespace TestApp
{
    internal class InputParser
    {
        public Input Parse(ReadOnlySpan<char> input)
        {
            Input? result = null;
            var prev = 0;
            for (var curr = 0; curr < input.Length; curr++)
            {
                var ch = input[curr];
                if (ch == ',')
                {
                    var temp = ParseListItem(input[prev..curr]);
                    prev = curr + 1;
                    result = result switch
                    {
                        ListInput li => li.Add(temp),
                        null => new ListInput(temp),
                        not null => throw new InvalidOperationException()
                    };
                }
            }

            var lastItem = ParseListItem(input[prev..]);
            return result switch
            {
                null => lastItem,
                ListInput li => li.Add(lastItem),
                _ => throw new InvalidOperationException()
            };
        }

        private Input ParseListItem(ReadOnlySpan<char> input)
        {
            if (input.IsEmpty)
            {
                throw new ArgumentException();
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

            Input? result;
            
            if (rngPos != -1)
            {
                if (
                    uint.TryParse(input[..rngPos], out var lhs) &&
                    uint.TryParse(input[(rngPos + 1)..stepPos], out var rhs) &&
                    lhs <= rhs
                )
                {
                    result = new RangeInput(lhs, rhs);
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            else if (input[0] is '*' && stepPos == 1)
            {
                result = new AnyInput();
            }
            else
            {
                if (uint.TryParse(input[..stepPos], out var val))
                {
                    result = new SingularInput(val);
                }
                else
                {
                    throw new ArgumentException();
                }
            }

            if (stepPos == input.Length)
            {
                return result;
            }

            if (stepPos is var _ && (uint.TryParse(input[(stepPos + 1)..], out var stepBy) && stepBy > 0))
            {
                return new StepByInput(result, stepBy);
            }
            
            throw new ArgumentException();
        }
    }
}