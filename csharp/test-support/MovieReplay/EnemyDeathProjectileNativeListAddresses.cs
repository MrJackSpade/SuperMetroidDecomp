using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the enemy death and drop ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class EnemyDeathProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<EnemyDeathAndDropSegment>[] Entries =
    [
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.RespawnTail, 0xeca3),
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.BigExplosion, 0xecaf),
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.BigExplosionEnd, 0xecc1),
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.FakeKraidExplosion, 0xecc9),
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.FakeKraidExplosionEnd, 0xecdf),
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.NormalExplosion, 0xed4b),
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.SmallExplosion, 0xed69),
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.SmallEnergy, 0xed8d),
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.BigEnergy, 0xeda3),
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.Missiles, 0xedb9),
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.SuperMissiles, 0xeddd),
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.PowerBombs, 0xedeb),
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.KilledBySamusContact, 0xedff),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:ECA3-EE42, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xeca3, 0xee43);

    /// <summary>
    /// The native entries the big and Fake Kraid explosions start at (#2061): each loads its bursts' count and runs on into
    /// the burst's list.
    /// </summary>
    internal static IReadOnlyList<NativeCountedEntry<EnemyDeathAndDropSegment>> CountedExplosions { get; } =
    [
        new NativeCountedEntry<EnemyDeathAndDropSegment>("the big explosion", EnemyDeathAndDropAnimations.Explosions[EnemyDeathExplosion.Big], 0xecab),
        new NativeCountedEntry<EnemyDeathAndDropSegment>("Fake Kraid's explosion", EnemyDeathAndDropAnimations.Explosions[EnemyDeathExplosion.FakeKraid], 0xecc5),
    ];

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(EnemyDeathAndDropSegment program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);

    /// <summary>The native entry <paramref name="animation"/> starts at: its counted entry, or the entry of the segment it is keyed by.</summary>
    internal static ushort Entry(Animation<EnemyDeathAndDropSegment> animation) =>
        CountedExplosions.SingleOrDefault(counted => ReferenceEquals(counted.Animation, animation)) is { } counted
            ? counted.Address
            : Entry(EnemyDeathAndDropAnimations.StartingAt.Single(pair => ReferenceEquals(pair.Value, animation)).Key);

    /// <summary>The family's typed programs beside their native lists; the big and Fake Kraid explosions start at their counted entries.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, EnemyDeathAndDropAnimations.Segments, EnemyDeathAndDropAnimations.StartingAt, EnemyProjectileProgramFrameOperands.KeyOf, EnemyDeathProjectileCueRules.Rules,
        countedEntries: CountedExplosions,
        onlyRunOnInto: new HashSet<EnemyDeathAndDropSegment> { EnemyDeathAndDropSegment.BigExplosionEnd, EnemyDeathAndDropSegment.FakeKraidExplosionEnd });
}
