using SuperMetroid.Core;
using System.Collections.Frozen;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The bank-$8D spritemap each enemy-projectile composition frame is stored at, and each picture of the Ceres elevator
/// arrival's pad and platform, which the cartridge composes in the same bank (#2425).
/// </summary>
[EncodingOwner("bank-$8D addresses of the enemy-projectile spritemaps")]
internal static partial class EnemyProjectileCompositionDefinitions
{
    /// <summary>The empty bank-$8D projectile spritemap at $8D:8000, which draws nothing.</summary>
    internal const ushort BlankSpritemap = 0x8000;

    /// <summary>The spritemap of each installed composition: the Skree- and Metaree-debris compositions $86:8ABF and $86:8AC7 select.</summary>
    internal static readonly FrozenDictionary<EnemyProjectileFrameId, ushort> Spritemaps = new Dictionary<EnemyProjectileFrameId, ushort>
    {
        [EnemyProjectileFrameId.SkreeDebris] = 0x8023,
        [EnemyProjectileFrameId.MetareeDebris] = 0x8435,
    }.ToFrozenDictionary();

    /// <summary>
    /// The spritemap of each pose's picture of the Ceres elevator arrival: <c>Spritemap_CeresElevatorPad_0</c> and
    /// <c>_1</c>, and <c>Spritemap_CeresElevatorPlatform</c>.
    /// </summary>
    internal static readonly FrozenDictionary<CeresElevatorArrivalPose, ushort> ElevatorArrivalSpritemaps = new Dictionary<CeresElevatorArrivalPose, ushort>
    {
        [CeresElevatorArrivalPose.PadUnlit] = 0xb1ba,
        [CeresElevatorArrivalPose.PadLit] = 0xb1d0,
        [CeresElevatorArrivalPose.Platform] = 0x846d,
    }.ToFrozenDictionary();
}
