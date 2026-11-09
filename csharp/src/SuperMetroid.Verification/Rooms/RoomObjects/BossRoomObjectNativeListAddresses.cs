using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Botwoon;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>
/// The native bank-$84 lists of the boss-room room objects: the terrain changes of #1554 (Botwoon's wall, the Kraid
/// room, Crocomire's arena, Spore Spawn's ceiling and Mother Brain's fake death) and the programs typed by #1296, for
/// the ROM oracles that play them beside the typed changes and programs and for the read guards that forbid them. Only
/// verification names these lists.
/// </summary>
internal static class BossRoomObjectNativeListAddresses
{
    /// <summary>Botwoon's wall lists at $84:AB31..AB50 and $84:AB67..AB6C, by the mutation each starts.</summary>
    internal static IReadOnlyList<NativeProgramEntry<BotwoonWallMutation>> BotwoonWallEntries { get; } =
    [
        new NativeProgramEntry<BotwoonWallMutation>(BotwoonWallMutation.Crumble, 0xab31),
        new NativeProgramEntry<BotwoonWallMutation>(BotwoonWallMutation.Clear, 0xab67),
    ];

    /// <summary>
    /// The Kraid room lists at $84:AB6D..ABD5 and $84:ABDD..ABE2, around the move-right routine, by the mutation each
    /// starts: a platform variant starts at the background list a ceiling crumble runs on into.
    /// </summary>
    internal static IReadOnlyList<NativeProgramEntry<KraidRoomMutation>> KraidRoomEntries { get; } =
    [
        new NativeProgramEntry<KraidRoomMutation>(KraidRoomMutation.CrumbleCeilingIntoBackground1, 0xab6d),
        new NativeProgramEntry<KraidRoomMutation>(KraidRoomMutation.CrumbleCeilingIntoBackground2, 0xab7f),
        new NativeProgramEntry<KraidRoomMutation>(KraidRoomMutation.CrumblePlatformVariant1, 0xab8b),
        new NativeProgramEntry<KraidRoomMutation>(KraidRoomMutation.CrumbleCeilingIntoBackground3, 0xab91),
        new NativeProgramEntry<KraidRoomMutation>(KraidRoomMutation.CrumblePlatformVariant2, 0xab9d),
        new NativeProgramEntry<KraidRoomMutation>(KraidRoomMutation.ClearCeiling, 0xaba3),
        new NativeProgramEntry<KraidRoomMutation>(KraidRoomMutation.CrumbleSpikes, 0xaba9),
        new NativeProgramEntry<KraidRoomMutation>(KraidRoomMutation.ClearSpikes, 0xabdd),
    ];

    /// <summary>Crocomire's arena lists at $84:AFCA..AFE7, six bytes each, by the mutation each starts.</summary>
    internal static IReadOnlyList<NativeProgramEntry<CrocomireArenaMutation>> CrocomireArenaEntries { get; } =
    [
        new NativeProgramEntry<CrocomireArenaMutation>(CrocomireArenaMutation.ClearBridge, 0xafca),
        new NativeProgramEntry<CrocomireArenaMutation>(CrocomireArenaMutation.CrumbleBridgeBlock, 0xafd0),
        new NativeProgramEntry<CrocomireArenaMutation>(CrocomireArenaMutation.ClearBridgeBlock, 0xafd6),
        new NativeProgramEntry<CrocomireArenaMutation>(CrocomireArenaMutation.ClearInvisibleWall, 0xafdc),
        new NativeProgramEntry<CrocomireArenaMutation>(CrocomireArenaMutation.CreateInvisibleWall, 0xafe2),
    ];

    /// <summary>
    /// Mother Brain's fake-death lists at $84:AC05..AC88, six bytes each in authored order, by the mutation each starts.
    /// The two lists after the mural's last row draw solid rows nothing in the game spawns.
    /// </summary>
    internal static IReadOnlyList<NativeProgramEntry<MotherBrainRoomMutation>> MotherBrainFakeDeathEntries { get; } =
    [
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.FillWall, 0xac05),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.EscapeDoor, 0xac0b),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.MuralRow1, 0xac11),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.MuralRow2, 0xac17),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.MuralRow3, 0xac1d),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.MuralRow4, 0xac23),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.MuralRow5, 0xac29),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.MuralRow6, 0xac2f),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.MuralRow7, 0xac35),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.MuralRow8, 0xac3b),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.MuralRow9, 0xac41),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.MuralRow10, 0xac47),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.MuralRow11, 0xac4d),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.MuralRow12, 0xac53),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.ClearCeilingBlock, 0xac65),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.ClearCeilingTube, 0xac6b),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.ClearBottomMiddleSideTube, 0xac71),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.ClearBottomMiddleTubes, 0xac77),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.ClearBottomLeftTube, 0xac7d),
        new NativeProgramEntry<MotherBrainRoomMutation>(MotherBrainRoomMutation.ClearBottomRightTube, 0xac83),
    ];

    /// <summary>Whether a bank-$84 offset lies in one of these typed families' native lists or routines.</summary>
    internal static bool IsTypedProgramByte(int offset) =>
        offset is >= 0xab31 and < 0xabe3 or >= 0xac05 and < 0xac89 or >= 0xafca and < 0xafe8;

    /// <summary>Spore Spawn's ceiling lists (<c>InstList_PLM_CrumbleSporeSpawnCeiling</c>, <c>..ClearSporeSpawnCeiling</c>), by the mutation each starts.</summary>
    internal static IReadOnlyList<NativeProgramEntry<SporeSpawnCeilingMutation>> SporeSpawnCeilingEntries { get; } =
    [
        new NativeProgramEntry<SporeSpawnCeilingMutation>(SporeSpawnCeilingMutation.Crumble, 0xab12),
        new NativeProgramEntry<SporeSpawnCeilingMutation>(SporeSpawnCeilingMutation.Clear, 0xab21),
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
    internal static IReadOnlyList<NativeProgramEntry<MotherBrainGlassObjectProgram>> MotherBrainGlassEntries { get; } =
    [
        new NativeProgramEntry<MotherBrainGlassObjectProgram>(MotherBrainGlassObjectProgram.Entry, 0xd202),
        new NativeProgramEntry<MotherBrainGlassObjectProgram>(MotherBrainGlassObjectProgram.Stage0, 0xd211),
        new NativeProgramEntry<MotherBrainGlassObjectProgram>(MotherBrainGlassObjectProgram.Stage1, 0xd21b),
        new NativeProgramEntry<MotherBrainGlassObjectProgram>(MotherBrainGlassObjectProgram.Stage2, 0xd225),
        new NativeProgramEntry<MotherBrainGlassObjectProgram>(MotherBrainGlassObjectProgram.Stage3, 0xd247),
        new NativeProgramEntry<MotherBrainGlassObjectProgram>(MotherBrainGlassObjectProgram.Stage4, 0xd251),
        new NativeProgramEntry<MotherBrainGlassObjectProgram>(MotherBrainGlassObjectProgram.Stage5, 0xd25b),
        new NativeProgramEntry<MotherBrainGlassObjectProgram>(MotherBrainGlassObjectProgram.Stage6, 0xd27d),
        new NativeProgramEntry<MotherBrainGlassObjectProgram>(MotherBrainGlassObjectProgram.Stage7, 0xd29f),
        new NativeProgramEntry<MotherBrainGlassObjectProgram>(MotherBrainGlassObjectProgram.Stage8, 0xd2c1),
        new NativeProgramEntry<MotherBrainGlassObjectProgram>(MotherBrainGlassObjectProgram.BossDead, 0xd2ed),
        new NativeProgramEntry<MotherBrainGlassObjectProgram>(MotherBrainGlassObjectProgram.NoGlass, 0xd2f3),
    ];

    /// <summary>The Bomb Torizo hand's list (<c>InstList_PLM_BombTorizosCrumblingChozo</c>) and its continuation after the sleep.</summary>
    internal static IReadOnlyList<NativeProgramEntry<BombTorizoHandProgram>> BombTorizoHandEntries { get; } =
    [
        new NativeProgramEntry<BombTorizoHandProgram>(BombTorizoHandProgram.Sleeping, 0xd368),
        new NativeProgramEntry<BombTorizoHandProgram>(BombTorizoHandProgram.Crumble, 0xd372),
    ];
}
