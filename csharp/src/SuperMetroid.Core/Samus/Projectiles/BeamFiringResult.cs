namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>What firing a beam produced (#2560): the sound request to queue, the sound with its priority, or none.</summary>
/// <param name="Request">The sound to queue with its priority, or null for none.</param>
internal readonly partial record struct BeamFiringResult(SoundRequest? Request)
{
    /// <summary>No sound to queue.</summary>
    internal static BeamFiringResult Silent => default;

    /// <summary>The request for <paramref name="sound"/> at <paramref name="priority"/>, or none without a sound.</summary>
    internal static BeamFiringResult Of(SoundEffectId? sound, SoundRequestPriority priority) =>
        new(sound is { } queued ? new SoundRequest(queued, priority) : null);
}
