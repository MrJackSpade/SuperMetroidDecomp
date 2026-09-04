using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Game;

/// <summary>One palette-FX call to a library-specific bank-$80 sound queue.</summary>
public readonly record struct PaletteFxSoundRequest(
    SoundEffectId SoundEffect,
    byte MaximumQueued);

/// <summary>One palette-FX call to the bank-$80 delayed music queue.</summary>
public readonly record struct PaletteFxMusicRequest(
    MusicCommand Command,
    MusicCommandDelay Delay);
