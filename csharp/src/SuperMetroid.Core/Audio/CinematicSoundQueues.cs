namespace SuperMetroid.Core.Audio;

/// <summary>
/// The frontend's sound queues as a cinematic's gameplay systems reach them (#2057, #2499): the intro's flashback Samus,
/// shots and bombs request their sounds through the same queue test every request passes, as the game's do while its
/// cinematic runs them. They request no music, so they reach no music queue; the cinematic's music is its own.
/// </summary>
/// <param name="queues">The frontend's sound and music queues.</param>
internal sealed partial class CinematicSoundQueues(AudioQueues queues) : IGameplaySoundQueue
{
    /// <summary>The frontend's queues.</summary>
    private readonly AudioQueues queues = Ensure.NotNull(queues);

    /// <inheritdoc/>
    public SoundQueueAdmission QueueSound(SoundRequest request) => queues.QueueSound(request);
}
