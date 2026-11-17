namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>What firing a beam produced: the shot place taken, if any, and the sound to queue and its priority.</summary>
/// <param name="Place">The place the shot took among Samus's shots, or null.</param>
/// <param name="Sound">The sound to queue, or null.</param>
/// <param name="Priority">The priority of the sound's request, or none without a sound.</param>
internal readonly record struct BeamFiringResult(int? Place, SoundEffectId? Sound, SoundRequestPriority? Priority);
