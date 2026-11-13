using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Powamp.Artwork;
using SuperMetroid.Core.Enemies.Powamp;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the Powamp spike ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class PowampSpikeNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<PowampSpikeProgram>[] Entries =
    [
        new NativeProgramEntry<PowampSpikeProgram>(PowampSpikeProgram.Initial, 0xd208),
        new NativeProgramEntry<PowampSpikeProgram>(PowampSpikeProgram.Delete, 0xd218),
    ];

    /// <summary>The bank-$86 bytes the lists occupy, $86:D208-D219, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xd208, 0xd21a);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, PowampSpikeAnimations.Segments, PowampSpikeAnimations.StartingAt, (frame => EnemyProjectileProgramFrameSources.Operand(frame)), NativeEnemyProjectileCommonCues.Rules);
}
