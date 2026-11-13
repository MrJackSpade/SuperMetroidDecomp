using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// The codec of the cartridge's room level word (#1469): the one place its bit layout is known. A level word packs a
/// block's number in its tileset's block set (bits 0 through 9), its horizontal and vertical mirroring (bits 10 and 11)
/// and its collision kind (the high nibble, the index of the kind's handler in the cartridge's collision jump tables).
/// The import decodes every level word once, here; the game holds <see cref="RoomTerrainBlock"/>s. Tools that write
/// words back use the same layout through Tooling's encoder. The codec also names the two halves each word is stored
/// as, which a Wave shot's read landing halfway through a block takes from two neighbouring words; the import decodes
/// what every such read finds into the installed straddled terrain (#1612).
/// </summary>
[EncodingOwner("room level data word")]
public static class RoomLevelWordCodec
{
    /// <summary>The bits naming the block shown.</summary>
    private const int BlockMask = (1 << 10) - 1;

    /// <summary>The bit mirroring the block left to right.</summary>
    internal const int HorizontalBit = 1 << 10;

    /// <summary>The bit mirroring the block top to bottom.</summary>
    internal const int VerticalBit = 1 << 11;

    /// <summary>Distance from the low bit to the collision nibble.</summary>
    internal const int CollisionShift = 12;

    /// <summary>Each collision kind by the nibble the cartridge stores for it.</summary>
    internal static ImmutableArray<RoomCollisionType> KindsByNibble { get; } =
    [
        RoomCollisionType.Air, RoomCollisionType.Slope, RoomCollisionType.SpikeAir, RoomCollisionType.SpecialAir,
        RoomCollisionType.ShootableAir, RoomCollisionType.HorizontalExtension, RoomCollisionType.UnrevealedAir,
        RoomCollisionType.BombableAir, RoomCollisionType.SolidBlock, RoomCollisionType.DoorBlock,
        RoomCollisionType.SpikeBlock, RoomCollisionType.SpecialBlock, RoomCollisionType.ShootableBlock,
        RoomCollisionType.VerticalExtension, RoomCollisionType.GrappleBlock, RoomCollisionType.BombableBlock,
    ];

    /// <summary>Decodes a level word into the terrain block it stores.</summary>
    /// <param name="word">A cartridge level word.</param>
    public static RoomTerrainBlock Decode(ushort word) =>
        new(DecodeAppearance(word), KindsByNibble[word >> CollisionShift]);

    /// <summary>The bits in each stored half of a level word.</summary>
    private const int HalfBits = 8;

    /// <summary>How many values each stored half of a level word holds.</summary>
    internal const int HalfValues = 1 << HalfBits;

    /// <summary>
    /// The half of <paramref name="word"/> the level data stores first, its low eight bits: the start of the block, which a
    /// read landing halfway through the block before it ends in (#1612).
    /// </summary>
    /// <param name="word">A cartridge level word.</param>
    internal static int FirstStoredHalf(ushort word) => word & (HalfValues - 1);

    /// <summary>
    /// The half of <paramref name="word"/> the level data stores second, its high eight bits: the end of the block, which a
    /// read landing halfway through the block starts in (#1612).
    /// </summary>
    /// <param name="word">A cartridge level word.</param>
    internal static int SecondStoredHalf(ushort word) => word >> HalfBits;

    /// <summary>
    /// The level word read from <paramref name="firstHalf"/> stored first and <paramref name="secondHalf"/> stored second:
    /// what a read landing halfway through a block finds, from the block's second stored half and the next block's first
    /// (#1612).
    /// </summary>
    /// <param name="firstHalf">The half stored first, 0 through 255.</param>
    /// <param name="secondHalf">The half stored second, 0 through 255.</param>
    internal static ushort OfStoredHalves(int firstHalf, int secondHalf) =>
        checked((ushort)(Ensure.BetweenInclusive(0, HalfValues - 1, firstHalf) |
            (Ensure.BetweenInclusive(0, HalfValues - 1, secondHalf) << HalfBits)));

    /// <summary>Decodes the appearance half of a level word, ignoring its collision nibble.</summary>
    /// <param name="word">A cartridge level word, or an appearance-only word.</param>
    public static RoomBlockAppearance DecodeAppearance(ushort word) =>
        new(word & BlockMask,
            ((word & HorizontalBit) != 0 ? BlockMirroring.Horizontal : BlockMirroring.None) |
            ((word & VerticalBit) != 0 ? BlockMirroring.Vertical : BlockMirroring.None));
}
