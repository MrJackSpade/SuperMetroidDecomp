using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Chooses the program of each of Mother Brain's death explosions by its projectile parameter.</summary>
internal static class MotherBrainDeathExplosions
{
    /// <summary>The program projectile parameter zero through two selects.</summary>
    internal static MiscDustProjectileSegment Animation(int parameter) =>
        (uint)parameter < (uint)MotherBrainDeathExplosionDefinitions.Animations.Length
            ? MotherBrainDeathExplosionDefinitions.Animations[parameter]
            : throw new ArgumentOutOfRangeException(
                nameof(parameter), parameter, "Mother Brain death explosion parameter must be zero through two.");
}
