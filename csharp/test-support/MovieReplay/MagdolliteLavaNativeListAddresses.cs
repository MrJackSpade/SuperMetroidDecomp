using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.Core.Enemies.Magdollite;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the Magdollite lava ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class MagdolliteLavaNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<MagdolliteLavaSegment>[] Entries =
    [
        new NativeProgramEntry<MagdolliteLavaSegment>(MagdolliteLavaSegment.SharedDelete, 0x84fc),
        new NativeProgramEntry<MagdolliteLavaSegment>(MagdolliteLavaSegment.Left, 0xdfd8),
        new NativeProgramEntry<MagdolliteLavaSegment>(MagdolliteLavaSegment.Right, 0xdfde),
        new NativeProgramEntry<MagdolliteLavaSegment>(MagdolliteLavaSegment.Shot, 0xdfe4),
    ];

    /// <summary>The bank-$86 bytes the lists occupy, $86:DFD8-DFE9, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xdfd8, 0xdfea);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(MagdolliteLavaSegment program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, MagdolliteLavaAnimations.Segments, MagdolliteLavaAnimations.StartingAt, (frame => EnemyProjectileProgramFrameSources.Operand(frame)), MagdolliteLavaCueRules.Rules);
}
