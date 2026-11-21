namespace SuperMetroid.Core.Audio;

/// <summary>
/// The way gameplay requests sounds (#1620, #2499): every system that makes sounds makes its request here the moment it
/// makes it, and the request goes straight on into the sound queues these requests reach, so the queues receive an
/// update's requests in its run order across every system. A system that also requests music takes
/// <see cref="GameplayAudioRequests"/>; one that only makes sounds, as a cinematic's Samus, shots and bombs do, takes these.
/// </summary>
/// <remarks>
/// The requests always reach queues (#2057): a runtime's reach its session's, a cinematic's the frontend's, a check's queues
/// of its own; every system that makes sounds takes the requests it joins when it is built, so no request is ever made
/// that reaches no queue. Evidence for the run order: the game's main gameplay update runs the colour effects, Samus's
/// input handling (which fires her weapons), the projectiles' hits on enemies, the enemies, Samus's movement and
/// animation, the enemy projectiles, the room objects, the enemy projectiles' hits, the power bomb's hits, the drawing and
/// the HUD in turn, and each of them calls the game's sound queues at the point it runs; none holds a request back for a
/// later system.
/// </remarks>
internal partial class GameplaySoundRequests
{
    /// <summary>The sound queues the requests go on into.</summary>
    internal readonly IGameplaySoundQueue sounds;

    /// <summary>
    /// Requests that go on into <paramref name="sounds"/>. The queues reach the session, which reaches the systems holding
    /// these requests, so a stored state restores the requests before what their queues hold (#2602): an ordinary
    /// constructor, not a primary one, whose arguments a restore would have to rebuild first.
    /// </summary>
    /// <param name="sounds">The sound queues every request goes on into as it is made.</param>
    internal GameplaySoundRequests(IGameplaySoundQueue sounds) => this.sounds = Ensure.NotNull(sounds);

    /// <summary>
    /// Requests <paramref name="request"/>'s sound now, after the update's earlier sound requests, returning what its queue
    /// did with it.
    /// </summary>
    internal SoundQueueAdmission QueueSound(SoundRequest request) => sounds.QueueSound(request);

    /// <summary>Requests <paramref name="soundEffect"/> now (see <see cref="QueueSound(SoundRequest)"/>).</summary>
    /// <param name="soundEffect">The sound.</param>
    /// <param name="priority">How full its queue may be for the request to join it.</param>
    internal SoundQueueAdmission QueueSound(SoundEffectId soundEffect, SoundRequestPriority priority) =>
        QueueSound(new SoundRequest(soundEffect, priority));
}
