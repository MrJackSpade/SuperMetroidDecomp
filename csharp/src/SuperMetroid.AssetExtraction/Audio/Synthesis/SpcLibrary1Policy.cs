namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Exclusive library-one policy identities used by the extracted audio manifest.</summary>
internal enum SpcLibrary1Policy
{
    /// <summary>SPC $2AAB, nSound1Voices_1_sound1Priority_0.</summary>
    OneVoiceLowPriority = 0,
    /// <summary>SPC $2AB6, nSound1Voices_1_sound1Priority_1.</summary>
    OneVoiceHighPriority = 1,
    /// <summary>SPC $2AC1, nSound1Voices_2_sound1Priority_0.</summary>
    TwoVoicesLowPriority = 2,
    /// <summary>SPC $2ACC, nSound1Voices_3_sound1Priority_1.</summary>
    ThreeVoicesHighPriority = 3,
    /// <summary>SPC $2AD7, nSound1Voices_4_sound1Priority_0.</summary>
    FourVoicesLowPriority = 4,
    /// <summary>SPC $2AE2, duplicate four-voice low-priority handler selected by Power Bomb command $01.</summary>
    PowerBombFourVoices = 5,
}
