using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's sound and music requests at the end of the enemy update. The materialization cycle and the fight's other
/// sounds are separate sources, so one of each survives an update; a later request from the same source in that
/// update replaces the earlier one (the update-end rule of <see cref="EnemyAudioQueue.RequestSoundAtUpdateEnd"/>).
/// </summary>
internal static class PhantoonAudioRequests
{
    /// <summary>Requests a step of the materialization cycle.</summary>
    internal static void RequestPhantoonMaterializationSound(this EnemyAudioQueue audio, SoundEffectId sound) =>
        audio.RequestSoundAtUpdateEnd(EnemySoundSource.PhantoonMaterialization, sound, SoundRequestPriority.Normal);

    /// <summary>
    /// Drops this update's materialization step: the body's main AI starts each update without one, so only a step its
    /// parts request after it survives.
    /// </summary>
    internal static void CancelPhantoonMaterializationSound(this EnemyAudioQueue audio) =>
        audio.CancelSoundAtUpdateEnd(EnemySoundSource.PhantoonMaterialization);

    /// <summary>Requests a combat sound: a shot cry, a death explosion or the dying cry.</summary>
    internal static void RequestPhantoonCombatSound(this EnemyAudioQueue audio, SoundEffectId sound) =>
        audio.RequestSoundAtUpdateEnd(EnemySoundSource.PhantoonCombat, sound, SoundRequestPriority.Normal);

    /// <summary>Requests Phantoon's music change.</summary>
    internal static void RequestPhantoonMusic(this EnemyAudioQueue audio, MusicRequest request) =>
        audio.RequestMusicAtUpdateEnd(EnemyMusicSource.Phantoon, new DelayedMusicRequest(request, MusicQueueDelay.Shortest));
}
