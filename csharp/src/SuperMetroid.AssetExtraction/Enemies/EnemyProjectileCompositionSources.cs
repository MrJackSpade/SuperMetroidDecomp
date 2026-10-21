using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The bank-$8D spritemap each enemy-projectile composition frame is stored at.</summary>
internal static class EnemyProjectileCompositionSources
{
    /// <summary>The empty bank-$8D projectile spritemap at $8D:8000, which draws nothing.</summary>
    internal const ushort BlankSpritemap = 0x8000;

    /// <summary>The spritemap of each installed composition.</summary>
    private static readonly IReadOnlyDictionary<EnemyProjectileFrameId, ushort> Spritemaps =
        new Dictionary<EnemyProjectileFrameId, ushort>
    {
        // Spritemap_CeresElevatorPad_0.
        [EnemyProjectileFrameId.CeresElevatorPad0] = 0xb1ba,
        // Spritemap_CeresElevatorPad_1.
        [EnemyProjectileFrameId.CeresElevatorPad1] = 0xb1d0,
        // Spritemap_CeresElevatorPlatform.
        [EnemyProjectileFrameId.CeresElevatorPlatform] = 0x846d,
        // The Skree-debris composition selected by $86:8ABF.
        [EnemyProjectileFrameId.SkreeDebris] = 0x8023,
        // The Metaree-debris composition selected by $86:8AC7.
        [EnemyProjectileFrameId.MetareeDebris] = 0x8435,
    };

    /// <summary>The bank-$8D spritemap composition frame <paramref name="frame"/> is stored at.</summary>
    internal static ushort Spritemap(EnemyProjectileFrameId frame) => frame == EnemyProjectileFrameId.Blank
        ? BlankSpritemap
        : Spritemaps.TryGetValue(frame, out ushort spritemap)
            ? spritemap
            : throw new ArgumentOutOfRangeException(nameof(frame), frame, "This projectile frame is not a bank-$8D composition.");
}
