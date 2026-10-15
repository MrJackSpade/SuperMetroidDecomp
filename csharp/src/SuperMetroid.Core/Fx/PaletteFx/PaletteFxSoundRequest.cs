namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One palette-FX call to a library-specific bank-$80 sound queue.</summary>
public readonly record struct PaletteFxSoundRequest(
    SoundEffectId SoundEffect,
    byte MaximumQueued,
    bool SoundSuppressed = false);
