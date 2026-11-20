namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The four ledges of Kraid's fingernail contour: where his body pushes back a nail level with each.</summary>
/// <remarks>
/// A nail above all four ledges is pushed back where <see cref="KraidNailPastLedges"/> places it (#1500).
/// </remarks>
internal static class KraidNailContourDefinitions
{
    /// <summary>
    /// The ledges, from his feet up (#2400). Their left offsets advance 24 pixels from -64 to 8; the first three ledges'
    /// bounds are 56 pixels apart, starting at 16, and the last, his head's, is -128.
    /// </summary>
    internal static IReadOnlyList<KraidNailLedge> Ledges { get; } =
    [
        new(Top: 16, Left: -64),
        new(Top: -40, Left: -40),
        new(Top: -96, Left: -16),
        new(Top: -128, Left: 8),
    ];
}
