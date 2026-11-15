using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.NoobTube;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the n00b tube projectile ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class NoobTubeProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<NoobTubeProjectileSegment>[] Entries =
    [
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Crack, 0xd3d7),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.CrackFallen, 0xd46f),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardDownOutward, 0xd47d),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardDownOutwardFlying, 0xd481),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardDownOutwardFalling, 0xd495),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardUpOutward, 0xd4a1),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardUpOutwardFlying, 0xd4a5),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardUpOutwardFalling, 0xd4b9),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardUpSteep, 0xd4c5),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardUpSteepFlying, 0xd4c9),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardUpSteepFalling, 0xd4dd),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardUpDiagonalInner, 0xd4e9),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardUpDiagonalInnerFlying, 0xd4ed),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardUpDiagonalInnerFalling, 0xd501),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardUpDiagonalOuter, 0xd50d),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardUpDiagonalOuterFlying, 0xd511),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardUpDiagonalOuterFalling, 0xd525),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardDownDiagonal, 0xd531),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardDownDiagonalFlying, 0xd535),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardDownDiagonalFalling, 0xd549),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardDownOuter, 0xd555),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardDownOuterFlying, 0xd559),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardDownOuterFalling, 0xd56d),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardOutward, 0xd579),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardOutwardFlying, 0xd57d),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardOutwardFalling, 0xd591),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardUp, 0xd59d),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardUpFlying, 0xd5a1),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardUpFalling, 0xd5b3),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardDownInner, 0xd5bd),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardDownInnerFlying, 0xd5c1),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ShardDownInnerFalling, 0xd5d5),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ReleasedAirBubble, 0xd652),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:D3D7-D699, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xd3d7, 0xd69a);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(NoobTubeProjectileSegment program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, NoobTubeProjectileAnimations.Segments, NoobTubeProjectileAnimations.StartingAt, frame => EnemyProjectileProgramFrameSources.Operand(frame.Shown), NoobTubeProjectileCueRules.Rules, reflectedPresentationKey: frame => frame.Mirrored is { } mirrored ? EnemyProjectileProgramFrameSources.Operand(mirrored) : null);
}
