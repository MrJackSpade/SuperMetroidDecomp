namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Immutable summary of one ordinary-projectile input phase. The pass has already queued its sounds into its audio
/// requests as it made them; the firing sound is also reported here for a cinematic that plays its projectiles' firing
/// sound itself.
/// </summary>
/// <param name="FiringSound">The firing sound's request queued during the pass, with its priority, or none with no request.</param>
public readonly record struct SamusProjectileFrameResult(SoundRequest? FiringSound);
