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
    internal static readonly (NoobTubeProjectileProgram Program, ushort Entry)[] Entries =
    [
        (NoobTubeProjectileProgram.Crack, 0xd3d7),
        (NoobTubeProjectileProgram.CrackLoop, 0xd46f),
        (NoobTubeProjectileProgram.Shard0, 0xd47d),
        (NoobTubeProjectileProgram.Shard0Loop, 0xd481),
        (NoobTubeProjectileProgram.Shard0Loop2, 0xd495),
        (NoobTubeProjectileProgram.Shard1, 0xd4a1),
        (NoobTubeProjectileProgram.Shard1Loop, 0xd4a5),
        (NoobTubeProjectileProgram.Shard1Loop2, 0xd4b9),
        (NoobTubeProjectileProgram.Shard2, 0xd4c5),
        (NoobTubeProjectileProgram.Shard2Loop, 0xd4c9),
        (NoobTubeProjectileProgram.Shard2Loop2, 0xd4dd),
        (NoobTubeProjectileProgram.Shard3, 0xd4e9),
        (NoobTubeProjectileProgram.Shard3Loop, 0xd4ed),
        (NoobTubeProjectileProgram.Shard3Loop2, 0xd501),
        (NoobTubeProjectileProgram.Shard4, 0xd50d),
        (NoobTubeProjectileProgram.Shard4Loop, 0xd511),
        (NoobTubeProjectileProgram.Shard4Loop2, 0xd525),
        (NoobTubeProjectileProgram.Shard5, 0xd531),
        (NoobTubeProjectileProgram.Shard5Loop, 0xd535),
        (NoobTubeProjectileProgram.Shard5Loop2, 0xd549),
        (NoobTubeProjectileProgram.Shard6, 0xd555),
        (NoobTubeProjectileProgram.Shard6Loop, 0xd559),
        (NoobTubeProjectileProgram.Shard6Loop2, 0xd56d),
        (NoobTubeProjectileProgram.Shard7, 0xd579),
        (NoobTubeProjectileProgram.Shard7Loop, 0xd57d),
        (NoobTubeProjectileProgram.Shard7Loop2, 0xd591),
        (NoobTubeProjectileProgram.Shard8, 0xd59d),
        (NoobTubeProjectileProgram.Shard8Loop, 0xd5a1),
        (NoobTubeProjectileProgram.Shard8Loop2, 0xd5b3),
        (NoobTubeProjectileProgram.Shard9, 0xd5bd),
        (NoobTubeProjectileProgram.Shard9Loop, 0xd5c1),
        (NoobTubeProjectileProgram.Shard9Loop2, 0xd5d5),
        (NoobTubeProjectileProgram.ReleasedAirBubble, 0xd652),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:D3D7-D699, which production must never read.</summary>
    internal static readonly (ushort First, ushort End) Extent = (0xd3d7, 0xd69a);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(NoobTubeProjectileProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
