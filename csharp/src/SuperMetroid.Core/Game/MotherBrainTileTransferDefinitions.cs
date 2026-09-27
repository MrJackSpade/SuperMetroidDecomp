using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>Immutable cartridge transfer metadata used during the Mother Brain encounter.</summary>
public static class MotherBrainTileTransferDefinitions
{
    /// <summary>$A9:8FE5, four size/source/destination records loading Baby graphics +$400 onward.</summary>
    public const int BabyTileList = 0xa98fe5;

    /// <summary>Four nonzero records precede the Baby transfer list terminator.</summary>
    public const int BabyTileCount = 4;

    /// <summary>Each native record contains a word size, long source, and word VRAM destination.</summary>
    public const int RecordSize = 7;

    /// <summary>
    /// Compiled $A9:8FE5 Baby Metroid transfer record. The native list consists of four
    /// consecutive $200-byte source pages and consecutive $100-word OBJ destinations.
    /// </summary>
    public static MotherBrainSpriteTileTransferRequest BabyTileTransfer(int index)
    {
        MotherBrainSpecialSpriteSheetDefinition sheet =
            MotherBrainSpecialSpriteArtworkDefinitions.BabyMetroid;
        if ((uint)index >= BabyTileCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return new MotherBrainSpriteTileTransferRequest(
            EntryIndex: (ushort)index,
            Size: MotherBrainSpecialSpriteSheetDefinition.PageByteCount,
            SourceAddress: unchecked((uint)(sheet.SourceAddress +
                index * MotherBrainSpecialSpriteSheetDefinition.PageByteCount)),
            VramDestination: unchecked((ushort)(sheet.FirstDestinationWord +
                index * MotherBrainSpecialSpriteSheetDefinition.DestinationWordStride)));
    }
}
