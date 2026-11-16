using System.Collections.Frozen;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Cartridge values and helpers of <see cref="EnemyProjectileProgramFrameDefinitions"/> that only verification reads.</summary>
internal static class EnemyProjectileProgramFrameDefinitionsConstants
{
    /// <summary>
    /// The operands that show an installed program frame's picture again (#2210, #2249), with the frame each shows: the hand
    /// beam's second and third stages draw its first stage's seven pictures from operands of their own, and Phantoon's flame
    /// lists repeat the poses another of his lists shows, his resting flame's last pose a copy of its resting pose. The import
    /// composes each frame once from <see cref="EnemyProjectileProgramFrameDefinitions.Operands"/>; tools reading native lists find its frame here.
    /// </summary>
    internal static readonly FrozenDictionary<ushort, EnemyProjectileFrameId> AlsoStoredAt = new Dictionary<ushort, EnemyProjectileFrameId>
    {
        [0xc7b9] = EnemyProjectileFrameId.MotherBrainHandBeamOrbSmall,
        [0xc7c2] = EnemyProjectileFrameId.MotherBrainHandBeamOrbRed,
        [0xc7c6] = EnemyProjectileFrameId.MotherBrainHandBeamOrbRedYellowCore,
        [0xc7ca] = EnemyProjectileFrameId.MotherBrainHandBeamOrbOrange,
        [0xc7ce] = EnemyProjectileFrameId.MotherBrainHandBeamOrbWhiteCore,
        [0xc7d2] = EnemyProjectileFrameId.MotherBrainHandBeamOrbYellow,
        [0xc7d6] = EnemyProjectileFrameId.MotherBrainHandBeamOrbWhite,
        [0xc7da] = EnemyProjectileFrameId.MotherBrainHandBeamOrbSmall,
        [0xc7e3] = EnemyProjectileFrameId.MotherBrainHandBeamOrbRed,
        [0xc7e7] = EnemyProjectileFrameId.MotherBrainHandBeamOrbRedYellowCore,
        [0xc7eb] = EnemyProjectileFrameId.MotherBrainHandBeamOrbOrange,
        [0xc7ef] = EnemyProjectileFrameId.MotherBrainHandBeamOrbWhiteCore,
        [0xc7f3] = EnemyProjectileFrameId.MotherBrainHandBeamOrbYellow,
        [0xc7f7] = EnemyProjectileFrameId.MotherBrainHandBeamOrbWhite,
        [0x9774] = EnemyProjectileFrameId.PhantoonFlameRingedEye,
        [0x9778] = EnemyProjectileFrameId.PhantoonFlameBrightEye,
        [0x977c] = EnemyProjectileFrameId.PhantoonFlameFlickering,
        [0x9798] = EnemyProjectileFrameId.PhantoonFlameResting,
        [0x97ae] = EnemyProjectileFrameId.PhantoonFlameSplashed,
        [0x97ce] = EnemyProjectileFrameId.PhantoonFlameBlank,
        [0x97de] = EnemyProjectileFrameId.PhantoonFlameBlank,
        [0x97ea] = EnemyProjectileFrameId.PhantoonFlameKindling,
        [0x97ee] = EnemyProjectileFrameId.PhantoonFlameTeardrop,
        [0x97f2] = EnemyProjectileFrameId.PhantoonFlameSputtering,
        [0x97fc] = EnemyProjectileFrameId.PhantoonFlameDying,
        [0x9800] = EnemyProjectileFrameId.PhantoonFlameGuttering,
        [0x9804] = EnemyProjectileFrameId.PhantoonFlameWisps,
        [0x9808] = EnemyProjectileFrameId.PhantoonFlameLastWisp,
    }.ToFrozenDictionary();
}
