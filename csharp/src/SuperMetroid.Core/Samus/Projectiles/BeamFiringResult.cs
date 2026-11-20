namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>What firing a beam produced: the sound to queue and its priority.</summary>
/// <param name="Sound">The sound to queue, or null.</param>
/// <param name="Priority">The priority of the sound's request, or none without a sound.</param>
internal readonly record struct BeamFiringResult(SoundEffectId? Sound, SoundRequestPriority? Priority);
