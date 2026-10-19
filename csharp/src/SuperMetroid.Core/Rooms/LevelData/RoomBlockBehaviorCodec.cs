namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The codec between <see cref="RoomBlockBehavior"/> and the BTS byte a room's behavior data stores. It is the one
/// place a typed behavior becomes that byte; the behavior holds 0 through 255, so the conversion is lossless.
/// </summary>
[SnesEncodingOwner]
internal static class RoomBlockBehaviorCodec
{
    /// <summary>The BTS byte that stores <paramref name="behavior"/>.</summary>
    internal static byte ToBtsByte(this RoomBlockBehavior behavior) => checked((byte)behavior.Value);
}
