namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One delayed music command emitted by the Shitroid encounter.</summary>
public readonly record struct ShitroidMusicRequest(MusicCommand Command, MusicCommandDelay Delay);
