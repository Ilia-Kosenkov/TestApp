﻿#nullable enable
using System;
using System.Collections.Immutable;

namespace TestApp
{
    public class InputParser : IParser
    {
        ///     HH:mm:ss                       2
        ///     HH:mm:ss.fff                   3
        ///     yyyy.MM.dd HH:mm:ss            5
        ///     yyyy.MM.dd HH:mm:ss.fff        6
        ///     yyyy.MM.dd w HH:mm:ss          6
        ///     yyyy.MM.dd w HH:mm:ss.fff      7
        public ScheduleRep Parse(ReadOnlySpan<char> input)
        {
            Span<int> sepPositions = stackalloc int[9]; // 7 from the input plus lower and upper limit
            Span<char> sepTypes = stackalloc char[7];
            sepPositions[0] = -1;
            var sepCount = 0;

            for (var i = 0; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case >= '0' and <= '9' or '*' or '/' or ',' or '-':
                        continue;
                    case var ch and ('.' or ' ' or ':'):
                        sepTypes[sepCount] = ch;
                        sepPositions[++sepCount] = i;
                        break;
                    case var ch:
                        throw new ArgumentException($"Invalid symbol '{ch}'");
                }

                if (sepCount == 8)
                {
                    throw new ArgumentException("Too many separators ('.', ':' or ' ') in the string");
                }
            }

            sepPositions[sepCount + 1] = input.Length;
            sepPositions = sepPositions.Slice(0, sepCount + 2);
            sepTypes = sepTypes[..sepCount];
            // Match with the number of actually found separators
            return (sepCount switch
            {
                2 when sepTypes.SequenceEqual(stackalloc char[] { ':', ':' }) => Parse_HH_mm_ss(input, sepPositions),
                3 when sepTypes.SequenceEqual(stackalloc char[] { ':', ':', '.' }) => Parse_HH_mm_ss_fff(
                    input, sepPositions
                ),
                5 when sepTypes.SequenceEqual(stackalloc char[] { '.', '.', ' ', ':', ':' }) =>
                    Parse_yyyy_MM_dd_HH_mm_ss(input, sepPositions),
                6 when sepTypes.SequenceEqual(stackalloc char[] { '.', '.', ' ', ' ', ':', ':' }) =>
                    Parse_yyyy_MM_dd_w_HH_mm_ss(input, sepPositions),
                6 when sepTypes.SequenceEqual(stackalloc char[] { '.', '.', ' ', ':', ':', '.' }) =>
                    Parse_yyyy_MM_dd_HH_mm_ss_fff(input, sepPositions),
                7 when sepTypes.SequenceEqual(stackalloc char[] { '.', '.', ' ', ' ', ':', ':', '.' }) =>
                    Parse_yyyy_MM_dd_w_HH_mm_ss_fff(input, sepPositions),
                _ => throw new ArgumentException()
            }).Validate();
        }
        
        private Input ParseElement(ReadOnlySpan<char> input)
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

        private ScheduleRep Parse_HH_mm_ss(ReadOnlySpan<char> @string, ReadOnlySpan<int> seps) =>
            new()
            {
                Hours = ParseElement(@string[(seps[0] + 1)..seps[1]]),
                Minutes = ParseElement(@string[(seps[1] + 1)..seps[2]]),
                Seconds = ParseElement(@string[(seps[2] + 1)..seps[3]])
            };

        private ScheduleRep Parse_HH_mm_ss_fff(ReadOnlySpan<char> @string, ReadOnlySpan<int> seps) =>
            Parse_HH_mm_ss(@string[..seps[3]], seps[..4]) with
            {
                Milliseconds = ParseElement(@string[(seps[3] + 1)..seps[4]])
            };
        
        private ScheduleRep Parse_yyyy_MM_dd(ReadOnlySpan<char> @string, ReadOnlySpan<int> seps) =>
            new()
            {
                Years = ParseElement(@string[(seps[0] + 1)..seps[1]]),
                Months = ParseElement(@string[(seps[1] + 1)..seps[2]]),
                Days = ParseElement(@string[(seps[2] + 1)..seps[3]])
            };

        private ScheduleRep Parse_yyyy_MM_dd_HH_mm_ss(ReadOnlySpan<char> @string, ReadOnlySpan<int> seps)
        {
            var ymd = Parse_yyyy_MM_dd(@string[..seps[3]], seps[..4]);
            var hms = Parse_HH_mm_ss(@string, seps[3..]);

            return ymd with
            {
                Hours = hms.Hours,
                Minutes = hms.Minutes,
                Seconds = hms.Seconds
            };
        }
           
        private ScheduleRep Parse_yyyy_MM_dd_w_HH_mm_ss(ReadOnlySpan<char> @string, ReadOnlySpan<int> seps)
        {
            var ymd = Parse_yyyy_MM_dd(@string[..seps[3]], seps[..4]);
            return Parse_HH_mm_ss(@string, seps[4..]) with
            {
                Years = ymd.Years,
                Months = ymd.Months,
                Days = ymd.Days,
                
                WeekDays = ParseElement(@string[(seps[3] + 1)..seps[4]])
            };
        }
        
        private ScheduleRep Parse_yyyy_MM_dd_HH_mm_ss_fff(ReadOnlySpan<char> @string, ReadOnlySpan<int> seps)
        {
            var ymd = Parse_yyyy_MM_dd(@string[..seps[3]], seps[..4]);
            return Parse_HH_mm_ss_fff(@string, seps[3..]) with
            {
                Years = ymd.Years,
                Months = ymd.Months,
                Days = ymd.Days
            };
        }
        
        private ScheduleRep Parse_yyyy_MM_dd_w_HH_mm_ss_fff(ReadOnlySpan<char> @string, ReadOnlySpan<int> seps)
        {
            var ymd = Parse_yyyy_MM_dd(@string[..seps[3]], seps[..4]);
            return Parse_HH_mm_ss_fff(@string, seps[4..]) with
            {
                Years = ymd.Years,
                Months = ymd.Months,
                Days = ymd.Days,
                
                WeekDays = ParseElement(@string[(seps[3] + 1)..seps[4]])
            };
        }
    }
}