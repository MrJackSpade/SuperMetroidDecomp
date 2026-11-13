using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's sound and music requests, each queued as Phantoon makes it, after the update's earlier requests.</summary>
internal static class PhantoonAudioRequests
{
    /// <summary>Queues a step of the materialization cycle.</summary>
    internal static void RequestPhantoonMaterializationSound(this EnemyAudioQueue audio, SoundEffectId sound) =>
        audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>Queues a combat sound: a shot cry, a death explosion or the dying cry.</summary>
    internal static void RequestPhantoonCombatSound(this EnemyAudioQueue audio, SoundEffectId sound) =>
        audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>Queues Phantoon's music change.</summary>
    internal static void RequestPhantoonMusic(this EnemyAudioQueue audio, MusicRequest request) =>
        audio.QueueEnemyMusic(new DelayedMusicRequest(request, MusicQueueDelay.Shortest));
}
