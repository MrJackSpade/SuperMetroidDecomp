using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Verification.Enemies.Deaths;

/// <summary>
/// The native bank-$86 lists the enemy death and drop ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class EnemyDeathProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<EnemyDeathProjectileProgram>[] Entries =
    [
        new NativeProgramEntry<EnemyDeathProjectileProgram>(EnemyDeathProjectileProgram.RespawnTail, 0xeca3),
        new NativeProgramEntry<EnemyDeathProjectileProgram>(EnemyDeathProjectileProgram.BigExplosion, 0xecab),
        new NativeProgramEntry<EnemyDeathProjectileProgram>(EnemyDeathProjectileProgram.BigExplosionLoop, 0xecaf),
        new NativeProgramEntry<EnemyDeathProjectileProgram>(EnemyDeathProjectileProgram.MiniKraidExplosion, 0xecc5),
        new NativeProgramEntry<EnemyDeathProjectileProgram>(EnemyDeathProjectileProgram.MiniKraidExplosionLoop, 0xecc9),
        new NativeProgramEntry<EnemyDeathProjectileProgram>(EnemyDeathProjectileProgram.NormalExplosion, 0xed4b),
        new NativeProgramEntry<EnemyDeathProjectileProgram>(EnemyDeathProjectileProgram.SmallExplosion, 0xed69),
        new NativeProgramEntry<EnemyDeathProjectileProgram>(EnemyDeathProjectileProgram.SmallEnergy, 0xed8d),
        new NativeProgramEntry<EnemyDeathProjectileProgram>(EnemyDeathProjectileProgram.BigEnergy, 0xeda3),
        new NativeProgramEntry<EnemyDeathProjectileProgram>(EnemyDeathProjectileProgram.Missiles, 0xedb9),
        new NativeProgramEntry<EnemyDeathProjectileProgram>(EnemyDeathProjectileProgram.SuperMissiles, 0xeddd),
        new NativeProgramEntry<EnemyDeathProjectileProgram>(EnemyDeathProjectileProgram.PowerBombs, 0xedeb),
        new NativeProgramEntry<EnemyDeathProjectileProgram>(EnemyDeathProjectileProgram.KilledBySamusContact, 0xedff),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:ECA3-EE42, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xeca3, 0xee43);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(EnemyDeathProjectileProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
