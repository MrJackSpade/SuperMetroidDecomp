namespace SuperMetroid.Core.Audio;

/// <summary>
/// A check's record of the sound requests gameplay makes (#2137, #2499): each request is recorded, in request order, and
/// goes straight on into the sound queues the record wraps, which treat it exactly as they would without the record.
/// </summary>
/// <param name="queues">The sound queues every request goes on into.</param>
internal sealed partial class RecordingSoundQueue(IGameplaySoundQueue queues) : IGameplaySoundQueue
{
    /// <summary>The sound queues the requests go on into.</summary>
    private readonly IGameplaySoundQueue queues = Ensure.NotNull(queues);

    /// <summary>The sound requests made since the check's update began, in request order.</summary>
    private readonly List<SoundRequest> sounds = [];

    /// <summary>The sound requests made since the check's update began, in request order.</summary>
    internal IReadOnlyList<SoundRequest> Sounds => sounds;

    /// <summary>Begins a check's update: the requests made before it leave the record.</summary>
    internal void BeginUpdate() => sounds.Clear();

    /// <inheritdoc/>
    public SoundQueueAdmission QueueSound(SoundRequest request)
    {
        sounds.Add(request);
        return queues.QueueSound(request);
    }
}
