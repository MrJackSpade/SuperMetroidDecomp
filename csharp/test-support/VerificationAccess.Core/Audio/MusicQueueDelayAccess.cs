namespace SuperMetroid.Core.Audio;

/// <summary>Verification access to <see cref="MusicQueueDelay"/> members production does not use.</summary>
internal static class MusicQueueDelayAccess
{
    /// <summary>Reconstructs a validated delay without invoking production queueing.</summary>
    extension(MusicQueueDelay)
    {
        /// <summary>
        /// Rehydrates a delay a subsystem already lengthened to the shortest one, rejecting a shorter one.
        /// </summary>
        internal static MusicQueueDelay FromEffectiveUpdates(int updates)
        {
            if (updates < AudioQueueDefinitions.MinimumMusicDelayFrames)
            {
                throw new InvalidDataException(
                    $"Music queue delay {updates} is below the shortest delay of {AudioQueueDefinitions.MinimumMusicDelayFrames} updates.");
            }

            return new MusicQueueDelay(updates);
        }
    }
}
