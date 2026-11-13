using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Shaktool.Artwork;
using SuperMetroid.Core.Enemies.Shaktool;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the Shaktool attack circle ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class ShaktoolProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<ShaktoolProjectileSegment>[] Entries =
    [
        new NativeProgramEntry<ShaktoolProjectileSegment>(ShaktoolProjectileSegment.Front, 0xbd68),
        new NativeProgramEntry<ShaktoolProjectileSegment>(ShaktoolProjectileSegment.FrontLoop, 0xbd70),
        new NativeProgramEntry<ShaktoolProjectileSegment>(ShaktoolProjectileSegment.Middle, 0xbd78),
        new NativeProgramEntry<ShaktoolProjectileSegment>(ShaktoolProjectileSegment.MiddleLoop, 0xbd84),
        new NativeProgramEntry<ShaktoolProjectileSegment>(ShaktoolProjectileSegment.Back, 0xbd8c),
        new NativeProgramEntry<ShaktoolProjectileSegment>(ShaktoolProjectileSegment.BackLoop, 0xbd94),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:BD68-BD9B, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xbd68, 0xbd9c);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(ShaktoolProjectileSegment program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, ShaktoolProjectileAnimations.Segments, ShaktoolProjectileAnimations.StartingAt, (frame => EnemyProjectileProgramFrameSources.Operand(frame)), NativeEnemyProjectileCommonCues.Rules);
}
