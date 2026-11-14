using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Kago.Artwork;
using SuperMetroid.Core.Enemies.Kago;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the Kago bug ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class KagoBugProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<KagoBugProjectileSegment>[] Entries =
    [
        new NativeProgramEntry<KagoBugProjectileSegment>(KagoBugProjectileSegment.SharedDelete, 0x84fc),
        // A new bug waits on the list Kraid's rocks share; the port gives it its own animation of that pose.
        new NativeProgramEntry<KagoBugProjectileSegment>(KagoBugProjectileSegment.Waiting, 0x9c7d),
        new NativeProgramEntry<KagoBugProjectileSegment>(KagoBugProjectileSegment.Landed, 0xd03c),
        new NativeProgramEntry<KagoBugProjectileSegment>(KagoBugProjectileSegment.Falling, 0xd04a),
        new NativeProgramEntry<KagoBugProjectileSegment>(KagoBugProjectileSegment.JumpStart, 0xd052),
        new NativeProgramEntry<KagoBugProjectileSegment>(KagoBugProjectileSegment.JumpLoop, 0xd05c),
        new NativeProgramEntry<KagoBugProjectileSegment>(KagoBugProjectileSegment.Shot, 0xd064),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:D03C-D07F, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xd03c, 0xd080);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(KagoBugProjectileSegment program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, KagoBugProjectileAnimations.Segments, KagoBugProjectileAnimations.StartingAt, (frame => EnemyProjectileProgramFrameSources.Operand(frame)), KagoBugProjectileCueRules.Rules);
}
