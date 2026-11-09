namespace SuperMetroid.Core.Audio;

/// <summary>What starting a room-library sound effect does to the low-health mode, which rejects every other room-library request except the clearing one (#1305).</summary>
public enum LowHealthModeEffect
{
    /// <summary>The mode is unchanged.</summary>
    Unchanged,

    /// <summary>The mode is cleared.</summary>
    Clear,

    /// <summary>The mode is entered.</summary>
    Enter,
}
