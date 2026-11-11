namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's death mosaic, as its dying fade-out grows it: one block size every sixteenth frame,
/// from the smallest to the largest, before the fade-out runs.
/// </summary>
internal static class PhantoonDeathDefinitions
{
    /// <summary>The death mosaic's first block size in screen pixels: no quantization.</summary>
    internal const int InitialMosaicSize = 1;

    /// <summary>The death mosaic's largest block size in screen pixels, after which it stops growing.</summary>
    internal const int FinalMosaicSize = 16;
}
