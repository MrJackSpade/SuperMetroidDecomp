using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Botwoon;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>
/// The native bank-$84 lists of the boss-room PLM programs typed by #1296 (Botwoon's wall, the Kraid
/// room, Crocomire's arena and Mother Brain's fake death), for the ROM oracles that walk them beside the typed
/// programs and for the read guards that forbid them. Only verification names these lists.
/// </summary>
internal static class BossRoomPlmNativeListAddresses
{
    /// <summary>Botwoon's wall lists at $84:AB31..AB50 and $84:AB67..AB6C.</summary>
    internal static IReadOnlyList<NativeProgramEntry<BotwoonWallProgram>> BotwoonWallEntries { get; } =
    [
        new NativeProgramEntry<BotwoonWallProgram>(BotwoonWallProgram.Crumble, 0xab31),
        new NativeProgramEntry<BotwoonWallProgram>(BotwoonWallProgram.CrumbleRow, 0xab36),
        new NativeProgramEntry<BotwoonWallProgram>(BotwoonWallProgram.Clear, 0xab67),
    ];

    /// <summary>The Kraid room lists at $84:AB6D..ABD5 and $84:ABDD..ABE2, around the move-right routine.</summary>
    internal static IReadOnlyList<NativeProgramEntry<KraidRoomProgram>> KraidRoomEntries { get; } =
    [
        new NativeProgramEntry<KraidRoomProgram>(KraidRoomProgram.CrumbleCeilingIntoBackground1, 0xab6d),
        new NativeProgramEntry<KraidRoomProgram>(KraidRoomProgram.SetCeilingToBackground1, 0xab79),
        new NativeProgramEntry<KraidRoomProgram>(KraidRoomProgram.CrumbleCeilingIntoBackground2, 0xab7f),
        new NativeProgramEntry<KraidRoomProgram>(KraidRoomProgram.SetCeilingToBackground2, 0xab8b),
        new NativeProgramEntry<KraidRoomProgram>(KraidRoomProgram.CrumbleCeilingIntoBackground3, 0xab91),
        new NativeProgramEntry<KraidRoomProgram>(KraidRoomProgram.SetCeilingToBackground3, 0xab9d),
        new NativeProgramEntry<KraidRoomProgram>(KraidRoomProgram.ClearCeiling, 0xaba3),
        new NativeProgramEntry<KraidRoomProgram>(KraidRoomProgram.CrumbleSpikes, 0xaba9),
        new NativeProgramEntry<KraidRoomProgram>(KraidRoomProgram.CrumbleSpikesPass, 0xabac),
        new NativeProgramEntry<KraidRoomProgram>(KraidRoomProgram.ClearSpikes, 0xabdd),
    ];

    /// <summary>Crocomire's arena lists at $84:AFCA..AFE7, six bytes each.</summary>
    internal static IReadOnlyList<NativeProgramEntry<CrocomireArenaProgram>> CrocomireArenaEntries { get; } =
    [
        new NativeProgramEntry<CrocomireArenaProgram>(CrocomireArenaProgram.ClearBridge, 0xafca),
        new NativeProgramEntry<CrocomireArenaProgram>(CrocomireArenaProgram.CrumbleBridgeBlock, 0xafd0),
        new NativeProgramEntry<CrocomireArenaProgram>(CrocomireArenaProgram.ClearBridgeBlock, 0xafd6),
        new NativeProgramEntry<CrocomireArenaProgram>(CrocomireArenaProgram.ClearInvisibleWall, 0xafdc),
        new NativeProgramEntry<CrocomireArenaProgram>(CrocomireArenaProgram.CreateInvisibleWall, 0xafe2),
    ];

    /// <summary>Mother Brain's fake-death lists at $84:AC05..AC88, six bytes each in authored order.</summary>
    internal static IReadOnlyList<NativeProgramEntry<MotherBrainFakeDeathProgram>> MotherBrainFakeDeathEntries { get; } =
        [.. Enum.GetValues<MotherBrainFakeDeathProgram>().Select((program, index) => new NativeProgramEntry<MotherBrainFakeDeathProgram>(program, (ushort)(0xac05 + 6 * index)))];

    /// <summary>Whether a bank-$84 offset lies in one of these typed families' native lists or routines.</summary>
    internal static bool IsTypedProgramByte(int offset) =>
        offset is >= 0xab31 and < 0xabe3 or >= 0xac05 and < 0xac89 or >= 0xafca and < 0xafe8;

    /// <summary>Spore Spawn's ceiling lists (<c>InstList_PLM_CrumbleSporeSpawnCeiling</c>, <c>..ClearSporeSpawnCeiling</c>).</summary>
    internal static IReadOnlyList<NativeProgramEntry<SporeSpawnCeilingProgram>> SporeSpawnCeilingEntries { get; } =
    [
        new NativeProgramEntry<SporeSpawnCeilingProgram>(SporeSpawnCeilingProgram.Crumble, 0xab12),
        new NativeProgramEntry<SporeSpawnCeilingProgram>(SporeSpawnCeilingProgram.Clear, 0xab21),
    ];

    /// <summary>Draygon's cannon lists, right then left (<c>InstList_PLM_DraygonCannon..FacingRight/Left_0..2</c>).</summary>
    internal static IReadOnlyList<NativeProgramEntry<DraygonCannonProgram>> DraygonCannonEntries { get; } =
    [
        new NativeProgramEntry<DraygonCannonProgram>(DraygonCannonProgram.RightStart, 0xdcde),
        new NativeProgramEntry<DraygonCannonProgram>(DraygonCannonProgram.RightIdle, 0xdce6),
        new NativeProgramEntry<DraygonCannonProgram>(DraygonCannonProgram.RightHit, 0xdcf0),
        new NativeProgramEntry<DraygonCannonProgram>(DraygonCannonProgram.RightDestroy, 0xdd11),
        new NativeProgramEntry<DraygonCannonProgram>(DraygonCannonProgram.RightSmoke, 0xdd13),
        new NativeProgramEntry<DraygonCannonProgram>(DraygonCannonProgram.LeftStart, 0xddb9),
        new NativeProgramEntry<DraygonCannonProgram>(DraygonCannonProgram.LeftIdle, 0xddc1),
        new NativeProgramEntry<DraygonCannonProgram>(DraygonCannonProgram.LeftHit, 0xddcb),
        new NativeProgramEntry<DraygonCannonProgram>(DraygonCannonProgram.LeftDestroy, 0xddec),
        new NativeProgramEntry<DraygonCannonProgram>(DraygonCannonProgram.LeftSmoke, 0xddee),
    ];

    /// <summary>Mother Brain's glass lists (<c>InstList_PLM_MotherBrainsGlass_0..1</c>, the stages after, and the unused resolved states).</summary>
    internal static IReadOnlyList<NativeProgramEntry<MotherBrainGlassPlmProgram>> MotherBrainGlassEntries { get; } =
    [
        new NativeProgramEntry<MotherBrainGlassPlmProgram>(MotherBrainGlassPlmProgram.Entry, 0xd202),
        new NativeProgramEntry<MotherBrainGlassPlmProgram>(MotherBrainGlassPlmProgram.Stage0, 0xd211),
        new NativeProgramEntry<MotherBrainGlassPlmProgram>(MotherBrainGlassPlmProgram.Stage1, 0xd21b),
        new NativeProgramEntry<MotherBrainGlassPlmProgram>(MotherBrainGlassPlmProgram.Stage2, 0xd225),
        new NativeProgramEntry<MotherBrainGlassPlmProgram>(MotherBrainGlassPlmProgram.Stage3, 0xd247),
        new NativeProgramEntry<MotherBrainGlassPlmProgram>(MotherBrainGlassPlmProgram.Stage4, 0xd251),
        new NativeProgramEntry<MotherBrainGlassPlmProgram>(MotherBrainGlassPlmProgram.Stage5, 0xd25b),
        new NativeProgramEntry<MotherBrainGlassPlmProgram>(MotherBrainGlassPlmProgram.Stage6, 0xd27d),
        new NativeProgramEntry<MotherBrainGlassPlmProgram>(MotherBrainGlassPlmProgram.Stage7, 0xd29f),
        new NativeProgramEntry<MotherBrainGlassPlmProgram>(MotherBrainGlassPlmProgram.Stage8, 0xd2c1),
        new NativeProgramEntry<MotherBrainGlassPlmProgram>(MotherBrainGlassPlmProgram.BossDead, 0xd2ed),
        new NativeProgramEntry<MotherBrainGlassPlmProgram>(MotherBrainGlassPlmProgram.NoGlass, 0xd2f3),
    ];

    /// <summary>The Bomb Torizo hand's list (<c>InstList_PLM_BombTorizosCrumblingChozo</c>) and its continuation after the sleep.</summary>
    internal static IReadOnlyList<NativeProgramEntry<BombTorizoHandProgram>> BombTorizoHandEntries { get; } =
    [
        new NativeProgramEntry<BombTorizoHandProgram>(BombTorizoHandProgram.Sleeping, 0xd368),
        new NativeProgramEntry<BombTorizoHandProgram>(BombTorizoHandProgram.Crumble, 0xd372),
    ];
}
