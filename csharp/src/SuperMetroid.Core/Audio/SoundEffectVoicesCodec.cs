namespace SuperMetroid.Core.Audio;

/// <summary>
/// The single conversion between <see cref="SoundEffectVoices"/> and the export renderer's voice
/// mask, one bit per voice with the first voice in the lowest bit. The clip exporter decodes the
/// voices it observed through <see cref="FromVoiceMask"/>, and an offline re-render selects them
/// through <see cref="ToVoiceMask"/>; nothing else turns the set into a mask.
/// </summary>
[SnesEncodingOwner]
public static class SoundEffectVoicesCodec
{
    /// <summary>The voice set a renderer voice mask names; every bit of the mask is a voice.</summary>
    public static SoundEffectVoices FromVoiceMask(byte mask) => (SoundEffectVoices)mask;

    /// <summary>The renderer voice mask holding exactly <paramref name="voices"/>.</summary>
    public static byte ToVoiceMask(this SoundEffectVoices voices) => (byte)voices;
}
