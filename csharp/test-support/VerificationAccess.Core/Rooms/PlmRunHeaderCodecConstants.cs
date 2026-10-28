using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="PlmRunHeaderCodec"/> that only verification reads.</summary>
internal static class PlmRunHeaderCodecConstants
{
    /// <summary>The direction a stored header word, 0 through 65535, gives its run.</summary>
    internal static PlmRunDirection Direction(int word) =>
        (Ensure.BetweenInclusive(0, ushort.MaxValue, word) & PlmRunHeaderCodec.VerticalBit) != 0 ? PlmRunDirection.Vertical : PlmRunDirection.Horizontal;
    /// <summary>The block count a stored header word, 0 through 65535, gives its run.</summary>
    internal static int Count(int word) => Ensure.BetweenInclusive(0, ushort.MaxValue, word) & PlmRunHeaderCodec.CountBits;
}
