namespace SuperMetroid.Core.Audio;

/// <summary>
/// One sound library's requests waiting for the library's queue pass, in the order they were queued (#1604). The pass takes
/// the oldest waiting request when the library is ready for one (<see cref="TakeOldest"/>).
/// </summary>
/// <remarks>
/// A queue never holds more than <see cref="SoundQueueAdmission.MaximumOccupancy"/> requests: every request is admitted
/// by <see cref="SoundQueueAdmission.For"/> from <see cref="Count"/>, and every priority turns a request away once that
/// many wait, so a request always finds room (<see cref="Add"/>).
/// </remarks>
internal sealed class SoundQueue
{
    /// <summary>The waiting requests, oldest first.</summary>
    internal readonly List<SoundEffectId> Waiting = [];

    /// <summary>The waiting requests, 0 through <see cref="SoundQueueAdmission.MaximumOccupancy"/>.</summary>
    internal int Count => Waiting.Count;

    /// <summary>Queues <paramref name="sound"/> behind every request already waiting.</summary>
    /// <param name="sound">An admitted request.</param>
    /// <exception cref="InvalidOperationException">
    /// <see cref="SoundQueueAdmission.MaximumOccupancy"/> requests already wait, so the request was never admitted.
    /// </exception>
    internal void Add(SoundEffectId sound)
    {
        if (Waiting.Count >= SoundQueueAdmission.MaximumOccupancy)
        {
            throw new InvalidOperationException(
                $"{sound} joined a sound queue already holding {Waiting.Count} requests; admission turns it away first.");
        }
        Waiting.Add(sound);
    }

    /// <summary>Removes and returns the oldest waiting request, or none when nothing waits.</summary>
    internal SoundEffectId? TakeOldest()
    {
        if (Waiting.Count == 0)
            return null;
        SoundEffectId oldest = Waiting[0];
        Waiting.RemoveAt(0);
        return oldest;
    }

    /// <summary>Drops every waiting request.</summary>
    internal void Clear() => Waiting.Clear();
}
