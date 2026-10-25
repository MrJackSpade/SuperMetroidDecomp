namespace SuperMetroid.Core.Fx;

/// <summary>The liquid options as the word Samus's liquid physics copies, which only verification reads.</summary>
internal static class RoomLiquidOptionsCodecConstants
{
    /// <summary>The options as the liquid-options word Samus's liquid physics copies.</summary>
    internal static ushort ToNativeWord(this RoomLiquidOptions options) => (ushort)options;
}
