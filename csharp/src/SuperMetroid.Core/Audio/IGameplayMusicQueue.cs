namespace SuperMetroid.Core.Audio;

/// <summary>
/// The music queue a session's gameplay music requests go straight into as they are made (#2499), behind the game state's
/// own test of whether gameplay may queue music at all.
/// </summary>
internal interface IGameplayMusicQueue
{
    /// <summary>Puts <paramref name="request"/> on the music queue.</summary>
    void QueueMusic(GameplayMusicRequest request);
}
