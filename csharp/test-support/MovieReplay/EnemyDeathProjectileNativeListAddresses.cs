using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the enemy death explosion and drop ROM oracle (#641) walks: each program's entry point, the
/// explosions' and the drops' families apart (#2554). Only verification reads them; the port runs typed programs.
/// </summary>
internal static class EnemyDeathProjectileNativeListAddresses
{
    /// <summary>Each typed explosion program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<EnemyDeathExplosionSegment>[] ExplosionEntries =
    [
        new NativeProgramEntry<EnemyDeathExplosionSegment>(EnemyDeathExplosionSegment.BigExplosion, 0xecaf),
        new NativeProgramEntry<EnemyDeathExplosionSegment>(EnemyDeathExplosionSegment.BigExplosionEnd, 0xecc1),
        new NativeProgramEntry<EnemyDeathExplosionSegment>(EnemyDeathExplosionSegment.FakeKraidExplosion, 0xecc9),
        new NativeProgramEntry<EnemyDeathExplosionSegment>(EnemyDeathExplosionSegment.FakeKraidExplosionEnd, 0xecdf),
        new NativeProgramEntry<EnemyDeathExplosionSegment>(EnemyDeathExplosionSegment.NormalExplosion, 0xed4b),
        new NativeProgramEntry<EnemyDeathExplosionSegment>(EnemyDeathExplosionSegment.SmallExplosion, 0xed69),
        new NativeProgramEntry<EnemyDeathExplosionSegment>(EnemyDeathExplosionSegment.KilledBySamusContact, 0xedff),
    ];

    /// <summary>Each typed drop program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<EnemyDeathAndDropSegment>[] DropEntries =
    [
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.RespawnPause, 0xeca3),
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.SmallEnergy, 0xed8d),
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.BigEnergy, 0xeda3),
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.Missiles, 0xedb9),
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.SuperMissiles, 0xeddd),
        new NativeProgramEntry<EnemyDeathAndDropSegment>(EnemyDeathAndDropSegment.PowerBombs, 0xedeb),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:ECA3-EE42, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xeca3, 0xee43);

    /// <summary>
    /// The native entries the big and Fake Kraid explosions start at (#2061): each loads its bursts' count and runs on into
    /// the burst's list.
    /// </summary>
    internal static IReadOnlyList<NativeCountedEntry<EnemyDeathExplosionSegment>> CountedExplosions { get; } =
    [
        new NativeCountedEntry<EnemyDeathExplosionSegment>("the big explosion", EnemyDeathExplosionAnimations.Explosions[EnemyDeathExplosion.Big], 0xecab),
        new NativeCountedEntry<EnemyDeathExplosionSegment>("Fake Kraid's explosion", EnemyDeathExplosionAnimations.Explosions[EnemyDeathExplosion.FakeKraid], 0xecc5),
    ];

    /// <summary>The native entry of the drop program <paramref name="program"/>.</summary>
    internal static ushort Entry(EnemyDeathAndDropSegment program) => NativeEnemyProjectileProgramOracle.EntryOf(DropEntries, program);

    /// <summary>The native entry the drop animation <paramref name="animation"/> starts at: the entry of the segment it is keyed by.</summary>
    internal static ushort Entry(Animation<EnemyDeathAndDropSegment> animation) =>
        Entry(EnemyDeathAndDropAnimations.StartingAt.Single(pair => ReferenceEquals(pair.Value, animation)).Key);

    /// <summary>The native entry the explosion animation <paramref name="animation"/> starts at: its counted entry, or the entry of the segment it is keyed by.</summary>
    internal static ushort Entry(Animation<EnemyDeathExplosionSegment> animation) =>
        CountedExplosions.SingleOrDefault(counted => ReferenceEquals(counted.Animation, animation)) is { } counted
            ? counted.Address
            : NativeEnemyProjectileProgramOracle.EntryOf(ExplosionEntries,
                EnemyDeathExplosionAnimations.StartingAt.Single(pair => ReferenceEquals(pair.Value, animation)).Key);

    /// <summary>The explosions' typed programs beside their native lists; the big and Fake Kraid explosions start at their counted entries.</summary>
    internal static NativeEnemyProjectileFamily ExplosionFamily { get; } = NativeEnemyProjectileFamily.Of(ExplosionEntries,
        EnemyDeathExplosionAnimations.Segments,
        new Dictionary<EnemyDeathExplosionSegment, Animation<EnemyDeathExplosionSegment>>
        {
            [EnemyDeathExplosionSegment.SmallExplosion] = EnemyDeathExplosionAnimations.StartingAt[EnemyDeathExplosionSegment.SmallExplosion],
            [EnemyDeathExplosionSegment.NormalExplosion] = EnemyDeathExplosionAnimations.StartingAt[EnemyDeathExplosionSegment.NormalExplosion],
            [EnemyDeathExplosionSegment.KilledBySamusContact] = EnemyDeathExplosionAnimations.StartingAt[EnemyDeathExplosionSegment.KilledBySamusContact],
        },
        EnemyProjectileProgramFrameOperands.KeyOf, EnemyDeathProjectileCueRules.ExplosionRules,
        countedEntries: CountedExplosions,
        onlyRunOnInto: new HashSet<EnemyDeathExplosionSegment>
        {
            EnemyDeathExplosionSegment.BigExplosionEnd, EnemyDeathExplosionSegment.FakeKraidExplosionEnd,
        });

    /// <summary>The drops' typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily DropFamily { get; } = NativeEnemyProjectileFamily.Of(DropEntries, EnemyDeathAndDropAnimations.Segments,
        EnemyDeathAndDropAnimations.StartingAt, EnemyProjectileProgramFrameOperands.KeyOf, EnemyDeathProjectileCueRules.DropRules);
}
