using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>
/// The native bank-$84 lists of the room PLM programs typed by #1296, for the ROM oracles that walk them beside the
/// typed programs. Only verification names these lists.
/// </summary>
internal static class RoomObjectNativeListAddresses
{
    /// <summary>The n00b tube's first list (<c>InstList_PLM_NoobTube_0</c>, $84:D4D4).</summary>
    internal const ushort NoobTubeList = 0xd4d4;

    /// <summary>The shared one-instruction lists (<c>InstList_PLM_Delete</c> $84:AAE3, the metroids-cleared sleep $84:DB42).</summary>
    internal static IReadOnlyList<NativeProgramEntry<RoomObjectSharedProgram>> SharedEntries { get; } =
    [
        new NativeProgramEntry<RoomObjectSharedProgram>(RoomObjectSharedProgram.Delete, 0xaae3),
        new NativeProgramEntry<RoomObjectSharedProgram>(RoomObjectSharedProgram.Sleep, 0xdb42),
    ];

    /// <summary>The Speed Booster lavaquake's list (<c>InstList_PLM_SpeedBoosterEscape</c>, $84:B88A), one wait per sleep.</summary>
    internal const ushort SpeedBoosterEscapeList = 0xb88a;

    /// <summary>The Speed Booster blocks' lists ($84:C928 and $84:C951..C9F8).</summary>
    /// <summary>Every scripted room program's native entry.</summary>
    internal static IReadOnlyList<NativeProgramEntry<ScriptedRoomObjectProgram>> ScriptedRoomEntries { get; } =
    [
        new NativeProgramEntry<ScriptedRoomObjectProgram>(ScriptedRoomObjectProgram.ElevatorPlatform, 0xafb6),
        new NativeProgramEntry<ScriptedRoomObjectProgram>(ScriptedRoomObjectProgram.MotherBrainEscapeGate, 0xbb34),
        new NativeProgramEntry<ScriptedRoomObjectProgram>(ScriptedRoomObjectProgram.MotherBrainEscapeGateClosing, 0xbb44),
    ];

    /// <summary>
    /// The lists of the room objects that left the scripted program set for typed terrain changes (#1554): the Maridia
    /// elevatube, the critters' wall breakthrough, Crateria's escape passage and the enemy-breakable block.
    /// </summary>
    internal static IReadOnlyList<NativeProgramEntry<string>> ScriptedTerrainChangeEntries { get; } =
    [
        new NativeProgramEntry<string>(nameof(ScriptedTerrainChangeDefinitions.MaridiaElevatube), 0xb8f0),
        new NativeProgramEntry<string>(nameof(ScriptedTerrainChangeDefinitions.CritterWallBreakthrough), 0xb9a2),
        new NativeProgramEntry<string>(nameof(ScriptedTerrainChangeDefinitions.CrateriaEscapePassage), 0xbb19),
        new NativeProgramEntry<string>(nameof(ScriptedTerrainChangeDefinitions.EnemyBreakableBlockBreak), 0xcd53),
    ];

    /// <summary>The native entry of block-reaction program <paramref name="program"/>.</summary>
    internal static ushort BlockReactionEntry(BlockReactionObjectProgram program) =>
        BlockReactionEntries.Single(entry => entry.Program == program).Entry;

    /// <summary>Every block-reaction program's native entry.</summary>
    internal static IReadOnlyList<NativeProgramEntry<BlockReactionObjectProgram>> BlockReactionEntries { get; } =
    [
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.CrumbleReveal1x1, 0xc8ec),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.CrumbleReveal2x1, 0xc8f2),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.CrumbleReveal1x2, 0xc8f8),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.CrumbleReveal2x2, 0xc8fe),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.BombedPowerBombBlockUnused, 0xc91c),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.BombedSuperMissileBlockUnused, 0xc922),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.ContactCrumble1x1Respawning, 0xc9f9),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.ContactCrumble2x1Respawning, 0xca1c),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.ContactCrumble1x2Respawning, 0xca41),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.ContactCrumble2x2Respawning, 0xca66),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.ContactCrumble1x1Permanent, 0xca8b),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.ContactCrumble2x1Permanent, 0xcaa0),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.ContactCrumble1x2Permanent, 0xcab5),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.ContactCrumble2x2Permanent, 0xcaca),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.RespawningShotBlock1x1, 0xcadf),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.RespawningShotBlock2x1, 0xcb02),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.RespawningShotBlock1x2, 0xcb27),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.RespawningShotBlock2x2, 0xcb4c),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.RespawningSuperMissileBlock, 0xcb71),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.RespawningPowerBombBlock, 0xcb94),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.PermanentShotBlock1x1, 0xcbb7),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.PermanentShotBlock2x1, 0xcbcc),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.PermanentShotBlock1x2, 0xcbe1),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.PermanentShotBlock2x2, 0xcbf6),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.PermanentSuperMissileBlock, 0xcc0b),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.PermanentPowerBombBlock, 0xcc20),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.CollisionBombBlock1x1Respawning, 0xcc35),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.ReactionBombBlock1x1Respawning, 0xcc3c),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.BombBlock1x1RespawningBreak, 0xcc3f),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.CollisionBombBlock2x1Respawning, 0xcc5f),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.ReactionBombBlock2x1Respawning, 0xcc66),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.BombBlock2x1RespawningBreak, 0xcc69),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.CollisionBombBlock1x2Respawning, 0xcc8b),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.ReactionBombBlock1x2Respawning, 0xcc92),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.BombBlock1x2RespawningBreak, 0xcc95),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.CollisionBombBlock2x2Respawning, 0xccb7),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.ReactionBombBlock2x2Respawning, 0xccbe),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.BombBlock2x2RespawningBreak, 0xccc1),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.CollisionBombBlock1x1Permanent, 0xcce3),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.ReactionBombBlock1x1Permanent, 0xccea),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.BombBlock1x1PermanentBreak, 0xcced),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.CollisionBombBlock2x1Permanent, 0xccff),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.ReactionBombBlock2x1Permanent, 0xcd06),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.BombBlock2x1PermanentBreak, 0xcd09),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.CollisionBombBlock1x2Permanent, 0xcd1b),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.ReactionBombBlock1x2Permanent, 0xcd22),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.BombBlock1x2PermanentBreak, 0xcd25),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.CollisionBombBlock2x2Permanent, 0xcd37),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.ReactionBombBlock2x2Permanent, 0xcd3e),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.BombBlock2x2PermanentBreak, 0xcd41),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.RespawningBreakableGrappleBlock, 0xcd6a),
        new NativeProgramEntry<BlockReactionObjectProgram>(BlockReactionObjectProgram.PermanentBreakableGrappleBlock, 0xcda9),
    ];

    internal static IReadOnlyList<NativeProgramEntry<SpeedBoosterBlockProgram>> SpeedBoosterBlockEntries { get; } =
    [
        new NativeProgramEntry<SpeedBoosterBlockProgram>(SpeedBoosterBlockProgram.BrinstarSlowRespawning, 0xc951),
        new NativeProgramEntry<SpeedBoosterBlockProgram>(SpeedBoosterBlockProgram.Respawning, 0xc974),
        new NativeProgramEntry<SpeedBoosterBlockProgram>(SpeedBoosterBlockProgram.DachoraRespawning, 0xc997),
        new NativeProgramEntry<SpeedBoosterBlockProgram>(SpeedBoosterBlockProgram.BrinstarSlowPermanent, 0xc9cf),
        new NativeProgramEntry<SpeedBoosterBlockProgram>(SpeedBoosterBlockProgram.Permanent, 0xc9e4),
        new NativeProgramEntry<SpeedBoosterBlockProgram>(SpeedBoosterBlockProgram.BombReveal, 0xc928),
    ];

    /// <summary>
    /// The downward gate's lists: the open and closed gates ($84:BC13, $84:BC3A), the continuations after their sleeps
    /// ($84:BC1F, $84:BC46), and the eight trigger lists from $84:BCAF, six bytes each.
    /// </summary>
    internal static IReadOnlyList<NativeProgramEntry<DownwardGateProgram>> DownwardGateEntries { get; } =
    [
        new NativeProgramEntry<DownwardGateProgram>(DownwardGateProgram.Open, 0xbc13),
        new NativeProgramEntry<DownwardGateProgram>(DownwardGateProgram.Close, 0xbc1f),
        new NativeProgramEntry<DownwardGateProgram>(DownwardGateProgram.Closed, 0xbc3a),
        new NativeProgramEntry<DownwardGateProgram>(DownwardGateProgram.Reopen, 0xbc46),
        new NativeProgramEntry<DownwardGateProgram>(DownwardGateProgram.TriggerBlueLeft, 0xbcaf),
        new NativeProgramEntry<DownwardGateProgram>(DownwardGateProgram.TriggerBlueRight, 0xbcb5),
        new NativeProgramEntry<DownwardGateProgram>(DownwardGateProgram.TriggerRedLeft, 0xbcbb),
        new NativeProgramEntry<DownwardGateProgram>(DownwardGateProgram.TriggerRedRight, 0xbcc1),
        new NativeProgramEntry<DownwardGateProgram>(DownwardGateProgram.TriggerGreenLeft, 0xbcc7),
        new NativeProgramEntry<DownwardGateProgram>(DownwardGateProgram.TriggerGreenRight, 0xbccd),
        new NativeProgramEntry<DownwardGateProgram>(DownwardGateProgram.TriggerYellowLeft, 0xbcd3),
        new NativeProgramEntry<DownwardGateProgram>(DownwardGateProgram.TriggerYellowRight, 0xbcd9),
    ];

    /// <summary>The bytes from a sleeping gate list's entry to the continuation after its sleep.</summary>
    internal const int DownwardGateSleepContinuationOffset = 12;

    /// <summary>The Tourian elevator access lists (<c>InstList_PLM_CrumbleAccessToTourianElevator_0..1</c>, <c>..ClearAccess..</c>).</summary>
    internal static IReadOnlyList<NativeProgramEntry<TourianAccessFloor>> TourianAccessEntries { get; } =
    [
        new NativeProgramEntry<TourianAccessFloor>(TourianAccessFloor.Crumble, 0xaae5),
        new NativeProgramEntry<TourianAccessFloor>(TourianAccessFloor.Clear, 0xab0c),
    ];

    /// <summary>The Wrecked Ship attic's list (<c>InstList_PLM_WreckedShipAttic</c>, $84:BAFF).</summary>
    internal const ushort WreckedShipAtticList = 0xbaff;

    /// <summary>The lavaquake list's length in words: three install-and-sleep phases.</summary>
    internal const int SpeedBoosterEscapeWordCount = 9;
}
