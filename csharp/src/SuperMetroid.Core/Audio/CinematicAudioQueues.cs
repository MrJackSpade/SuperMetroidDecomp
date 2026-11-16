namespace SuperMetroid.Core.Audio;

/// <summary>
/// The frontend's queues as a cinematic's gameplay systems reach them (#2057): the intro's flashback Samus, shots and bombs
/// request their sounds through the same queue test every request passes, as the game's do while its cinematic runs them.
/// </summary>
/// <param name="queues">The frontend's sound and music queues.</param>
internal sealed class CinematicAudioQueues(AudioQueues queues) : IGameplayAudioQueues
{
    /// <summary>The frontend's queues.</summary>
    private readonly AudioQueues queues = Ensure.NotNull(queues);

    /// <inheritdoc/>
    public SoundQueueAdmission QueueSound(SoundRequest request) => queues.QueueSound(request);

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Always: the cinematic's music is its own, and its gameplay systems request none.</exception>
    public void QueueMusic(GameplayMusicRequest request) =>
        throw new InvalidOperationException($"A cinematic's Samus, shots and bombs request no music, but {request} was requested.");
}
