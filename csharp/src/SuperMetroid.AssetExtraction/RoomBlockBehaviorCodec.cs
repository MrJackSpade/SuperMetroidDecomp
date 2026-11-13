using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// The codec of the cartridge's block behaviour byte (#1551): the one place its layout is known. The byte stored beside
/// each level word means what the block's collision kind makes of it, so the import decodes every reading at once into a
/// <see cref="RoomBlockBehavior"/>:
/// <list type="bullet">
/// <item>a reaction: bit seven selects the area's reactions over the common ones, and the low seven bits are the entry;</item>
/// <item>a slope: the low five bits are the shape, bit six mirrors it left to right and bit seven top to bottom;</item>
/// <item>an extension: the whole byte, two's complement, is the signed offset;</item>
/// <item>a door: the low seven bits are the door-list entry.</item>
/// </list>
/// Only the import and checks that compare the game's own bytes convert through it: the installation holds each block's
/// behaviour as the reading its collision kind takes, and the X-ray reveals' rules by their reactions (#1709), so nothing
/// the game loads holds the byte.
/// </summary>
[EncodingOwner("room block behaviour byte")]
public static class RoomBlockBehaviorCodec
{
    /// <summary>The bit selecting the area's reactions, a slope's vertical mirroring, and an extension's sign.</summary>
    private const int HighBit = 0x80;

    /// <summary>The bit mirroring a slope left to right.</summary>
    private const int SlopeHorizontalBit = 0x40;

    /// <summary>The bits naming a reaction's entry or a door's link.</summary>
    private const int EntryMask = 0x7F;

    /// <summary>The bits naming a slope's shape.</summary>
    private const int SlopeShapeMask = 0x1F;

    /// <summary>Decodes a behaviour byte into every reading the block's collision kinds may take of it.</summary>
    /// <param name="value">The byte, 0 through 255.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a byte.</exception>
    public static RoomBlockBehavior Decode(int value)
    {
        Ensure.BetweenInclusive(0, byte.MaxValue, value);
        bool high = (value & HighBit) != 0;
        int entry = value & EntryMask;
        var reaction = new BlockReaction(high ? BlockReactionScope.AreaSpecific : BlockReactionScope.Common, entry);
        BlockMirroring mirroring = ((value & SlopeHorizontalBit) != 0 ? BlockMirroring.Horizontal : BlockMirroring.None) |
            (high ? BlockMirroring.Vertical : BlockMirroring.None);
        var slope = new SlopeGeometry(value & SlopeShapeMask, mirroring);
        return new RoomBlockBehavior(reaction, slope, unchecked((sbyte)value), entry);
    }

    /// <summary>Encodes a behaviour as its byte, from its reaction, which holds the whole byte.</summary>
    /// <param name="behavior">A decoded behaviour, or one a room object wrote.</param>
    public static int Encode(RoomBlockBehavior behavior) =>
        (behavior.Reaction.IsAreaSpecific ? HighBit : 0) | behavior.Reaction.Entry;
}
