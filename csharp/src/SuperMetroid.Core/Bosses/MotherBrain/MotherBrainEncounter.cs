using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain encounter state shared across Tourian: her setup's attachment to the room, the warning Zebes's escape
/// shows, and the encounter's sound and music requests.
/// </summary>
internal sealed partial class MotherBrainEncounter
{
    /// <summary>Receives the encounter's sound and music requests.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>The encounters of the room's bosses, to which her body's setup attaches her (#2194).</summary>
    private readonly RoomBossEncounters _encounters;

    /// <summary>Binds the encounter's requests to the enemy audio queue and her setup to the room's boss encounters.</summary>
    internal MotherBrainEncounter(EnemyAudioQueue audio, RoomBossEncounters encounters)
    {
        _audio = audio;
        _encounters = encounters;
    }

    /// <summary>
    /// Begins <paramref name="motherBrain"/>'s encounter as her body sets up: the room's display, colours and escape text
    /// and her brain's late draw read her from the room from here (#2194). No part reads it there; each of her parts, the
    /// tubes and the Baby hold the encounter they joined.
    /// </summary>
    /// <returns>The encounter begun.</returns>
    internal MotherBrainEnemyState Begin(MotherBrainEnemyState motherBrain)
    {
        _encounters.Attach(Ensure.NotNull(motherBrain));
        return motherBrain;
    }

    /// <summary>
    /// Queues a sound of the body's fight, fake death and tube collapse, or of the Baby Metroid's cries in the cutscene,
    /// as it is made, after the update's earlier requests.
    /// </summary>
    internal void RequestBodySound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>Queues the rainbow beam's drain sound starting or stopping.</summary>
    internal void RequestRainbowBeamSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>Queues a sound a Mother Brain animation cue makes, at normal priority.</summary>
    internal void RequestCueSound(SoundEffectId sound) => _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>Queues a sound request that carries its own priority, as an explosion's does.</summary>
    internal void RequestCueSound(SoundRequest request) => _audio.QueueEnemySound(request.SoundEffect, request.Priority);

    /// <summary>
    /// Queues one music change. Mother Brain's changes come in sequences (stop, load a data set, select a track), each
    /// queued after the one before it.
    /// </summary>
    internal void RequestMusic(MusicRequest request, MusicQueueDelay delay) =>
        _audio.QueueEnemyMusic(new DelayedMusicRequest(request, delay));
}
