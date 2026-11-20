namespace SuperMetroid.Core.Audio;

/// <summary>
/// One sound library's requests waiting for the library's queue pass, in the order they were queued (#1604). The pass takes
/// the oldest waiting request when the library is ready for one (<see cref="TakeOldest"/>).
/// </summary>
/// <remarks>
/// A queue never holds more than <see cref="SoundQueueAdmission.MaximumOccupancy"/> requests: a request joins only through
/// <see cref="Admit"/>, which admits it by <see cref="SoundQueueAdmission.For"/> from the queue's own count, and every
/// priority turns a request away once that many wait. The game's own handling of a request arriving at a queue with no
/// room is therefore never reached.
/// </remarks>
internal sealed class SoundQueue
{
    /// <summary>The waiting requests, oldest first.</summary>
    private readonly List<SoundEffectId> Waiting = [];

    /// <summary>The waiting requests, 0 through <see cref="SoundQueueAdmission.MaximumOccupancy"/>.</summary>
    internal int Count => Waiting.Count;

    /// <summary>
    /// Admits <paramref name="sound"/> by its <paramref name="priority"/> and how many requests wait, and queues it behind
    /// every request already waiting when it is admitted and <paramref name="suppression"/> lets it join.
    /// </summary>
    /// <param name="sound">The request.</param>
    /// <param name="priority">How full the queue may be for the request to be admitted; tested before suppression.</param>
    /// <param name="suppression">What turns sounds away now.</param>
    internal SoundQueueAdmission Admit(SoundEffectId sound, SoundRequestPriority priority, SoundSuppression suppression)
    {
        SoundQueueAdmission admission = SoundQueueAdmission.For(Waiting.Count, priority);
        if (admission.Joins(suppression))
            Waiting.Add(sound);
        return admission;
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
