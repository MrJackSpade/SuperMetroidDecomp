namespace SuperMetroid.Core.Game;

/// <summary>Music request produced by Bomb Torizo's ROM instruction stream.</summary>
/// <param name="Command">Track or music-data command to pass to the gameplay music queue.</param>
/// <param name="Delay">Authored queue delay, expressed by the music command's gameplay-frame delay identifier.</param>
public readonly record struct BombTorizoMusicRequest(MusicCommand Command, MusicCommandDelay Delay);
