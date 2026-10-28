namespace SuperMetroid.Core.Audio;

/// <summary>
/// The set of the eight playback voices a rendered sound-effect layer sounds on. The sound driver
/// keys voices as a composable set (each effect channel adds its voice), so a layer, or several
/// layers merged by an override, carry the union.
/// </summary>
[Flags]
public enum SoundEffectVoices
{
    /// <summary>No voice.</summary>
    None = 0,

    /// <summary>The first voice.</summary>
    Voice0 = 1,

    /// <summary>The second voice.</summary>
    Voice1 = 2,

    /// <summary>The third voice.</summary>
    Voice2 = 4,

    /// <summary>The fourth voice.</summary>
    Voice3 = 8,

    /// <summary>The fifth voice.</summary>
    Voice4 = 16,

    /// <summary>The sixth voice.</summary>
    Voice5 = 32,

    /// <summary>The seventh voice.</summary>
    Voice6 = 64,

    /// <summary>The eighth voice.</summary>
    Voice7 = 128,
}
