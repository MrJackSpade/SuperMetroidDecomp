using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>
/// The native bank-$84 lists of the boss-room PLM programs typed by #1296 (Botwoon's wall, the Kraid
/// room, Crocomire's arena and Mother Brain's fake death), for the ROM oracles that walk them beside the typed
/// programs and for the read guards that forbid them. Only verification names these lists.
/// </summary>
internal static class BossRoomPlmNativeListAddresses
{
    /// <summary>Botwoon's wall lists at $84:AB31..AB50 and $84:AB67..AB6C.</summary>
    internal static IReadOnlyList<(BotwoonWallProgram Program, ushort Entry)> BotwoonWallEntries { get; } =
    [
        (BotwoonWallProgram.Crumble, 0xab31),
        (BotwoonWallProgram.CrumbleRow, 0xab36),
        (BotwoonWallProgram.Clear, 0xab67),
    ];

    /// <summary>The Kraid room lists at $84:AB6D..ABD5 and $84:ABDD..ABE2, around the move-right routine.</summary>
    internal static IReadOnlyList<(KraidRoomProgram Program, ushort Entry)> KraidRoomEntries { get; } =
    [
        (KraidRoomProgram.CrumbleCeilingIntoBackground1, 0xab6d),
        (KraidRoomProgram.SetCeilingToBackground1, 0xab79),
        (KraidRoomProgram.CrumbleCeilingIntoBackground2, 0xab7f),
        (KraidRoomProgram.SetCeilingToBackground2, 0xab8b),
        (KraidRoomProgram.CrumbleCeilingIntoBackground3, 0xab91),
        (KraidRoomProgram.SetCeilingToBackground3, 0xab9d),
        (KraidRoomProgram.ClearCeiling, 0xaba3),
        (KraidRoomProgram.CrumbleSpikes, 0xaba9),
        (KraidRoomProgram.CrumbleSpikesPass, 0xabac),
        (KraidRoomProgram.ClearSpikes, 0xabdd),
    ];

    /// <summary>Crocomire's arena lists at $84:AFCA..AFE7, six bytes each.</summary>
    internal static IReadOnlyList<(CrocomireArenaProgram Program, ushort Entry)> CrocomireArenaEntries { get; } =
    [
        (CrocomireArenaProgram.ClearBridge, 0xafca),
        (CrocomireArenaProgram.CrumbleBridgeBlock, 0xafd0),
        (CrocomireArenaProgram.ClearBridgeBlock, 0xafd6),
        (CrocomireArenaProgram.ClearInvisibleWall, 0xafdc),
        (CrocomireArenaProgram.CreateInvisibleWall, 0xafe2),
    ];

    /// <summary>Mother Brain's fake-death lists at $84:AC05..AC88, six bytes each in authored order.</summary>
    internal static IReadOnlyList<(MotherBrainFakeDeathProgram Program, ushort Entry)> MotherBrainFakeDeathEntries { get; } =
        [.. Enum.GetValues<MotherBrainFakeDeathProgram>().Select((program, index) => (program, (ushort)(0xac05 + 6 * index)))];

    /// <summary>Whether a bank-$84 offset lies in one of these typed families' native lists or routines.</summary>
    internal static bool IsTypedProgramByte(int offset) =>
        offset is >= 0xab31 and < 0xabe3 or >= 0xac05 and < 0xac89 or >= 0xafca and < 0xafe8;

    /// <summary>Spore Spawn's ceiling lists (<c>InstList_PLM_CrumbleSporeSpawnCeiling</c>, <c>..ClearSporeSpawnCeiling</c>).</summary>
    internal static IReadOnlyList<(SporeSpawnCeilingProgram Program, ushort Entry)> SporeSpawnCeilingEntries { get; } =
    [
        (SporeSpawnCeilingProgram.Crumble, 0xab12),
        (SporeSpawnCeilingProgram.Clear, 0xab21),
    ];

    /// <summary>Draygon's cannon lists, right then left (<c>InstList_PLM_DraygonCannon..FacingRight/Left_0..2</c>).</summary>
    internal static IReadOnlyList<(DraygonCannonProgram Program, ushort Entry)> DraygonCannonEntries { get; } =
    [
        (DraygonCannonProgram.RightStart, 0xdcde),
        (DraygonCannonProgram.RightIdle, 0xdce6),
        (DraygonCannonProgram.RightHit, 0xdcf0),
        (DraygonCannonProgram.RightDestroy, 0xdd11),
        (DraygonCannonProgram.RightSmoke, 0xdd13),
        (DraygonCannonProgram.LeftStart, 0xddb9),
        (DraygonCannonProgram.LeftIdle, 0xddc1),
        (DraygonCannonProgram.LeftHit, 0xddcb),
        (DraygonCannonProgram.LeftDestroy, 0xddec),
        (DraygonCannonProgram.LeftSmoke, 0xddee),
    ];

    /// <summary>Mother Brain's glass lists (<c>InstList_PLM_MotherBrainsGlass_0..1</c>, the stages after, and the unused resolved states).</summary>
    internal static IReadOnlyList<(MotherBrainGlassPlmProgram Program, ushort Entry)> MotherBrainGlassEntries { get; } =
    [
        (MotherBrainGlassPlmProgram.Entry, 0xd202),
        (MotherBrainGlassPlmProgram.Stage0, 0xd211),
        (MotherBrainGlassPlmProgram.Stage1, 0xd21b),
        (MotherBrainGlassPlmProgram.Stage2, 0xd225),
        (MotherBrainGlassPlmProgram.Stage3, 0xd247),
        (MotherBrainGlassPlmProgram.Stage4, 0xd251),
        (MotherBrainGlassPlmProgram.Stage5, 0xd25b),
        (MotherBrainGlassPlmProgram.Stage6, 0xd27d),
        (MotherBrainGlassPlmProgram.Stage7, 0xd29f),
        (MotherBrainGlassPlmProgram.Stage8, 0xd2c1),
        (MotherBrainGlassPlmProgram.BossDead, 0xd2ed),
        (MotherBrainGlassPlmProgram.NoGlass, 0xd2f3),
    ];

    /// <summary>The Bomb Torizo hand's list (<c>InstList_PLM_BombTorizosCrumblingChozo</c>) and its continuation after the sleep.</summary>
    internal static IReadOnlyList<(BombTorizoHandProgram Program, ushort Entry)> BombTorizoHandEntries { get; } =
    [
        (BombTorizoHandProgram.Sleeping, 0xd368),
        (BombTorizoHandProgram.Crumble, 0xd372),
    ];
}
