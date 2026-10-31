using SuperMetroid.Core;
using System.Collections.Frozen;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The bank-$8D spritemap each enemy-projectile composition frame is stored at.</summary>
[EncodingOwner("bank-$8D addresses of the enemy-projectile spritemaps")]
internal static class EnemyProjectileCompositionDefinitions
{
    /// <summary>The empty bank-$8D projectile spritemap at $8D:8000, which draws nothing.</summary>
    internal const ushort BlankSpritemap = 0x8000;

    /// <summary>
    /// The spritemap of each installed composition: <c>Spritemap_CeresElevatorPad_0</c> and <c>_1</c>,
    /// <c>Spritemap_CeresElevatorPlatform</c>, and the Skree- and Metaree-debris compositions $86:8ABF and $86:8AC7 select.
    /// </summary>
    internal static readonly FrozenDictionary<EnemyProjectileFrameId, ushort> Spritemaps = new Dictionary<EnemyProjectileFrameId, ushort>
    {
        [EnemyProjectileFrameId.CeresElevatorPad0] = 0xb1ba,
        [EnemyProjectileFrameId.CeresElevatorPad1] = 0xb1d0,
        [EnemyProjectileFrameId.CeresElevatorPlatform] = 0x846d,
        [EnemyProjectileFrameId.SkreeDebris] = 0x8023,
        [EnemyProjectileFrameId.MetareeDebris] = 0x8435,
    }.ToFrozenDictionary();
}
