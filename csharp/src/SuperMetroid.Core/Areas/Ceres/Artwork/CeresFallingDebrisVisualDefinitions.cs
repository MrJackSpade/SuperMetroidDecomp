using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>The Ceres falling tiles' installed frames and their projectile definitions' animation identities.</summary>
internal static class CeresFallingDebrisVisualDefinitions
{
    /// <summary>
    /// The graphics the falling tiles draw in: the shared sheet in the doors' escaping colours, which the tiles show
    /// because they fall only once Ridley has escaped (#1464, #1512).
    /// </summary>
    internal static SpriteGraphics Graphics { get; } = SpriteGraphics.SharedIn(CeresDoorShades.InEscapingColours);

    /// <summary>The projectile frame each piece's look shows.</summary>
    internal static IReadOnlyDictionary<CeresFallingDebrisLook, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<CeresFallingDebrisLook, EnemyProjectileFrameId>
        {
            [CeresFallingDebrisLook.Light] = EnemyProjectileFrameId.CeresFallingDebrisLight,
            [CeresFallingDebrisLook.Dark] = EnemyProjectileFrameId.CeresFallingDebrisDark,
        };
}
