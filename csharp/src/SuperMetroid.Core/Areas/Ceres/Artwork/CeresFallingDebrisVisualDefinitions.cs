using SuperMetroid.Core.Enemies.Projectiles;

using P = SuperMetroid.Core.Areas.Ceres.CeresFallingDebrisProgram;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>The Ceres falling tiles' installed frames and their projectile definitions' native list identities.</summary>
internal static class CeresFallingDebrisVisualDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_CeresFallingTile_Light</c> at $86:9750: the list the light tile's projectile definition names.</summary>
    internal const ushort LightDefinitionList = 0x9750;
    /// <summary><c>InstList_EnemyProjectile_CeresFallingTile_Dark</c> at $86:9756: the list the dark tile's projectile definition names.</summary>
    internal const ushort DarkDefinitionList = 0x9756;

    /// <summary>The frames in installed order.</summary>
    internal static IReadOnlyList<CeresFallingDebrisProgram> Frames { get; } = [P.Light, P.Dark];

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<CeresFallingDebrisProgram, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<CeresFallingDebrisProgram, EnemyProjectileFrameId>
        {
            [CeresFallingDebrisProgram.Light] = EnemyProjectileFrameId.CeresFallingDebrisFrame00,
            [CeresFallingDebrisProgram.Dark] = EnemyProjectileFrameId.CeresFallingDebrisFrame01,
        };
}
