using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the enemy death and drop ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class EnemyDeathProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<EnemyDeathProjectileSegment>[] Entries =
    [
        new NativeProgramEntry<EnemyDeathProjectileSegment>(EnemyDeathProjectileSegment.RespawnTail, 0xeca3),
        new NativeProgramEntry<EnemyDeathProjectileSegment>(EnemyDeathProjectileSegment.BigExplosion, 0xecab),
        new NativeProgramEntry<EnemyDeathProjectileSegment>(EnemyDeathProjectileSegment.BigExplosionLoop, 0xecaf),
        new NativeProgramEntry<EnemyDeathProjectileSegment>(EnemyDeathProjectileSegment.MiniKraidExplosion, 0xecc5),
        new NativeProgramEntry<EnemyDeathProjectileSegment>(EnemyDeathProjectileSegment.MiniKraidExplosionLoop, 0xecc9),
        new NativeProgramEntry<EnemyDeathProjectileSegment>(EnemyDeathProjectileSegment.NormalExplosion, 0xed4b),
        new NativeProgramEntry<EnemyDeathProjectileSegment>(EnemyDeathProjectileSegment.SmallExplosion, 0xed69),
        new NativeProgramEntry<EnemyDeathProjectileSegment>(EnemyDeathProjectileSegment.SmallEnergy, 0xed8d),
        new NativeProgramEntry<EnemyDeathProjectileSegment>(EnemyDeathProjectileSegment.BigEnergy, 0xeda3),
        new NativeProgramEntry<EnemyDeathProjectileSegment>(EnemyDeathProjectileSegment.Missiles, 0xedb9),
        new NativeProgramEntry<EnemyDeathProjectileSegment>(EnemyDeathProjectileSegment.SuperMissiles, 0xeddd),
        new NativeProgramEntry<EnemyDeathProjectileSegment>(EnemyDeathProjectileSegment.PowerBombs, 0xedeb),
        new NativeProgramEntry<EnemyDeathProjectileSegment>(EnemyDeathProjectileSegment.KilledBySamusContact, 0xedff),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:ECA3-EE42, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xeca3, 0xee43);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(EnemyDeathProjectileSegment program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, EnemyDeathProjectileAnimations.Segments, EnemyDeathProjectileAnimations.StartingAt, (frame => EnemyProjectileProgramFrameSources.Operand(frame)), EnemyDeathProjectileCueRules.Rules);
}
