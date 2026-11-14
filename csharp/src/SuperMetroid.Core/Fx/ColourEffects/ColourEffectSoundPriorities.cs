namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>The priority at which palette-effect animations request their sounds.</summary>
public static class ColourEffectSoundPriorities
{
    /// <summary>The priority of every sound a palette-effect animation's sound cue requests.</summary>
    public static readonly SoundRequestPriority CueSound = SoundRequestPriority.Normal;
}
