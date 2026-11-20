namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Exclusive library-two policy identities used by the extracted audio manifest.</summary>
internal enum SpcLibrary2Policy
{
    /// <summary>SPC $3987, nSound2Voices_1_sound2Priority_0.</summary>
    OneVoiceLowPriority,
    /// <summary>SPC $3992, nSound2Voices_1_sound2Priority_1.</summary>
    OneVoiceHighPriority,
    /// <summary>SPC $399D, nSound2Voices_2_sound2Priority_0.</summary>
    TwoVoicesLowPriority,
    /// <summary>SPC $39A8, nSound2Voices_2_sound2Priority_1.</summary>
    TwoVoicesHighPriority,
}
