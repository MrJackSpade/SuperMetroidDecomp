using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Escape;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Compiled fixed $A9:8F8F sprite-transfer list for phase-two leg loading.</summary>
public static class MotherBrainLegTileTransferDefinitions
{

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
}
