using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's requests at the end of the enemy update. A later request from the same source in that update replaces the
/// earlier one (the update-end rule of <see cref="EnemyAudioQueue.RequestSoundAtUpdateEnd"/>).
/// </summary>
internal static class RidleyAudioRequests
{
    /// <summary>Requests the sound of a small explosion around the dying Norfair Ridley.</summary>
    internal static void RequestRidleyDeathExplosionSound(this EnemyAudioQueue audio) =>
        audio.RequestSoundAtUpdateEnd(EnemySoundSource.Ridley, RidleySoundDefinitions.DeathExplosion, SoundRequestPriority.Low);

    /// <summary>Requests a music change from either Ridley encounter.</summary>
    internal static void RequestRidleyMusic(this EnemyAudioQueue audio, MusicRequest request) =>
        audio.RequestMusicAtUpdateEnd(EnemyMusicSource.Ridley, new DelayedMusicRequest(request, MusicQueueDelay.Shortest));
}
