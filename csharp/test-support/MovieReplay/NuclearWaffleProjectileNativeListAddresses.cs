using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Nuclear.Artwork;
using SuperMetroid.Core.Enemies.Nuclear;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the Nuclear Waffle body ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class NuclearWaffleProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<NuclearWaffleProjectileProgram>[] Entries =
    [
        new NativeProgramEntry<NuclearWaffleProjectileProgram>(NuclearWaffleProjectileProgram.Initial, 0xbb5e),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:BB5E-BB91, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xbb5e, 0xbb92);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(NuclearWaffleProjectileProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, NuclearWaffleProjectileAnimations.Segments, NuclearWaffleProjectileAnimations.StartingAt, (frame => EnemyProjectileProgramFrameSources.Operand(frame)), NativeEnemyProjectileCommonCues.Rules);
}
