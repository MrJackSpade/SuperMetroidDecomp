namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>What firing a beam produced: the shot place taken, if any, and the sound to queue under its limit.</summary>
/// <param name="Place">The place the shot took among Samus's shots, or null.</param>
/// <param name="Sound">The sound to queue, or null.</param>
/// <param name="Priority">The queue limit for the sound, or none without a sound.</param>
internal readonly record struct BeamFiringResult(int? Place, SoundEffectId? Sound, SoundRequestPriority? Priority);
