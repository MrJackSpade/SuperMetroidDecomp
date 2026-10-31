namespace SuperMetroid.Core.Enemies;

/// <summary>Verification reads of <see cref="EnemyAudioQueue"/>'s end-of-update requests through its production operations.</summary>
internal static class EnemyAudioQueueAccess
{
    /// <summary>Reads the end-of-update requests a check's enemy pass has made so far.</summary>
    extension(EnemyAudioQueue self)
    {
        /// <summary>
        /// The sound <paramref name="source"/> has pending for the end of this update, or null when it has none. The read
        /// publishes the source, as the end of the enemy pass does, so the request also joins
        /// <see cref="EnemyAudioQueue.SoundRequests"/>.
        /// </summary>
        internal SoundEffectId? PublishedSoundFrom(EnemySoundSource source)
        {
            int before = self.SoundRequests.Count;
            self.PublishSoundsFrom(source);
            return self.SoundRequests.Count > before ? self.SoundRequests[^1].SoundEffect : null;
        }
    }
}
