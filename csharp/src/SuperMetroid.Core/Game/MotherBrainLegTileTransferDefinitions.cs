using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>Compiled fixed $A9:8F8F sprite-transfer list for phase-two leg loading.</summary>
public static class MotherBrainLegTileTransferDefinitions
{
    /// <summary>$A9:8F8F, first seven-byte record in the twelve-page list.</summary>
    public const ushort NativeListPointer = 0x8f8f;

    /// <summary>The full $A9:8F8F address used only by cartridge parity checks.</summary>
    public const int NativeListAddress = 0xa98f8f;

    /// <summary>Seven bytes per native size/source/destination record.</summary>
    public const ushort RecordByteCount = 7;

    /// <summary>Eight leg pages followed by four pages shared with attack restoration.</summary>
    public const int PageCount = 12;

    /// <summary>Returns one exact compiled native transfer record.</summary>
    public static MotherBrainSpriteTileTransferRequest Get(int index)
    {
        if ((uint)index >= PageCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        MotherBrainSpecialSpriteSheetDefinition legs =
            MotherBrainSpecialSpriteArtworkDefinitions.Legs;
        return new MotherBrainSpriteTileTransferRequest(
            EntryIndex: (ushort)index,
            Size: MotherBrainSpecialSpriteSheetDefinition.PageByteCount,
            SourceAddress: unchecked((uint)(legs.SourceAddress +
                index * MotherBrainSpecialSpriteSheetDefinition.PageByteCount)),
            VramDestination: unchecked((ushort)(legs.FirstDestinationWord +
                index * MotherBrainSpecialSpriteSheetDefinition.DestinationWordStride)));
    }

    /// <summary>Maps a serialized native record pointer to its page index.</summary>
    public static int IndexOf(ushort pointer)
    {
        int offset = pointer - NativeListPointer;
        if (offset < 0 || offset % RecordByteCount != 0 ||
            offset / RecordByteCount >= PageCount)
            throw new InvalidDataException(
                $"Mother Brain leg transfer pointer ${pointer:X4} is outside the twelve-record list.");
        return offset / RecordByteCount;
    }
}
