namespace SuperMetroid.Core.Audio;

/// <summary>
/// A check's record of the sound and music requests gameplay makes (#2137): each request is recorded, in request order,
/// and goes straight on into the queues the record wraps, which treat it exactly as they would without the record.
/// </summary>
/// <param name="queues">The queues every request goes on into.</param>
internal sealed class RecordingAudioQueues(IGameplayAudioQueues queues) : IGameplayAudioQueues
{
    /// <summary>The queues the requests go on into.</summary>
    private readonly IGameplayAudioQueues queues = Ensure.NotNull(queues);

    /// <summary>The sound requests made since the check's update began, in request order.</summary>
    private readonly List<SoundRequest> sounds = [];

    /// <summary>The music requests made since the check's update began, in request order.</summary>
    private readonly List<GameplayMusicRequest> music = [];

    /// <summary>The sound requests made since the check's update began, in request order.</summary>
    internal IReadOnlyList<SoundRequest> Sounds => sounds;

    /// <summary>The music requests made since the check's update began, in request order.</summary>
    internal IReadOnlyList<GameplayMusicRequest> Music => music;

    /// <summary>Begins a check's update: the requests made before it leave the record.</summary>
    internal void BeginUpdate()
    {
        sounds.Clear();
        music.Clear();
    }

    /// <inheritdoc/>
    public SoundQueueAdmission QueueSound(SoundRequest request)
    {
        sounds.Add(request);
        return queues.QueueSound(request);
    }

    /// <inheritdoc/>
    public void QueueMusic(GameplayMusicRequest request)
    {
        music.Add(request);
        queues.QueueMusic(request);
    }
}
