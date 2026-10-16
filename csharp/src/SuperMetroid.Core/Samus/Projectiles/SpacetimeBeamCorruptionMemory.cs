using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The malformed <c>$90:AD16</c> callback's copy when it falls back into LoadBeamPalette's loop at <c>$90:AD12</c>. The
/// cartridge's <c>STA $7EC1C0,X</c> loop overwrites palette buffers, the HUD tilemap, scroll zones, explored maps,
/// progression and level data. Owner decision on #1318: only the progression slice (<c>$7E:D820-D91B</c>) is kept,
/// exactly; the other regions are not replicated.
/// </summary>
[CpuBusBoundary("domain view: SpaceTime Beam corrupted palette copy")]
internal readonly struct SpacetimeBeamCorruptionMemory(SuperMetroid.Core.Saves.SaveMirrorMemory progressionMirror)
{
    /// <summary>
    /// Copies one word from <c>[$00],Y</c> (Y = <paramref name="sourceIndex"/>) to <c>$7E:C1C0,X</c> (X =
    /// <paramref name="destinationIndex"/>) with the native 24-bit address wrap, writing only the bytes that land in the
    /// progression slice. Those are reached only with a large inherited Y, so their sources are compiled bank-$00
    /// cartridge bytes.
    /// </summary>
    /// <returns>True when the written word touched a translated persistent-progression or saved-entry byte.</returns>
    internal bool CopyPaletteWord(ushort destinationIndex, ushort sourceIndex)
    {
        int sourceAddress = (SpacetimeBeamCorruptionLayout.SourceLongPointerValue + sourceIndex) & SnesCpuAddressLayout.AddressMask;
        int destinationAddress = (SpacetimeBeamCorruptionLayout.SpritePaletteSix.Address + destinationIndex) & SnesCpuAddressLayout.AddressMask;
        bool progression = false;
        for (int offset = 0; offset < sizeof(ushort); offset++)
        {
            int destination = (destinationAddress + offset) & SnesCpuAddressLayout.AddressMask;
            if (!SaveMirrorMemory.Overlaps(destination, destination))
                continue;
            progressionMirror.WriteProgressionByte(destination,
                SpacetimeBeamCopyDefinitions.ReadCartridgeByte((sourceAddress + offset) & SnesCpuAddressLayout.AddressMask));
            progression = true;
        }
        return progression;
    }
}
