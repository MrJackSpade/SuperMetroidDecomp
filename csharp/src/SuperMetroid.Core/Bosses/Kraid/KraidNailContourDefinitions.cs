namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The four ledges of Kraid's fingernail contour: where his body pushes back a nail level with each.</summary>
/// <remarks>
/// A nail above all four ledges is pushed back at the record the original game reads after them, which the installed
/// <see cref="KraidNailPastLedges"/> holds (#1500).
/// </remarks>
internal static class KraidNailContourDefinitions
{
    /// <summary>The contour's ledges.</summary>
    internal const int LedgeCount = 4;

    /// <summary>
    /// Each ledge's left offset. Independently reviewed for #1165 against pinned bank_A7.asm and original NTSC words:
    /// the four ledges advance 24 pixels from -64 to 8.
    /// </summary>
    internal static readonly short[] Lefts = [-64, -40, -16, 8];

    /// <summary>
    /// The comparison boundary paired with each left offset. The first three ledges are 56 pixels apart, starting at 16;
    /// the final authored head boundary is -128.
    /// </summary>
    internal static readonly short[] Tops = [16, -40, -96, -128];
}
