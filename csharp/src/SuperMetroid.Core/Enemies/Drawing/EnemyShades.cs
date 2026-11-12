using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// The shades every enemy picture, and every projectile and sprite object picture an enemy spawns, is pre-coloured in
/// (#1512, #1544): the looks any enemy can be drawn in besides its own colours. The import pre-colours each installed
/// spawned-object sheet in all of them, and each enemy sheet in those its enemy can be drawn in (#1643): an enemy that is
/// never frozen has no frozen look, so a draw asking its sheet for one fails.
/// </summary>
internal static class EnemyShades
{
    /// <summary>The hurt flash: the flash colours a hurt enemy shows on the updates it flashes.</summary>
    internal static PictureShade HurtFlash { get; } = new("enemy-hit-flash", 0);

    /// <summary>The frozen look: the picture in the ice beam's colours, as a frozen enemy shows.</summary>
    internal static PictureShade Frozen { get; } = new("enemy-frozen", 0);

    /// <summary>The look enemy projectiles share: the common projectile colours, which many spawned objects show.</summary>
    internal static PictureShade ProjectileLook { get; } = new("enemy-projectile-colours", 0);

    /// <summary>A draw showing the hurt flash.</summary>
    internal static PictureShading HurtFlashLook { get; } = PictureShading.Shaded(HurtFlash);

    /// <summary>A draw showing the frozen look.</summary>
    internal static PictureShading FrozenLook { get; } = PictureShading.Shaded(Frozen);

    /// <summary>A draw in the projectiles' shared look.</summary>
    internal static PictureShading InProjectileLook { get; } = PictureShading.Shaded(ProjectileLook);
}
