using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Chooses the animation of each of Mother Brain's death explosions by its projectile parameter.</summary>
internal static class MotherBrainDeathExplosions
{
    /// <summary>The animation of one of the three death explosion choices, zero through two.</summary>
    internal static MiscDustProjectileSegment Animation(int parameter) =>
        (uint)parameter < (uint)MotherBrainDeathExplosionDefinitions.Animations.Length
            ? MotherBrainDeathExplosionDefinitions.Animations[parameter]
            : throw new ArgumentOutOfRangeException(
                nameof(parameter), parameter, "Mother Brain death explosion parameter must be zero through two.");
}
