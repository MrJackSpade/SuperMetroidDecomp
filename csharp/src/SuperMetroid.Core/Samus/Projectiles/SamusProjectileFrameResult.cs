namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Immutable summary of one ordinary-projectile alpha pass. The pass has already queued its sounds into its audio
/// requests as it made them; the firing sound is also reported here for a cinematic that plays its projectiles' firing
/// sound itself.
/// </summary>
/// <param name="QueuedSoundEffect">The firing sound queued during the pass, if any.</param>
/// <param name="QueuedSoundMaximum">The queue limit of the firing request, or none with no request.</param>
/// <param name="QueuedSoundSuppressed">Whether the firing sound request was deliberately suppressed.</param>
public readonly record struct SamusProjectileFrameResult(
    SoundEffectId? QueuedSoundEffect,
    SoundRequestPriority? QueuedSoundMaximum,
    bool QueuedSoundSuppressed = false);
