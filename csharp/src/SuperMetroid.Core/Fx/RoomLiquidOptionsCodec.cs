namespace SuperMetroid.Core.Fx;

/// <summary>
/// The codec between <see cref="RoomLiquidOptions"/> and the liquid-options byte a room-FX record stores and Samus's
/// liquid physics copies as a word. It is the one place the options become or leave that encoding; the conversion is
/// lossless, so options no liquid tests survive the round trip.
/// </summary>
[SnesEncodingOwner]
internal static class RoomLiquidOptionsCodec
{
    /// <summary>The options a room-FX record's liquid-options byte selects.</summary>
    internal static RoomLiquidOptions FromRecordByte(byte value) => (RoomLiquidOptions)value;

    /// <summary>The options as the liquid-options word Samus's liquid physics copies.</summary>
    internal static ushort ToNativeWord(this RoomLiquidOptions options) => (ushort)options;
}
