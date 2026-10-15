using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Projectile list positions the collision and drawing fixtures use (#1301).</summary>
internal static class ProjectileFixtureCursors
{
    /// <summary>
    /// A live shot for fixtures that only need the slot active (its native pointer nonzero): they check collision,
    /// damage or drawing and never run the shot's list.
    /// </summary>
    internal static AnimationProgramCursor<SamusProjectileProgram> Active { get; } =
        AnimationProgramCursor<SamusProjectileProgram>.Start(SamusProjectileProgram.PowerRight);
}
