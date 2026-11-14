using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// A shot a pose transition hands to the next update's weapon firing (#1470). Its presence admits the turning
/// weapon firing and forces a held charge to release; the shot then leaves in its own direction rather than the
/// direction of the pose Samus is in by then.
/// </summary>
public readonly record struct SamusTransitionShot
{
    /// <summary>
    /// Hands over the shot of a pose aiming in <paramref name="aim"/>: one of the ten aims, or none for a pose that
    /// fires nothing, which hands over a shot with no aim.
    /// </summary>
    public SamusTransitionShot(SamusProjectileDirection? aim) => Aim = aim;

    /// <summary>
    /// The direction the shot leaves in, or null when the pose that handed it over names no aim. No retail transition
    /// hands over a shot with no aim, and firing one fails.
    /// </summary>
    public SamusProjectileDirection? Aim { get; }
}
