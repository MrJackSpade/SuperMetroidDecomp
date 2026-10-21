using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>
/// The native bank-$86 lists the n00b tube projectile ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class NoobTubeProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<NoobTubeProjectileProgram>[] Entries =
    [
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Crack, 0xd3d7),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.CrackLoop, 0xd46f),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard0, 0xd47d),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard0Loop, 0xd481),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard0Loop2, 0xd495),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard1, 0xd4a1),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard1Loop, 0xd4a5),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard1Loop2, 0xd4b9),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard2, 0xd4c5),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard2Loop, 0xd4c9),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard2Loop2, 0xd4dd),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard3, 0xd4e9),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard3Loop, 0xd4ed),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard3Loop2, 0xd501),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard4, 0xd50d),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard4Loop, 0xd511),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard4Loop2, 0xd525),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard5, 0xd531),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard5Loop, 0xd535),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard5Loop2, 0xd549),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard6, 0xd555),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard6Loop, 0xd559),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard6Loop2, 0xd56d),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard7, 0xd579),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard7Loop, 0xd57d),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard7Loop2, 0xd591),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard8, 0xd59d),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard8Loop, 0xd5a1),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard8Loop2, 0xd5b3),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard9, 0xd5bd),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard9Loop, 0xd5c1),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.Shard9Loop2, 0xd5d5),
        new NativeProgramEntry<NoobTubeProjectileProgram>(NoobTubeProjectileProgram.ReleasedAirBubble, 0xd652),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:D3D7-D699, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xd3d7, 0xd69a);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(NoobTubeProjectileProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
