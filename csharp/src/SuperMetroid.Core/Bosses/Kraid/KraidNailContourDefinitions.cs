namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Calculated geometry and bounded native overreads for Kraid's fingernail contour.</summary>
/// <remarks>
/// The contour has four ledges; a nail below all of them reads two more records from the code that follows the table
/// ($A7:BF2D..BF34). Those records are gameplay: they decide where a low nail is pushed. Their values are stated here
/// as the offsets and bounds the contour reads (#1413); the ROM oracle confirms they are the cartridge's words.
/// </remarks>
internal static class KraidNailContourDefinitions
{
    /// <summary>The contour's records: four ledges followed by the two the contour reads past them.</summary>
    internal const int RecordCount = 6;

    /// <summary>
    /// Each record's left offset. Independently reviewed for #1165 against pinned bank_A7.asm and original NTSC words:
    /// the four geometry ledges advance 24 pixels from -64 to 8; the next two are the overreads past the ledges, whose
    /// unsigned words are preserved.
    /// </summary>
    internal static readonly short[] Lefts = [-64, -40, -16, 8, 1312, 4050];

    /// <summary>
    /// The comparison boundary paired with each left offset. The first three ledges are 56 pixels apart, starting at 16;
    /// the final authored head boundary is -128. The two overread bounds follow; they are bounded overreads, not
    /// extrapolated contour geometry.
    /// </summary>
    internal static readonly short[] Tops = [16, -40, -96, -128, -21056, 14281];
}
