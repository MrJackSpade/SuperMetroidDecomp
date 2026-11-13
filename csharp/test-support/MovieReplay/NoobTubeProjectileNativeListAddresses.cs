using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.NoobTube.Artwork;
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
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.CrackLoop, 0xd46f),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard0, 0xd47d),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard0Loop, 0xd481),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard0Loop2, 0xd495),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard1, 0xd4a1),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard1Loop, 0xd4a5),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard1Loop2, 0xd4b9),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard2, 0xd4c5),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard2Loop, 0xd4c9),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard2Loop2, 0xd4dd),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard3, 0xd4e9),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard3Loop, 0xd4ed),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard3Loop2, 0xd501),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard4, 0xd50d),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard4Loop, 0xd511),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard4Loop2, 0xd525),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard5, 0xd531),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard5Loop, 0xd535),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard5Loop2, 0xd549),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard6, 0xd555),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard6Loop, 0xd559),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard6Loop2, 0xd56d),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard7, 0xd579),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard7Loop, 0xd57d),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard7Loop2, 0xd591),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard8, 0xd59d),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard8Loop, 0xd5a1),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard8Loop2, 0xd5b3),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard9, 0xd5bd),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard9Loop, 0xd5c1),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.Shard9Loop2, 0xd5d5),
        new NativeProgramEntry<NoobTubeProjectileSegment>(NoobTubeProjectileSegment.ReleasedAirBubble, 0xd652),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:D3D7-D699, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xd3d7, 0xd69a);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(NoobTubeProjectileSegment program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, NoobTubeProjectileAnimations.Segments, NoobTubeProjectileAnimations.StartingAt, frame => EnemyProjectileProgramFrameSources.Operand(NoobTubeProjectileFrameDefinitions.ShownProjectileFrames[new(frame.Pose)]), NoobTubeProjectileCueRules.Rules, reflectedPresentationKey: frame => frame.ReflectedPose is int pose ? EnemyProjectileProgramFrameSources.Operand(NoobTubeProjectileFrameDefinitions.ShownProjectileFrames[new(pose)]) : null);
}
