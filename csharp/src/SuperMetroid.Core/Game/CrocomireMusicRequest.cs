namespace SuperMetroid.Core.Game;

/// <summary>One delayed music request emitted by Crocomire's death graph.</summary>
public readonly record struct CrocomireMusicRequest(MusicCommand Command, MusicCommandDelay Delay);
