namespace SuperMetroid.Core.Audio;

/// <summary>One request for the sound queue, whichever system made it.</summary>
/// <param name="SoundEffect">The library-qualified sound identity supplied by the original caller.</param>
/// <param name="MaximumQueued">How full the sound's queue may be for the request to join it, as that caller chose.</param>
/// <param name="SoundSuppressed">Native queue guard captured at the request, not deferred publication.</param>
public readonly record struct SoundRequest(SoundEffectId SoundEffect, SoundQueueLimit MaximumQueued, bool SoundSuppressed = false)
{
    /// <summary>
    /// A request whose caller states its queue limit as the count of unread requests, the way the original's sound-queue
    /// entry points name it (Max6 is six); the count becomes the request's <see cref="SoundQueueLimit"/> here.
    /// </summary>
    /// <param name="SoundEffect">The library-qualified sound identity supplied by the original caller.</param>
    /// <param name="MaximumQueued">The unread requests at which the request is dropped, 1 through <see cref="AudioQueueDefinitions.MaximumSoundOccupancy"/>.</param>
    /// <param name="SoundSuppressed">Native queue guard captured at the request, not deferred publication.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="MaximumQueued"/> is outside 1 through <see cref="AudioQueueDefinitions.MaximumSoundOccupancy"/>.</exception>
    public SoundRequest(SoundEffectId SoundEffect, int MaximumQueued, bool SoundSuppressed = false)
        : this(SoundEffect, new SoundQueueLimit(MaximumQueued), SoundSuppressed)
    {
    }
}
