namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The priority at which palette-effect programs request their sounds.</summary>
public static class PaletteFxSoundPriorities
{
    /// <summary>The priority of every sound a palette-effect program's sound cue requests.</summary>
    public static readonly SoundRequestPriority CueSound = SoundRequestPriority.Normal;
}
