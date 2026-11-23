namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>The priority at which colour-effect animations request their sounds.</summary>
public static partial class ColourEffectSoundPriorities
{
    /// <summary>The priority of every sound a colour-effect animation's sound cue requests.</summary>
    public static readonly SoundRequestPriority CueSound = SoundRequestPriority.Normal;
}
