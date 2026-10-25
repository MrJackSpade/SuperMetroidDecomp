namespace SuperMetroid.Core.Audio;

/// <summary>
/// The single conversion between <see cref="SoundEffectVoices"/> and the export renderer's voice
/// mask, one bit per voice with the first voice in the lowest bit. The clip exporter decodes the
/// voices it observed through <see cref="FromVoiceMask"/>, and an offline re-render selects them
/// through <c>ToVoiceMask</c>; nothing else turns the set into a mask.
/// </summary>
[EncodingOwner("clip exporter voice mask")]
public static class SoundEffectVoicesCodec
{
    /// <summary>The voices in mask order: the first voice is the lowest bit.</summary>
    private static readonly SoundEffectVoices[] VoicesInMaskOrder =
    [
        SoundEffectVoices.Voice0, SoundEffectVoices.Voice1, SoundEffectVoices.Voice2, SoundEffectVoices.Voice3,
        SoundEffectVoices.Voice4, SoundEffectVoices.Voice5, SoundEffectVoices.Voice6, SoundEffectVoices.Voice7,
    ];

    /// <summary>The voice set a renderer voice mask names; every bit of the mask is a voice.</summary>
    public static SoundEffectVoices FromVoiceMask(int mask)
    {
        Ensure.BetweenInclusive(0, byte.MaxValue, mask);
        SoundEffectVoices voices = SoundEffectVoices.None;
        for (int voice = 0; voice < VoicesInMaskOrder.Length; voice++)
        {
            if ((mask & (1 << voice)) != 0)
                voices |= VoicesInMaskOrder[voice];
        }
        return voices;
    }
}
