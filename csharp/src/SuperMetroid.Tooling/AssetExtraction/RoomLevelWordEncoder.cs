using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Encodes typed terrain back into the cartridge's room level word (#1469), for development tools and checks that write
/// native seeds or compare against native traces. The game never encodes; the import only decodes, so the encoding lives
/// with the tools, reading the bit layout <see cref="RoomLevelWordCodec"/> owns.
/// </summary>
public static class RoomLevelWordEncoder
{
    /// <summary>Encodes a terrain block as the level word the cartridge stores for it.</summary>
    /// <param name="block">The terrain block.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block's collision kind is undefined.</exception>
    public static ushort Encode(RoomTerrainBlock block)
    {
        int nibble = RoomLevelWordCodec.KindsByNibble.IndexOf(block.Collision);
        if (nibble < 0)
            throw new ArgumentOutOfRangeException(nameof(block), block.Collision, "Undefined collision kind.");
        return checked((ushort)(EncodeAppearance(block.Appearance) | (nibble << RoomLevelWordCodec.CollisionShift)));
    }

    /// <summary>Encodes an appearance as an appearance-only level word: no collision bits.</summary>
    /// <param name="appearance">The appearance.</param>
    public static ushort EncodeAppearance(RoomBlockAppearance appearance) =>
        checked((ushort)(appearance.Block |
            ((appearance.Mirroring & BlockMirroring.Horizontal) != 0 ? RoomLevelWordCodec.HorizontalBit : 0) |
            ((appearance.Mirroring & BlockMirroring.Vertical) != 0 ? RoomLevelWordCodec.VerticalBit : 0)));
}
