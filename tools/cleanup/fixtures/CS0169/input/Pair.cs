namespace Fixture;

/// <summary>A pair of values with a private field nothing uses.</summary>
public struct Pair
{
    /// <summary>The first value.</summary>
    public int First { get; set; }

    private int _padding;
}
