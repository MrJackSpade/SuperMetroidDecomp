namespace SuperMetroid.Core.Audio;

/// <summary>
/// A check's record of the music requests gameplay makes (#2137, #2499): each request is recorded, in request order, and
/// goes straight on into the music queue the record wraps.
/// </summary>
/// <param name="queue">The music queue every request goes on into.</param>
internal sealed partial class RecordingMusicQueue(IGameplayMusicQueue queue) : IGameplayMusicQueue
{
    /// <summary>The music queue the requests go on into.</summary>
    private readonly IGameplayMusicQueue queue = Ensure.NotNull(queue);

    /// <summary>The music requests made since the check's update began, in request order.</summary>
    private readonly List<GameplayMusicRequest> music = [];

    /// <summary>The music requests made since the check's update began, in request order.</summary>
    internal IReadOnlyList<GameplayMusicRequest> Music => music;

    /// <summary>Begins a check's update: the requests made before it leave the record.</summary>
    internal void BeginUpdate() => music.Clear();

    /// <inheritdoc/>
    public void QueueMusic(GameplayMusicRequest request)
    {
        music.Add(request);
        queue.QueueMusic(request);
    }
}
