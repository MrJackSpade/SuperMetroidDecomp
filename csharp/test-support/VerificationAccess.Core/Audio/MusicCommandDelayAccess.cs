using SuperMetroid.Core.Audio;

/// <summary>Verification access to <see cref="MusicCommandDelay"/> members production does not use.</summary>
internal static class MusicCommandDelayAccess
{
    /// <summary>Reconstructs a validated effective delay without invoking production queueing.</summary>
    extension(MusicCommandDelay)
    {
        /// <summary>
        /// Rehydrates an already-effective delay published by a translated subsystem.
        /// </summary>
        internal static MusicCommandDelay FromEffectiveFrames(ushort frames)
        {
            if (frames < AudioRomData.Queues.MinimumMusicDelayFrames)
            {
                throw new InvalidDataException(
                    $"Effective music delay {frames} is below bank $80's minimum of eight frames.");
            }

            return new MusicCommandDelay(frames);
        }
    }
}
