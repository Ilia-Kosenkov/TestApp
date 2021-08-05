using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace TestApp
{
    internal abstract record Input
    {
        
    }

    internal record AnyInput : Input
    {
    }

    internal record SingularInput : Input
    {
        public int Value { get; init; }
    }

    internal record RangeInput : Input
    {
        public int LowerLimit { get; init; }
        public int UpperLimit { get; init; }
    }

    internal record StepByInput : Input
    {
        public Input ValueRange { get; init; } = new AnyInput();
        public int StepBy { get; init; }
    }

    internal record ListInput : Input
    {
        private readonly Input[] _elements = Array.Empty<Input>();

        public ListInput(params Input[] input)
        {
            if (_elements.Length != input.Length)
            {
                _elements = new Input[input.Length];
            }

            input.CopyTo(_elements.AsSpan());
        }

        public ListInput(IEnumerable<Input> input) : this(input.ToArray())
        {
        }
    }
}