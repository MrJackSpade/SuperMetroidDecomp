namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Exclusive library-three policies; ordinary handlers preserve the low-health mode.</summary>
internal enum SpcLibrary3Policy
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
