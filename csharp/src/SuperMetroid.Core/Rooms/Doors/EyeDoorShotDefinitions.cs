using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What Samus's weapons do to an eye door's eye (<see cref="EyeDoorEyeObject"/>).</summary>
internal static partial class EyeDoorShotDefinitions
{
    /// <summary>
    /// The weapon families that hit the eye, with what each does: a missile counts one hit against it, a Super Missile
    /// destroys it at once. Every other shot is rejected with its sound.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusProjectileFamily, EyeDoorHit> Hits = new Dictionary<SamusProjectileFamily, EyeDoorHit>
    {
        [SamusProjectileFamily.Missile] = EyeDoorHit.Counts,
        [SamusProjectileFamily.SuperMissile] = EyeDoorHit.DestroysAtOnce,
    };
}
