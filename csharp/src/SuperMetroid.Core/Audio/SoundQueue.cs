namespace SuperMetroid.Core.Audio;

/// <summary>
/// One sound library's requests waiting for its handler, in the order they were queued (#1604). The handler asks its
/// library for the oldest waiting request when its handshake is ready (<see cref="TakeOldest"/>).
/// </summary>
/// <remarks>
/// A queue never holds more than <see cref="SoundQueueAdmission.MaximumOccupancy"/> requests: every request is admitted
/// by <see cref="SoundQueueAdmission.For"/> from <see cref="Count"/>, and every priority turns a request away once that
/// many wait. The game's own handling of a request arriving at a queue with no room is therefore never reached, and the
/// queue fails loudly instead of reproducing it (<see cref="Add"/>).
/// </remarks>
internal sealed class SoundQueue
{
    /// <summary>The waiting requests, oldest first.</summary>
    private readonly List<SoundEffectId> _waiting = [];

    /// <summary>The waiting requests, 0 through <see cref="SoundQueueAdmission.MaximumOccupancy"/>.</summary>
    internal int Count => _waiting.Count;

    /// <summary>The waiting requests, oldest first; a live view that changes as requests are queued and taken.</summary>
    internal IReadOnlyList<SoundEffectId> Waiting => _waiting;

    /// <summary>Queues <paramref name="sound"/> behind every request already waiting.</summary>
    /// <param name="sound">An admitted request.</param>
    /// <exception cref="InvalidOperationException">
    /// <see cref="SoundQueueAdmission.MaximumOccupancy"/> requests already wait, so the request was never admitted.
    /// </exception>
    internal void Add(SoundEffectId sound)
    {
        if (_waiting.Count >= SoundQueueAdmission.MaximumOccupancy)
        {
            throw new InvalidOperationException(
                $"{sound} joined a sound queue already holding {_waiting.Count} requests; admission turns it away first.");
        }
        _waiting.Add(sound);
    }

    /// <summary>Removes and returns the oldest waiting request, or none when nothing waits.</summary>
    internal SoundEffectId? TakeOldest()
    {
        if (_waiting.Count == 0)
            return null;
        SoundEffectId oldest = _waiting[0];
        _waiting.RemoveAt(0);
        return oldest;
    }

    /// <summary>Drops every waiting request.</summary>
    internal void Clear() => _waiting.Clear();
}
