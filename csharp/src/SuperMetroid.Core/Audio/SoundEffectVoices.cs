namespace SuperMetroid.Core.Audio;

/// <summary>
/// The set of the eight playback voices a rendered sound-effect layer sounds on. The sound driver
/// keys voices as a composable set (each effect channel adds its voice), so a layer, or several
/// layers merged by an override, carry the union.
/// </summary>
[Flags]
public enum SoundEffectVoices : byte
{
    /// <summary>No voice.</summary>
    None = 0,
}
