using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectRunHeaderCodec"/> that only verification reads.</summary>
internal static class RoomObjectRunHeaderCodecConstants
{
    /// <summary>The direction a stored header word, 0 through 65535, gives its run.</summary>
    internal static RoomObjectRunDirection Direction(int word) =>
        (Ensure.BetweenInclusive(0, ushort.MaxValue, word) & RoomObjectRunHeaderCodec.VerticalBit) != 0 ? RoomObjectRunDirection.Vertical : RoomObjectRunDirection.Horizontal;
    /// <summary>The block count a stored header word, 0 through 65535, gives its run.</summary>
    internal static int Count(int word) => Ensure.BetweenInclusive(0, ushort.MaxValue, word) & RoomObjectRunHeaderCodec.CountBits;
}
