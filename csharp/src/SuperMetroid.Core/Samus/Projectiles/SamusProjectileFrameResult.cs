namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Immutable summary of one ordinary-projectile alpha pass.</summary>
/// <param name="QueuedSoundEffect">The primary sound effect queued during the pass, if any.</param>
/// <param name="QueuedSoundMaximum">The queue limit of the primary request, or none with no request.</param>
/// <param name="AdditionalSoundRequests">Additional ordered sound requests emitted by the pass.</param>
/// <param name="QueuedSoundSuppressed">Whether the primary sound request was deliberately suppressed.</param>
public readonly record struct SamusProjectileFrameResult(
    SoundEffectId? QueuedSoundEffect,
    SoundQueueLimit? QueuedSoundMaximum,
    IReadOnlyList<SoundRequest>? AdditionalSoundRequests = null,
    bool QueuedSoundSuppressed = false);
