namespace Fixture;

/// <summary>Holds a value this part's primary constructor takes.</summary>
/// <param name="value">The value.</param>
public sealed partial class Holder(int value)
{
    /// <summary>The value.</summary>
    public int Value { get; } = value;
}
