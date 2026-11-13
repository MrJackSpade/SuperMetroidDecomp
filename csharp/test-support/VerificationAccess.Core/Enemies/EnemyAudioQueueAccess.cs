namespace SuperMetroid.Core.Enemies;

/// <summary>Verification reads of the sounds the enemies have queued, through the audio requests their queue joins.</summary>
internal static class EnemyAudioQueueAccess
{
    /// <summary>Reads the sounds a check's enemies have queued since the update's audio began.</summary>
    extension(EnemyAudioQueue self)
    {
        /// <summary>The latest sound the enemies have queued since the update's audio began, or null when they have queued none.</summary>
        internal SoundEffectId? LatestSound => self.SoundRequests.Count > 0 ? self.SoundRequests[^1].SoundEffect : null;

        /// <summary>The sounds the enemies have queued since the update's audio began, in request order, as one comparable text.</summary>
        internal string QueuedSoundList => SoundList(self.SoundRequests.Select(request => request.SoundEffect));
    }

    /// <summary>Writes <paramref name="sounds"/> in order as one comparable text, for an expected sequence beside <c>QueuedSoundList</c>.</summary>
    internal static string SoundList(IEnumerable<SoundEffectId> sounds) => "[" + string.Join(", ", sounds) + "]";
}
