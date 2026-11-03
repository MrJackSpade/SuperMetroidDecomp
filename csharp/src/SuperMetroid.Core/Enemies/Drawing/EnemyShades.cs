using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// The shades every enemy picture, and every projectile and sprite object picture an enemy spawns, is pre-coloured in
/// (#1512): the looks any enemy can be drawn in besides its own colours. The import pre-colours each installed enemy and
/// spawned-object sheet in all of them.
/// </summary>
internal static class EnemyShades
{
    /// <summary>The hit flash: the picture in the common sprite colours, as a hurt enemy flashes.</summary>
    internal static PictureShade HitFlash { get; } = new("enemy-hit-flash", 0);

    /// <summary>The frozen look: the picture in the ice beam's colours, as a frozen enemy shows.</summary>
    internal static PictureShade Frozen { get; } = new("enemy-frozen", 0);

    /// <summary>The picture in the enemy projectiles' colours, which many spawned objects draw in.</summary>
    internal static PictureShade EnemyProjectileColours { get; } = new("enemy-projectile-colours", 0);

    /// <summary>A draw showing the hit flash.</summary>
    internal static PictureShading HitFlashed { get; } = PictureShading.Shaded(HitFlash);

    /// <summary>A draw showing the frozen look.</summary>
    internal static PictureShading FrozenLook { get; } = PictureShading.Shaded(Frozen);

    /// <summary>A draw in the enemy projectiles' colours.</summary>
    internal static PictureShading InEnemyProjectileColours { get; } = PictureShading.Shaded(EnemyProjectileColours);
}
