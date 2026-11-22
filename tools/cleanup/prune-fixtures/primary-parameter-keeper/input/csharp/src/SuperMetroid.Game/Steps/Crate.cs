namespace SuperMetroid.Game.Steps;

/// <summary>A crate, whose weight nothing reads.</summary>
/// <param name="label">The crate's label.</param>
/// <param name="weight">The crate's weight.</param>
internal sealed class Crate(string label, int weight)
{
    /// <summary>The crate's label.</summary>
    internal string Label { get; } = label;

    /// <summary>The crate's weight, which nothing reads.</summary>
    internal int Weight { get; } = weight;

    /// <summary>An empty crate, weighing nothing.</summary>
    internal Crate() : this("empty", 0)
    {
    }
}
