namespace SuperMetroid.Core.Fx;

/// <summary>Cartridge values and helpers of <see cref="RoomLiquidOptionsCodec"/> that only verification reads.</summary>
internal static class RoomLiquidOptionsCodecConstants
{
    /// <summary>The options as the liquid-options word Samus's liquid physics copies.</summary>
    internal static ushort ToNativeWord(this RoomLiquidOptions options) => (ushort)options;
}
