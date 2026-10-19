namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One palette-FX call to a library-specific bank-$80 sound queue.</summary>
public readonly record struct PaletteFxSoundRequest(
    SoundEffectId SoundEffect,
    int MaximumQueued,
    bool SoundSuppressed = false);
