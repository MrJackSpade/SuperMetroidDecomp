using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Projectile animation playbacks the collision and drawing fixtures use (#1301, #1600).</summary>
internal static class ProjectileFixturePlaybacks
{
    /// <summary>
    /// A live shot for fixtures that only need the slot active (its native pointer nonzero): they check collision,
    /// damage or drawing and never play the shot's animation.
    /// </summary>
    internal static AnimationPlayback<SamusProjectileSegment> Active { get; } =
        new(SamusProjectileAnimations.StartingWith(SamusProjectileSegment.PowerRight), AnimationPlace.StartOfAnimation);
}
