using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// The codec of the cartridge's room level word (#1469): the one place its bit layout is known. A level word packs a
/// block's number in its tileset's block set (bits 0 through 9), its horizontal and vertical mirroring (bits 10 and 11)
/// and its collision kind (the high nibble, the index of the kind's handler in the cartridge's collision jump tables).
/// The import decodes every level word once, here; the game holds <see cref="RoomTerrainBlock"/>s.
/// </summary>
[EncodingOwner("room level data word")]
public static class RoomLevelWordCodec
{
    /// <summary>The bits naming the block shown.</summary>
    private const int BlockMask = (1 << 10) - 1;

    /// <summary>The bit mirroring the block left to right.</summary>
    private const int HorizontalBit = 1 << 10;

    /// <summary>The bit mirroring the block top to bottom.</summary>
    private const int VerticalBit = 1 << 11;

    /// <summary>The bits of an appearance: the block and its mirroring.</summary>
    private const int AppearanceMask = BlockMask | HorizontalBit | VerticalBit;

    /// <summary>Distance from the low bit to the collision nibble.</summary>
    private const int CollisionShift = 12;

    /// <summary>Each collision kind by the nibble the cartridge stores for it.</summary>
    private static readonly RoomCollisionType[] KindsByNibble =
    [
        RoomCollisionType.Air, RoomCollisionType.Slope, RoomCollisionType.SpikeAir, RoomCollisionType.SpecialAir,
        RoomCollisionType.ShootableAir, RoomCollisionType.HorizontalExtension, RoomCollisionType.UnusedAir,
        RoomCollisionType.BombableAir, RoomCollisionType.SolidBlock, RoomCollisionType.DoorBlock,
        RoomCollisionType.SpikeBlock, RoomCollisionType.SpecialBlock, RoomCollisionType.ShootableBlock,
        RoomCollisionType.VerticalExtension, RoomCollisionType.GrappleBlock, RoomCollisionType.BombableBlock,
    ];

    /// <summary>Decodes a level word into the terrain block it stores.</summary>
    /// <param name="word">A cartridge level word.</param>
    public static RoomTerrainBlock Decode(ushort word) =>
        new(DecodeAppearance(word), KindsByNibble[word >> CollisionShift]);

    /// <summary>Encodes a terrain block as the level word the cartridge stores for it.</summary>
    /// <param name="block">The terrain block.</param>
    public static ushort Encode(RoomTerrainBlock block)
    {
        int nibble = Array.IndexOf(KindsByNibble, block.Collision);
        if (nibble < 0)
            throw new ArgumentOutOfRangeException(nameof(block), block.Collision, "Undefined collision kind.");
        return checked((ushort)(EncodeAppearance(block.Appearance) | (nibble << CollisionShift)));
    }

    /// <summary>Decodes the appearance half of a level word, ignoring its collision nibble.</summary>
    /// <param name="word">A cartridge level word, or an appearance-only word.</param>
    public static RoomBlockAppearance DecodeAppearance(ushort word) =>
        new(word & BlockMask,
            ((word & HorizontalBit) != 0 ? BlockMirroring.Horizontal : BlockMirroring.None) |
            ((word & VerticalBit) != 0 ? BlockMirroring.Vertical : BlockMirroring.None));

    /// <summary>Encodes an appearance as an appearance-only level word: no collision bits.</summary>
    /// <param name="appearance">The appearance.</param>
    public static ushort EncodeAppearance(RoomBlockAppearance appearance) =>
        checked((ushort)(appearance.Block |
            ((appearance.Mirroring & BlockMirroring.Horizontal) != 0 ? HorizontalBit : 0) |
            ((appearance.Mirroring & BlockMirroring.Vertical) != 0 ? VerticalBit : 0)));

    /// <summary>Whether a word holds only an appearance: no collision bits.</summary>
    /// <param name="word">The word, 0 through 65535 when valid.</param>
    public static bool IsAppearanceWord(int word) => (word & ~AppearanceMask) == 0;
}
