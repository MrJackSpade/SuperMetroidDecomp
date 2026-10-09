using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Game;

/// <summary>One palette-FX call to a library-specific bank-$80 sound queue.</summary>
/// <param name="SoundEffect">Effect identifier to submit to the sound queue.</param>
/// <param name="MaximumQueued">Queue occupancy limit supplied with the native sound request.</param>
/// <param name="SoundSuppressed">Whether the producer's sound-suppression condition prevents playback.</param>
public readonly record struct PaletteFxSoundRequest(
    SoundEffectId SoundEffect,
    byte MaximumQueued,
    bool SoundSuppressed = false);

/// <summary>One palette-FX call to the bank-$80 delayed music queue.</summary>
/// <param name="Command">Music command to submit to the delayed queue.</param>
/// <param name="Delay">Effective countdown before the queued command is applied, including the native minimum delay.</param>
public readonly record struct PaletteFxMusicRequest(
    MusicCommand Command,
    MusicCommandDelay Delay);
