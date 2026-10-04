namespace SuperMetroid.Core.Audio;

/// <summary>Exclusive library-one policy identities used by the extracted audio manifest.</summary>
internal enum SpcLibrary1Policy : byte
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

/// <summary>Exclusive library-two policy identities used by the extracted audio manifest.</summary>
internal enum SpcLibrary2Policy : byte
{
    /// <summary>SPC $3987, nSound2Voices_1_sound2Priority_0.</summary>
    OneVoiceLowPriority = 0,
    /// <summary>SPC $3992, nSound2Voices_1_sound2Priority_1.</summary>
    OneVoiceHighPriority = 1,
    /// <summary>SPC $399D, nSound2Voices_2_sound2Priority_0.</summary>
    TwoVoicesLowPriority = 2,
    /// <summary>SPC $39A8, nSound2Voices_2_sound2Priority_1.</summary>
    TwoVoicesHighPriority = 3,
}

/// <summary>Exclusive library-three policies; ordinary handlers preserve the low-health mode.</summary>
internal enum SpcLibrary3Policy : byte
{
    /// <summary>SPC $4DD0, sound3Configurations_sound1: one voice, mode zero, priority one.</summary>
    CancelAndClearLowHealthMode = 0,
    /// <summary>SPC $4DE0, sound3Configurations_sound2: one voice, mode two, priority unchanged.</summary>
    LowHealthModePreservePriority = 1,
    /// <summary>SPC $4E63, nSound3Voices_1_sound3Priority_0.</summary>
    OneVoiceLowPriority = 2,
    /// <summary>SPC $4E6E, nSound3Voices_2_sound3Priority_0.</summary>
    TwoVoicesLowPriority = 3,
    /// <summary>SPC $4E79, nSound3Voices_2_sound3Priority_1.</summary>
    TwoVoicesHighPriority = 4,
    /// <summary>SPC $4E84, nSound3Voices_1_sound3Priority_1.</summary>
    OneVoiceHighPriority = 5,
}
