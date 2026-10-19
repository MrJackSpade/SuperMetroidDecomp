namespace SuperMetroid.Core.Audio;

/// <summary>Cartridge values and helpers of <see cref="SoundEffectVoicesCodec"/> that only verification reads.</summary>
internal static class SoundEffectVoicesCodecConstants
{
    /// <summary>The renderer voice mask holding exactly <paramref name="voices"/>.</summary>
    public static byte ToVoiceMask(this SoundEffectVoices voices) => (byte)voices;
}
