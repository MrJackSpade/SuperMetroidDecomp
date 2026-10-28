using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Chooses the program of each of Mother Brain's death explosions by its projectile parameter.</summary>
internal static class MotherBrainDeathExplosions
{
    /// <summary>The program projectile parameter zero through two selects.</summary>
    internal static MiscDustProjectileProgram Program(int parameter) =>
        (uint)parameter < (uint)MotherBrainDeathExplosionDefinitions.Programs.Length
            ? MotherBrainDeathExplosionDefinitions.Programs[parameter]
            : throw new ArgumentOutOfRangeException(
                nameof(parameter), parameter, "Mother Brain death explosion parameter must be zero through two.");
}
