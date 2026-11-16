namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One of the three health colourings Ridley's own colours show as he weakens (#2242). Norfair Ridley's hurt health
/// bands each show one, and Ceres Ridley shows two of the same installed colourings as his accepted hits mount, so
/// editing one changes both encounters.
/// </summary>
public sealed class RidleyHealthShade
{
    /// <summary>Defines a colouring.</summary>
    /// <param name="key">The colouring's name in its shade's name.</param>
    internal RidleyHealthShade(string key) => Key = key;

    /// <summary>The colouring's name in its shade's name.</summary>
    public string Key { get; }

    /// <inheritdoc/>
    public override string ToString() => Key;
}
