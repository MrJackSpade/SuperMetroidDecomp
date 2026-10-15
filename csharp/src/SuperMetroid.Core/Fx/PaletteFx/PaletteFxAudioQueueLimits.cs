namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Native queue-admission limits selected by palette-FX audio instructions.</summary>
public static class PaletteFxAudioQueueLimits
{
    /// <summary>
    /// All three palette-FX SFX opcodes call the corresponding bank-$80
    /// <c>QueueSfxN_Max6</c> routine.
    /// </summary>
    public const byte SoundEffects = 6;
}
