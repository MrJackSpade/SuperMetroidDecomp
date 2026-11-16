using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomGraphicsSetCodec"/> that only verification reads.</summary>
internal static class RoomGraphicsSetCodecConstants
{
    /// <summary>The number the cartridge gives <paramref name="graphicsSet"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="graphicsSet"/> is not a room graphics set.</exception>
    public static int Encode(RoomGraphicsSet graphicsSet)
    {
        int number = Array.IndexOf(RoomGraphicsSetCodec.Numbered, graphicsSet);
        return number >= 0 ? number : throw new ArgumentOutOfRangeException(nameof(graphicsSet), graphicsSet, "Not a room graphics set.");
    }
}
