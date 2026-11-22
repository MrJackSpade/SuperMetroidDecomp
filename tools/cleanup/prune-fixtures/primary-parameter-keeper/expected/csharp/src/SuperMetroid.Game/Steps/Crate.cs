namespace SuperMetroid.Game.Steps;

/// <summary>A crate, whose weight nothing reads.</summary>
/// <param name="label">The crate's label.</param>
internal sealed class Crate(string label)
{
    /// <summary>The crate's label.</summary>
    internal string Label { get; } = label;

    /// <summary>An empty crate, weighing nothing.</summary>
    internal Crate() : this("empty")
    {
    }
}
