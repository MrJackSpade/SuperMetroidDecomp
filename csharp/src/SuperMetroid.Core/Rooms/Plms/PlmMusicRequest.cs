namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One delayed music request made by a room PLM instruction.</summary>
public readonly record struct PlmMusicRequest(
    MusicCommand Command,
    MusicCommandDelay Delay);
