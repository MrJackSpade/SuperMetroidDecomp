namespace SuperMetroid.Core.Game;

/// <summary>One palette-FX call to the bank-$80 delayed music queue.</summary>
public readonly record struct PaletteFxMusicRequest(
    MusicCommand Command,
    MusicCommandDelay Delay);
