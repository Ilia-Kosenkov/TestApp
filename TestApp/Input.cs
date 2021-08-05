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

    internal record SingularInput(int Value) : Input
    {
    }

    internal record RangeInput(int LowerLimit, int UpperLimit) : Input
    {
    }

    internal record StepByInput(Input ValueRange, int StepBy) : Input
    {
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