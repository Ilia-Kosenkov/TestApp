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
        private readonly List<Input> _elements;

        public IReadOnlyCollection<Input> Items => _elements;

        public ListInput(params Input[] input)
        {
            _elements = new List<Input>(input.Length);
            foreach (var item in input)
            {
                _elements.Add(item);
            }
        }

        public ListInput(IEnumerable<Input> input) 
        {
            _elements = input.ToList();
        }

        public ListInput Add(Input i)
        {
            _elements.Add(i);
            return this;
        }
    }
}