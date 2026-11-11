namespace SuperMetroid.Core.Audio;

/// <summary>
/// The game's music queue (#1604): music requests in the order they were queued, each waiting for the music handler to
/// fetch it, wait out its delay and play it. It has <see cref="AudioQueueDefinitions.MusicCapacity"/> positions and keeps
/// one free, so it holds at most one fewer request; a request queued with the shortest delay is dropped while it is
/// full (<see cref="QueueUnlessFull"/>), and a scheduled request is written anyway (<see cref="QueueOvertakingWhenFull"/>).
/// </summary>
/// <remarks>
/// <para>
/// Writing a scheduled request into a full queue reproduces the game's behaviour exactly, because it reaches gameplay:
/// the request takes the free position and the queue's write position lands on the request the handler fetches next, so
/// the queue then reads as empty. The handler fetches nothing more, and none of the requests that were waiting play; yet
/// they still hold their positions, so the game still counts music as queued (<see cref="HoldsAnyRequest"/>) and every
/// phase that waits for the music queue to empty keeps waiting. A further request, queued or scheduled, overwrites the
/// request at the fetch position and becomes the one readable request. When the handler retires the request it fetched
/// before the overtaking, once that request plays, the fetch position moves past the write position again, and the
/// hidden requests become readable and play in turn. A permanent item's fanfare empties the queue (<see cref="Clear"/>).
/// </para>
/// <para>
/// The fetch and write positions are kept only to reproduce that overtaking; an empty position holds no request.
/// </para>
/// </remarks>
internal sealed class MusicQueue
{
    /// <summary>The queue's positions; an empty position holds no request.</summary>
    private readonly QueuedMusic?[] _positions = new QueuedMusic?[AudioQueueDefinitions.MusicCapacity];

    /// <summary>The position of the request the handler fetches next, and retires once that request has played.</summary>
    private int _fetchPosition;

    /// <summary>The position the next queued request is written to.</summary>
    private int _writePosition;

    /// <summary>Whether any position holds a request, readable or overtaken: what the game counts as queued music.</summary>
    internal bool HoldsAnyRequest => _positions.Any(position => position is not null);

    /// <summary>
    /// The request the handler fetches next, or none while the queue reads as empty, which it also does after a scheduled
    /// request overtook its waiting requests.
    /// </summary>
    /// <exception cref="InvalidOperationException">A readable position holds no request, which no sequence of operations leaves.</exception>
    internal QueuedMusic? NextToFetch =>
        _fetchPosition == _writePosition
            ? null
            : _positions[_fetchPosition]
              ?? throw new InvalidOperationException($"Music queue position {_fetchPosition} is readable but holds no request.");

    /// <summary>Queues <paramref name="music"/> behind the waiting requests, or drops it when the queue is full.</summary>
    /// <param name="music">The request and its delay.</param>
    internal void QueueUnlessFull(QueuedMusic music)
    {
        if (After(_writePosition) == _fetchPosition)
            return;
        Write(music);
    }

    /// <summary>Queues <paramref name="music"/> behind the waiting requests; a full queue takes it anyway and is overtaken (see the remarks).</summary>
    /// <param name="music">The request and its delay.</param>
    internal void QueueOvertakingWhenFull(QueuedMusic music) => Write(music);

    /// <summary>Retires the request the handler fetched once it has played: its position empties and the next request becomes the one to fetch.</summary>
    internal void RetireFetched()
    {
        _positions[_fetchPosition] = null;
        _fetchPosition = After(_fetchPosition);
    }

    /// <summary>Empties every position, readable or overtaken, so the queue reads as empty and holds nothing.</summary>
    internal void Clear()
    {
        Array.Clear(_positions);
        _fetchPosition = _writePosition;
    }

    /// <summary>Writes <paramref name="music"/> at the write position and moves the write position on.</summary>
    private void Write(QueuedMusic music)
    {
        _positions[_writePosition] = music;
        _writePosition = After(_writePosition);
    }

    /// <summary>The position after <paramref name="position"/>, wrapping to the first after the last.</summary>
    private static int After(int position) => (position + 1) % AudioQueueDefinitions.MusicCapacity;
}
