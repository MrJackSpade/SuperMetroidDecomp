namespace SuperMetroid.Core.Game;

/// <summary>One delayed music command emitted by the Shitroid encounter.</summary>
public readonly record struct ShitroidMusicRequest(MusicCommand Command, MusicCommandDelay Delay);
