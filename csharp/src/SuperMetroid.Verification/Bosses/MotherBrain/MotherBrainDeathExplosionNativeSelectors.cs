using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Verification's native view of Mother Brain's death explosions (#1867): the cartridge chose an explosion by its
/// projectile parameter, an index into its three-entry selector table at $86:C929.
/// </summary>
internal static class MotherBrainDeathExplosionNativeSelectors
{
    /// <summary>The explosion each native parameter selects, in parameter order.</summary>
    internal static readonly MotherBrainDeathExplosionAnimation[] Animations =
        [MotherBrainDeathExplosionAnimation.SmallExplosion, MotherBrainDeathExplosionAnimation.Smoke, MotherBrainDeathExplosionAnimation.BigExplosion];
}
