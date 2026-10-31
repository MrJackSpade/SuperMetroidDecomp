using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>
/// The SpaceTime palette copy as the original callback runs it, over the cartridge, for checks (#1318, #1474): the
/// interrupted long store at $90:AD14 copies a word at a time from the read index the shot's animation leaves behind,
/// through bank $00's mirror of bank $80, to sprite palette six plus the destination index, while the signed read index
/// stays below the palette's 32 bytes. The game holds only each situation's decoded progression overwrite.
/// </summary>
internal static class SpacetimeCopyOracle
{
    /// <summary>Bytes from sprite palette six to the event bitset that opens the persistent-progress slice.</summary>
    private const int ProgressionSliceDistance = 0x1660;

    /// <summary>The signed bound $90:AD1C compares the advanced read index with.</summary>
    private const int PaletteByteCount = 0x20;

    /// <summary>Bytes from the event bitset at $7E:D820 through the saved loading state at $7E:D914-$D915.</summary>
    internal const int ProgressionSliceByteCount = 0xf6;

    /// <summary>Bytes the animation's next record lies past the record the copy inherits its read index from.</summary>
    private const int TimedRecordByteCount = 8;

    /// <summary>
    /// The bytes the copy writes into the persistent-progress slice, by offset from the event bitset, for a shot in slot
    /// <paramref name="slot"/> whose animation stands at list position <paramref name="listPosition"/>; null where it writes
    /// none. A byte read from below the cartridge half of bank $00 or past its end would be scratch memory and is rejected.
    /// </summary>
    internal static byte?[] ProgressionSlice(IImportCartridgeSource cartridge, int listPosition, int slot)
    {
        var slice = new byte?[ProgressionSliceByteCount];
        int x = (slot + 1) * sizeof(ushort);
        int y = unchecked((ushort)(listPosition - TimedRecordByteCount + sizeof(ushort)));
        while (unchecked((short)(y - PaletteByteCount)) < 0)
        {
            for (int offset = 0; offset < sizeof(ushort); offset++)
            {
                int sliceOffset = x + offset - ProgressionSliceDistance;
                if ((uint)sliceOffset >= ProgressionSliceByteCount)
                    continue;
                int source = y + offset;
                if (source < 0x8000 || source > ushort.MaxValue)
                    throw new InvalidDataException($"The SpaceTime copy writes progression byte {sliceOffset} from scratch memory at $00:{source:X4}.");
                slice[sliceOffset] = cartridge.ReadCartridgeByte(0x800000 | source);
            }
            x = unchecked((ushort)(x + sizeof(ushort)));
            y = unchecked((ushort)(y + sizeof(ushort)));
        }
        return slice;
    }
}
