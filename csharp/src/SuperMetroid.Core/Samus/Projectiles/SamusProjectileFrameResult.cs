namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Immutable summary of one ordinary-projectile alpha pass.</summary>
/// <param name="QueuedSoundEffect">The primary sound effect queued during the pass, if any.</param>
/// <param name="QueuedSoundMaximum">The maximum number of simultaneous sounds accepted for the primary request: zero with no request, otherwise 1 through 15.</param>
/// <param name="AdditionalSoundRequests">Additional ordered sound requests emitted by the pass.</param>
/// <param name="QueuedSoundSuppressed">Whether the primary sound request was deliberately suppressed.</param>
/// <param name="PersistentMemoryCorrupted">Whether the modeled projectile path corrupted persistent memory.</param>
public readonly record struct SamusProjectileFrameResult(
    SoundEffectId? QueuedSoundEffect,
    int QueuedSoundMaximum,
    IReadOnlyList<SamusSoundRequest>? AdditionalSoundRequests = null,
    bool QueuedSoundSuppressed = false,
    bool PersistentMemoryCorrupted = false);
