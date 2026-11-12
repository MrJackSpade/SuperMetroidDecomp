namespace Fixture;

/// <summary>A counter whose value is read through a property declared in its other part.</summary>
public sealed partial class Counter
{
    /// <summary>Creates a counter holding <paramref name="value"/>.</summary>
    public Counter(int value)
    {
        Count = value;
    }
}
