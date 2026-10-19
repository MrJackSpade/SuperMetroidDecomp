using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The malformed <c>$90:AD16</c> callback's copy when it falls back into LoadBeamPalette's loop at <c>$90:AD12</c>. The
/// cartridge's <c>STA $7EC1C0,X</c> loop overwrites palette buffers, the HUD tilemap, scroll zones, explored maps,
/// progression and level data. Owner decision on #1318: only the progression slice (<c>$7E:D820-D91B</c>) is kept,
/// exactly; the other regions are not replicated.
/// </summary>
internal readonly struct SpacetimeBeamCorruptionMemory(Bank80SystemState progression, SamusOverreadCatalog overreads)
{
    /// <summary>
    /// Copies one word from <c>[$00],Y</c> (Y = <paramref name="sourceIndex"/>) to <c>$7E:C1C0,X</c> (X =
    /// <paramref name="destinationIndex"/>) with the native 24-bit address wrap, writing only the bytes that land in the
    /// progression slice, straight into their owners. Those are reached only with a large inherited Y, so their sources
    /// are bank-$00 cartridge bytes, which the installed overread catalog holds.
    /// </summary>
    internal void CopyPaletteWord(ushort destinationIndex, ushort sourceIndex)
    {
        int sourceAddress = (SpacetimeBeamCorruptionLayout.SourceLongPointerValue + sourceIndex) & SnesCpuAddressLayout.AddressMask;
        int destinationAddress = (SpacetimeBeamCorruptionLayout.SpritePaletteSixAddress + destinationIndex) & SnesCpuAddressLayout.AddressMask;
        for (int offset = 0; offset < sizeof(ushort); offset++)
        {
            int destination = (destinationAddress + offset) & SnesCpuAddressLayout.AddressMask;
            int sliceOffset = destination - SpacetimeBeamCorruptionLayout.ProgressionSliceAddress;
            if (!Bank80SystemState.ProgressionSliceHolds(sliceOffset))
                continue;
            progression.WriteProgressionSliceByte(sliceOffset,
                overreads.SpacetimeCopySourceByte(
                    ((sourceAddress + offset) & SnesCpuAddressLayout.AddressMask) - SpacetimeBeamCopyDefinitions.FirstSourceAddress));
        }
    }
}
