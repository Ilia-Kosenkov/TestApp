#nullable enable
namespace TestApp
{
    public class ValidationException : System.Exception
    {
        public System.Type ThrowerType { get; }
        public ushort Value { get; }
        public ushort TestedLowerLimit { get; }
        public ushort TestedUpperLimit { get; }

        public ValidationException(System.Type type, ushort value, ushort lowerLimit, ushort upperLimit) :
            base("Input failed validation for particular range of values")
        {
            ThrowerType = type;
            Value = value;
            TestedLowerLimit = lowerLimit;
            TestedUpperLimit = upperLimit;
        }
    }
}