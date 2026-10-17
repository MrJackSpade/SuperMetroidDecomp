namespace SuperMetroid.Core.Progression;

/// <summary>
/// The single conversion between <see cref="BossBits"/> and the per-area boss byte the save mirror, the SRAM slot and
/// native snapshots store. The conversion is lossless in both directions: bits whose meaning is not yet proven travel
/// through untouched, so a save written by the cartridge round-trips exactly. Nothing else turns boss state into a byte.
/// </summary>
[SnesEncodingOwner]
public static class BossBitsCodec
{
    /// <summary>The stored boss byte holding exactly <paramref name="bits"/>.</summary>
    public static byte ToNativeByte(this BossBits bits) => (byte)bits;

    /// <summary>The boss state a stored byte holds, including any bit whose meaning is not yet proven.</summary>
    public static BossBits FromNativeByte(byte value) => (BossBits)value;
}
