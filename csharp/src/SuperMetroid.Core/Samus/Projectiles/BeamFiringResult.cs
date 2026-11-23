namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The sound firing a beam queues and its request's priority (#2554). Firing that queues no sound produces none, so a
/// sound is never without its priority.
/// </summary>
/// <param name="Sound">The sound to queue.</param>
/// <param name="Priority">The priority of the sound's request.</param>
internal readonly partial record struct BeamFiringResult(SoundEffectId Sound, SoundRequestPriority Priority)
{
    /// <summary>The sound <paramref name="sound"/> queued at <paramref name="priority"/>, or none when firing queues no sound.</summary>
    /// <param name="sound">The sound firing produced, or none.</param>
    /// <param name="priority">The priority its request takes.</param>
    internal static BeamFiringResult? Of(SoundEffectId? sound, SoundRequestPriority priority) =>
        sound is { } queued ? new BeamFiringResult(queued, priority) : null;
}
