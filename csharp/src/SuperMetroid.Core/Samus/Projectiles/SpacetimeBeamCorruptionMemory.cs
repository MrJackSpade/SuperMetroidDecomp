using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The malformed <c>$90:AD16</c> callback's copy when it falls back into LoadBeamPalette's loop at <c>$90:AD12</c>. The
/// cartridge's <c>STA $7EC1C0,X</c> loop overwrites palette buffers, the HUD tilemap, scroll zones, explored maps,
/// progression and level data. Owner decision on #1318: only the progression slice (<c>$7E:D820-D91B</c>) is kept,
/// exactly; the other regions are not replicated.
/// </summary>
internal readonly struct SpacetimeBeamCorruptionMemory(SharedGameState progression, InstalledSamusOverreads overreads)
{
    /// <summary>
    /// Copies one word from <c>[$00],Y</c> (Y = <paramref name="sourceIndex"/>) to sprite palette six's byte X (X =
    /// <paramref name="destinationIndex"/>), writing only the bytes that land in the progression slice, straight into
    /// their owners. Sixteen-bit indices never reach the 24-bit wrap. Those bytes are reached only with a large
    /// inherited Y, so their sources are bank-$00 cartridge bytes, which the installed overread catalog holds.
    /// </summary>
    internal void CopyPaletteWord(int destinationIndex, int sourceIndex)
    {
        int source = SpacetimeBeamCopyDefinitions.SourceIndexBase + sourceIndex;
        for (int offset = 0; offset < sizeof(ushort); offset++)
        {
            int sliceOffset = destinationIndex + offset - SpacetimeBeamCopyDefinitions.ProgressionSliceDistance;
            if (!SharedGameState.ProgressionSliceHolds(sliceOffset))
                continue;
            progression.WriteProgressionSliceByte(sliceOffset, overreads.SpacetimeCopySourceAt(source + offset));
        }
    }
}
