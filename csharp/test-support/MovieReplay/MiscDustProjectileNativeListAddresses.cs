using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the misc dust ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class MiscDustProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<DustExplosionAnimation>[] Entries =
    [
        new NativeProgramEntry<DustExplosionAnimation>(DustExplosionAnimation.MotherBrainElbowChargeParticle, 0xe100),
        new NativeProgramEntry<DustExplosionAnimation>(DustExplosionAnimation.MotherBrainElbowChargeEnergy, 0xe11a),
        new NativeProgramEntry<DustExplosionAnimation>(DustExplosionAnimation.SmallExplosion, 0xe138),
        new NativeProgramEntry<DustExplosionAnimation>(DustExplosionAnimation.MotherBrainRainbowBeamExplosion, 0xe152),
        new NativeProgramEntry<DustExplosionAnimation>(DustExplosionAnimation.DudShotTinyExplosion, 0xe17e),
        new NativeProgramEntry<DustExplosionAnimation>(DustExplosionAnimation.SmallDustCloud, 0xe1b0),
        new NativeProgramEntry<DustExplosionAnimation>(DustExplosionAnimation.CorpseDustCloud, 0xe1c6),
        new NativeProgramEntry<DustExplosionAnimation>(DustExplosionAnimation.EyeDoorSmokePuff, 0xe1d8),
        new NativeProgramEntry<DustExplosionAnimation>(DustExplosionAnimation.Smoke, 0xe1ea),
        new NativeProgramEntry<DustExplosionAnimation>(DustExplosionAnimation.BigExplosion, 0xe208),
        new NativeProgramEntry<DustExplosionAnimation>(DustExplosionAnimation.RockParticles, 0xe266),
        new NativeProgramEntry<DustExplosionAnimation>(DustExplosionAnimation.ShortBigDustCloud, 0xe2a8),
        new NativeProgramEntry<DustExplosionAnimation>(DustExplosionAnimation.BigDustCloud, 0xe2f2),
        new NativeProgramEntry<DustExplosionAnimation>(DustExplosionAnimation.LongDraygonBreathBubbles, 0xe3a0),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:E0EE-E42B, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xe0ee, 0xe42c);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(DustExplosionAnimation program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, DustExplosionProjectileAnimations.Segments, DustExplosionProjectileAnimations.StartingAt, (frame => EnemyProjectileProgramFrameSources.Operand(frame)), NativeEnemyProjectileCommonCues.Rules);
}
