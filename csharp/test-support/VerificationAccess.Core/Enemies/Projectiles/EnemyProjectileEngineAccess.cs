using SuperMetroid.Core.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Verification's single-projectile animation pass, for checks whose cues meet no bomb of Samus's.</summary>
internal static class EnemyProjectileEngineAccess
{
    /// <summary>Runs one projectile's animation as the projectile pass does.</summary>
    extension(EnemyProjectileEngine engine)
    {
        /// <summary>
        /// Advances <paramref name="projectile"/>'s typed animation one projectile pass, as the pass does, with idle new
        /// Samus bombs: no power bomb explodes for a cue to test (#2159).
        /// </summary>
        internal void AdvanceEnemyProjectileAnimations(EnemyProjectile projectile, SamusState samus, Position16<RoomPixels> camera,
            SuperMetroid.Core.Compatibility.ShortFrameCount shortCounter) =>
            engine.AdvanceEnemyProjectileAnimations(projectile, samus, camera, shortCounter, InstalledFixtures.CreateBombSystem());
    }
}
