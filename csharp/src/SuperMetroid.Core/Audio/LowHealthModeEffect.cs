namespace SuperMetroid.Core.Audio;

/// <summary>What starting a library-3 sound effect does to the low-health mode, which rejects every other library-3 request except the clearing one (#1305).</summary>
public enum LowHealthModeEffect
{
    /// <summary>The mode is unchanged.</summary>
    Unchanged,

    /// <summary>The mode is cleared.</summary>
    Clear,

    /// <summary>The mode is entered.</summary>
    Enter,
}
