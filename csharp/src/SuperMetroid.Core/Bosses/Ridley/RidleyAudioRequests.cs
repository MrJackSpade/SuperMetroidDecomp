using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's sound and music requests, each queued as Ridley makes it, after the update's earlier requests.</summary>
internal static class RidleyAudioRequests
{
    /// <summary>Queues the sound of a small explosion around the dying Norfair Ridley.</summary>
    internal static void RequestRidleyDeathExplosionSound(this EnemyAudioQueue audio) =>
        audio.QueueEnemySound(RidleySoundDefinitions.DeathExplosion, SoundRequestPriority.Low);

    /// <summary>Queues a music change from either Ridley encounter.</summary>
    internal static void RequestRidleyMusic(this EnemyAudioQueue audio, MusicRequest request) =>
        audio.QueueEnemyMusic(new DelayedMusicRequest(request, MusicQueueDelay.Shortest));
}
