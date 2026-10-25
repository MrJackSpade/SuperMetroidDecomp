namespace SuperMetroid.Core.Progression;

/// <summary>
/// The single conversion between <see cref="BossBits"/> and the per-area boss byte a save slot stores. The conversion is
/// lossless in both directions: bits whose meaning is not yet proven travel through untouched, so a save written by the
/// cartridge round-trips exactly. Nothing else turns boss state into a byte.
/// </summary>
[EncodingOwner("save slot boss byte")]
public static class BossBitsCodec
{
    /// <summary>The saved boss byte holding exactly <paramref name="bits"/>.</summary>
    public static byte ToSaveByte(this BossBits bits) => (byte)bits;

    /// <summary>The boss state a saved byte holds, including any bit whose meaning is not yet proven.</summary>
    public static BossBits FromSaveByte(byte value) => (BossBits)value;
}
