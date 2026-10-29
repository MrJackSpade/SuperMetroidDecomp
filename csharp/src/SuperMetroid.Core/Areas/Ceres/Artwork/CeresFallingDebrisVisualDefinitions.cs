using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>The Ceres falling tiles' installed frames and their projectile definitions' native list identities.</summary>
internal static class CeresFallingDebrisVisualDefinitions
{
    /// <summary>
    /// The graphics the falling tiles draw in: character zero in the escaping doors' colours, which every Ceres door
    /// loads once Ridley has escaped, the only time the tiles fall (#1464).
    /// </summary>
    internal static SpriteGraphics Graphics { get; } =
        SpriteGraphics.CharacterZeroIn(new EncounterSpriteColouring(CeresDoorColourDefinitions.Active));

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<CeresFallingDebrisProgram, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<CeresFallingDebrisProgram, EnemyProjectileFrameId>
        {
            [CeresFallingDebrisProgram.Light] = EnemyProjectileFrameId.CeresFallingDebrisFrame00,
            [CeresFallingDebrisProgram.Dark] = EnemyProjectileFrameId.CeresFallingDebrisFrame01,
        };
}
