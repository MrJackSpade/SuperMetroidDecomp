using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Phantoon;

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

    /// <summary>Phantoon's door lists at $84:B77B..B784 and $84:B785..B78E, by the change each starts (#1678).</summary>
    internal static IReadOnlyList<NativeProgramEntry<PhantoonDoorChange>> PhantoonDoorEntries { get; } =
    [
        new NativeProgramEntry<PhantoonDoorChange>(PhantoonDoorChange.FightDoor, 0xb77b),
        new NativeProgramEntry<PhantoonDoorChange>(PhantoonDoorChange.RestoredDoor, 0xb785),
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

}
