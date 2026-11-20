using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// A shot a pose transition hands to the next update's weapon firing (#1470). Its presence admits the turning
/// weapon firing and forces a held charge to release; the shot then leaves in its own direction rather than the
/// direction of the pose Samus is in by then.
/// </summary>
public readonly record struct SamusTransitionShot
{
    /// <summary>Hands over the shot of a pose aiming in <paramref name="aim"/>.</summary>
    public SamusTransitionShot(SamusProjectileDirection aim) => Aim = aim;

    /// <summary>The direction the shot leaves in.</summary>
    public SamusProjectileDirection Aim { get; }

    /// <summary>
    /// The shot of the pose <paramref name="samus"/> is in, or none for a pose that aims nowhere and so has no shot to hand
    /// over. Only the normal-jump setup and the moonwalk transition hand a shot over, and every normal-jump and moonwalk
    /// pose aims one of the ten aims (#1506).
    /// </summary>
    internal static SamusTransitionShot? OfPose(SamusState samus) =>
        SamusPoseMetadata.ReadAimDirection(samus) is { } aim ? new SamusTransitionShot(aim) : null;
}
