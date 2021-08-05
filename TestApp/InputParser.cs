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
            var prev = 0;
            var curr = 0;
            Input? result = null;
            while (curr < input.Length)
            {
                var ch = input[curr++];
                if (char.IsDigit(ch))
                {
                    continue;
                }

                if (ch == ',')
                {
                    // Parse(input[prev..curr]);
                    prev = curr;
                }
            }

            if (input[prev..curr].Trim().Equals("*", StringComparison.OrdinalIgnoreCase))
            {
                result = new AnyInput();
            }

            if (int.TryParse(input[prev..curr], out var intVal))
            {
                result = new SingularInput() { Value = intVal };
            }

        }
    }
}