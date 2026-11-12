namespace Fixture;

/// <summary>A sample with a private field nothing uses.</summary>
public sealed class Sample
{
    private int _unused;

    /// <summary>The sample's value.</summary>
    public int Value { get; init; }
}
