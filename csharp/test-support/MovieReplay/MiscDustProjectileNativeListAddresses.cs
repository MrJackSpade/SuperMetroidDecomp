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
    internal static readonly NativeProgramEntry<MiscDustAnimation>[] Entries =
    [
        new NativeProgramEntry<MiscDustAnimation>(MiscDustAnimation.MotherBrainElbowChargeParticle, 0xe100),
        new NativeProgramEntry<MiscDustAnimation>(MiscDustAnimation.MotherBrainElbowChargeEnergy, 0xe11a),
        new NativeProgramEntry<MiscDustAnimation>(MiscDustAnimation.SmallExplosion, 0xe138),
        new NativeProgramEntry<MiscDustAnimation>(MiscDustAnimation.MotherBrainRainbowBeamExplosion, 0xe152),
        new NativeProgramEntry<MiscDustAnimation>(MiscDustAnimation.DudShotTinyExplosion, 0xe17e),
        new NativeProgramEntry<MiscDustAnimation>(MiscDustAnimation.SmallDustCloud, 0xe1b0),
        new NativeProgramEntry<MiscDustAnimation>(MiscDustAnimation.CorpseDustCloud, 0xe1c6),
        new NativeProgramEntry<MiscDustAnimation>(MiscDustAnimation.EyeDoorSmokePuff, 0xe1d8),
        new NativeProgramEntry<MiscDustAnimation>(MiscDustAnimation.Smoke, 0xe1ea),
        new NativeProgramEntry<MiscDustAnimation>(MiscDustAnimation.BigExplosion, 0xe208),
        new NativeProgramEntry<MiscDustAnimation>(MiscDustAnimation.RockParticles, 0xe266),
        new NativeProgramEntry<MiscDustAnimation>(MiscDustAnimation.ShortBigDustCloud, 0xe2a8),
        new NativeProgramEntry<MiscDustAnimation>(MiscDustAnimation.BigDustCloud, 0xe2f2),
        new NativeProgramEntry<MiscDustAnimation>(MiscDustAnimation.LongDraygonBreathBubbles, 0xe3a0),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:E0EE-E42B, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xe0ee, 0xe42c);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(MiscDustAnimation program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, MiscDustProjectileAnimations.Segments, MiscDustProjectileAnimations.StartingAt, (frame => EnemyProjectileProgramFrameSources.Operand(frame)), NativeEnemyProjectileCommonCues.Rules);
}
