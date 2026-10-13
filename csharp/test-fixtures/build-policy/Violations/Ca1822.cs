namespace BuildPolicy;

/// <summary>Supplies an instance method with no instance state to trigger the CA1822 fixture.</summary>
internal sealed class Ca1822
{
    /// <summary>Doubles its argument while deliberately leaving the receiver unused.</summary>
    internal int Twice(int value) => value * 2;
}
