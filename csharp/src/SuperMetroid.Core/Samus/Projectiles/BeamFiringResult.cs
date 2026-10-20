namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>What firing a beam produced: the projectile slot used, if any, and the sound to queue under its limit.</summary>
/// <param name="Slot">The projectile slot fired into, or null.</param>
/// <param name="Sound">The sound to queue, or null.</param>
/// <param name="MaximumQueued">The queue limit for the sound.</param>
internal readonly record struct BeamFiringResult(int? Slot, SoundEffectId? Sound, int MaximumQueued);
