namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The codec between <see cref="SnesMainScreenLayers"/> and the TM/TS register byte the window/screen register cache
/// still holds. That cache keeps literal register bytes because a misaligned projectile word store can overwrite
/// them; this type is the one place the typed layer designation becomes that byte. The destination is the display
/// snapshot holding the typed designation directly, which removes this codec.
/// </summary>
[SnesEncodingOwner]
internal static class SnesMainScreenLayersCodec
{
    /// <summary>The layer designation as the TM/TS register byte that stores it.</summary>
    internal static byte ToNativeByte(this SnesMainScreenLayers layers) => (byte)layers;
}
