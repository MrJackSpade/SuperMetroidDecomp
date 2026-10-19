using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>
/// The native bank-$84 lists of the room PLM programs typed by #1296, for the ROM oracles that walk them beside the
/// typed programs. Only verification names these lists.
/// </summary>
internal static class RoomPlmNativeListAddresses
{
    /// <summary>The n00b tube's lists (<c>InstList_PLM_NoobTube_0..4</c>, $84:D4D4..D524).</summary>
    internal static IReadOnlyList<(NoobTubeProgram Program, ushort Entry)> NoobTubeEntries { get; } =
    [
        (NoobTubeProgram.Main, 0xd4d4),
        (NoobTubeProgram.WaitForInput, 0xd4e8),
        (NoobTubeProgram.Break, 0xd4f2),
        (NoobTubeProgram.AlreadyBroken, 0xd521),
    ];

    /// <summary>The Brinstar plants' lists (<c>InstList_PLM_BrinstarFloorPlant_0..1</c>, <c>..CeilingPlant_0..1</c>, $84:ACB8..AD37).</summary>
    internal static IReadOnlyList<(SamusEaterProgram Program, ushort Entry)> SamusEaterEntries { get; } =
    [
        (SamusEaterProgram.FloorBite, 0xacb8),
        (SamusEaterProgram.FloorChew, 0xacbf),
        (SamusEaterProgram.CeilingBite, 0xacf8),
        (SamusEaterProgram.CeilingChew, 0xacff),
    ];

    /// <summary>The shared one-instruction lists (<c>InstList_PLM_Delete</c> $84:AAE3, the metroids-cleared sleep $84:DB42).</summary>
    internal static IReadOnlyList<(RoomPlmSharedProgram Program, ushort Entry)> SharedEntries { get; } =
    [
        (RoomPlmSharedProgram.Delete, 0xaae3),
        (RoomPlmSharedProgram.Sleep, 0xdb42),
    ];

    /// <summary>The Speed Booster lavaquake's list (<c>InstList_PLM_SpeedBoosterEscape</c>, $84:B88A), one phase per sleep.</summary>
    internal static IReadOnlyList<(SpeedBoosterEscapeProgram Program, ushort Entry)> SpeedBoosterEscapeEntries { get; } =
    [
        (SpeedBoosterEscapeProgram.WaitForSpeedBooster, 0xb88a),
        (SpeedBoosterEscapeProgram.WaitForSamusLeft, 0xb890),
        (SpeedBoosterEscapeProgram.AdvanceLava, 0xb896),
    ];

    /// <summary>The Speed Booster blocks' lists ($84:C928 and $84:C951..C9F8).</summary>
    /// <summary>Every scripted room program's native entry.</summary>
    internal static IReadOnlyList<(ScriptedRoomPlmProgram Program, ushort Entry)> ScriptedRoomEntries { get; } =
    [
        (ScriptedRoomPlmProgram.ElevatorPlatform, 0xafb6),
        (ScriptedRoomPlmProgram.ShaktoolRoomController, 0xb8d6),
        (ScriptedRoomPlmProgram.MaridiaElevatube, 0xb8f0),
        (ScriptedRoomPlmProgram.OldTourianWallWait, 0xb919),
        (ScriptedRoomPlmProgram.OldTourianWallOpen, 0xb91f),
        (ScriptedRoomPlmProgram.EscapeAnimalReaction, 0xb9a2),
        (ScriptedRoomPlmProgram.CrateriaPassage, 0xbb19),
        (ScriptedRoomPlmProgram.MotherBrainEscapeGate, 0xbb34),
        (ScriptedRoomPlmProgram.MotherBrainEscapeGateClosing, 0xbb44),
        (ScriptedRoomPlmProgram.EnemyBreakableTerrain, 0xcd53),
        (ScriptedRoomPlmProgram.ChozoCrumblePlug, 0xd0f6),
        (ScriptedRoomPlmProgram.LowerNorfairChozoHand, 0xd13f),
        (ScriptedRoomPlmProgram.LowerNorfairChozoHandLowered, 0xd14d),
        (ScriptedRoomPlmProgram.ChozoClearSlope, 0xd3cf),
        (ScriptedRoomPlmProgram.ChozoBlockSlope, 0xd3ec),
    ];

    /// <summary>The native entry of block-reaction program <paramref name="program"/>.</summary>
    internal static ushort BlockReactionEntry(BlockReactionPlmProgram program) =>
        BlockReactionEntries.Single(entry => entry.Program == program).Entry;

    /// <summary>Every block-reaction program's native entry.</summary>
    internal static IReadOnlyList<(BlockReactionPlmProgram Program, ushort Entry)> BlockReactionEntries { get; } =
    [
        (BlockReactionPlmProgram.CrumbleReveal1x1, 0xc8ec),
        (BlockReactionPlmProgram.CrumbleReveal2x1, 0xc8f2),
        (BlockReactionPlmProgram.CrumbleReveal1x2, 0xc8f8),
        (BlockReactionPlmProgram.CrumbleReveal2x2, 0xc8fe),
        (BlockReactionPlmProgram.BombedPowerBombBlockUnused, 0xc91c),
        (BlockReactionPlmProgram.BombedSuperMissileBlockUnused, 0xc922),
        (BlockReactionPlmProgram.ContactCrumble1x1Respawning, 0xc9f9),
        (BlockReactionPlmProgram.ContactCrumble2x1Respawning, 0xca1c),
        (BlockReactionPlmProgram.ContactCrumble1x2Respawning, 0xca41),
        (BlockReactionPlmProgram.ContactCrumble2x2Respawning, 0xca66),
        (BlockReactionPlmProgram.ContactCrumble1x1Permanent, 0xca8b),
        (BlockReactionPlmProgram.ContactCrumble2x1Permanent, 0xcaa0),
        (BlockReactionPlmProgram.ContactCrumble1x2Permanent, 0xcab5),
        (BlockReactionPlmProgram.ContactCrumble2x2Permanent, 0xcaca),
        (BlockReactionPlmProgram.RespawningShotBlock1x1, 0xcadf),
        (BlockReactionPlmProgram.RespawningShotBlock2x1, 0xcb02),
        (BlockReactionPlmProgram.RespawningShotBlock1x2, 0xcb27),
        (BlockReactionPlmProgram.RespawningShotBlock2x2, 0xcb4c),
        (BlockReactionPlmProgram.RespawningSuperMissileBlock, 0xcb71),
        (BlockReactionPlmProgram.RespawningPowerBombBlock, 0xcb94),
        (BlockReactionPlmProgram.PermanentShotBlock1x1, 0xcbb7),
        (BlockReactionPlmProgram.PermanentShotBlock2x1, 0xcbcc),
        (BlockReactionPlmProgram.PermanentShotBlock1x2, 0xcbe1),
        (BlockReactionPlmProgram.PermanentShotBlock2x2, 0xcbf6),
        (BlockReactionPlmProgram.PermanentSuperMissileBlock, 0xcc0b),
        (BlockReactionPlmProgram.PermanentPowerBombBlock, 0xcc20),
        (BlockReactionPlmProgram.CollisionBombBlock1x1Respawning, 0xcc35),
        (BlockReactionPlmProgram.ReactionBombBlock1x1Respawning, 0xcc3c),
        (BlockReactionPlmProgram.BombBlock1x1RespawningBreak, 0xcc3f),
        (BlockReactionPlmProgram.CollisionBombBlock2x1Respawning, 0xcc5f),
        (BlockReactionPlmProgram.ReactionBombBlock2x1Respawning, 0xcc66),
        (BlockReactionPlmProgram.BombBlock2x1RespawningBreak, 0xcc69),
        (BlockReactionPlmProgram.CollisionBombBlock1x2Respawning, 0xcc8b),
        (BlockReactionPlmProgram.ReactionBombBlock1x2Respawning, 0xcc92),
        (BlockReactionPlmProgram.BombBlock1x2RespawningBreak, 0xcc95),
        (BlockReactionPlmProgram.CollisionBombBlock2x2Respawning, 0xccb7),
        (BlockReactionPlmProgram.ReactionBombBlock2x2Respawning, 0xccbe),
        (BlockReactionPlmProgram.BombBlock2x2RespawningBreak, 0xccc1),
        (BlockReactionPlmProgram.CollisionBombBlock1x1Permanent, 0xcce3),
        (BlockReactionPlmProgram.ReactionBombBlock1x1Permanent, 0xccea),
        (BlockReactionPlmProgram.BombBlock1x1PermanentBreak, 0xcced),
        (BlockReactionPlmProgram.CollisionBombBlock2x1Permanent, 0xccff),
        (BlockReactionPlmProgram.ReactionBombBlock2x1Permanent, 0xcd06),
        (BlockReactionPlmProgram.BombBlock2x1PermanentBreak, 0xcd09),
        (BlockReactionPlmProgram.CollisionBombBlock1x2Permanent, 0xcd1b),
        (BlockReactionPlmProgram.ReactionBombBlock1x2Permanent, 0xcd22),
        (BlockReactionPlmProgram.BombBlock1x2PermanentBreak, 0xcd25),
        (BlockReactionPlmProgram.CollisionBombBlock2x2Permanent, 0xcd37),
        (BlockReactionPlmProgram.ReactionBombBlock2x2Permanent, 0xcd3e),
        (BlockReactionPlmProgram.BombBlock2x2PermanentBreak, 0xcd41),
        (BlockReactionPlmProgram.RespawningBreakableGrappleBlock, 0xcd6a),
        (BlockReactionPlmProgram.PermanentBreakableGrappleBlock, 0xcda9),
    ];

    internal static IReadOnlyList<(SpeedBoosterBlockProgram Program, ushort Entry)> SpeedBoosterBlockEntries { get; } =
    [
        (SpeedBoosterBlockProgram.BrinstarSlowRespawning, 0xc951),
        (SpeedBoosterBlockProgram.Respawning, 0xc974),
        (SpeedBoosterBlockProgram.DachoraRespawning, 0xc997),
        (SpeedBoosterBlockProgram.BrinstarSlowPermanent, 0xc9cf),
        (SpeedBoosterBlockProgram.Permanent, 0xc9e4),
        (SpeedBoosterBlockProgram.BombReveal, 0xc928),
    ];

    /// <summary>
    /// The downward gate's lists: the open and closed gates ($84:BC13, $84:BC3A), the continuations after their sleeps
    /// ($84:BC1F, $84:BC46), and the eight trigger lists from $84:BCAF, six bytes each.
    /// </summary>
    internal static IReadOnlyList<(DownwardGateProgram Program, ushort Entry)> DownwardGateEntries { get; } =
    [
        (DownwardGateProgram.Open, 0xbc13),
        (DownwardGateProgram.Close, 0xbc1f),
        (DownwardGateProgram.Closed, 0xbc3a),
        (DownwardGateProgram.Reopen, 0xbc46),
        (DownwardGateProgram.TriggerBlueLeft, 0xbcaf),
        (DownwardGateProgram.TriggerBlueRight, 0xbcb5),
        (DownwardGateProgram.TriggerRedLeft, 0xbcbb),
        (DownwardGateProgram.TriggerRedRight, 0xbcc1),
        (DownwardGateProgram.TriggerGreenLeft, 0xbcc7),
        (DownwardGateProgram.TriggerGreenRight, 0xbccd),
        (DownwardGateProgram.TriggerYellowLeft, 0xbcd3),
        (DownwardGateProgram.TriggerYellowRight, 0xbcd9),
    ];

    /// <summary>The bytes from a sleeping gate list's entry to the continuation after its sleep.</summary>
    internal const int DownwardGateSleepContinuationOffset = 12;

    /// <summary>The Tourian elevator access lists (<c>InstList_PLM_CrumbleAccessToTourianElevator_0..1</c>, <c>..ClearAccess..</c>).</summary>
    internal static IReadOnlyList<(TourianAccessProgram Program, ushort Entry)> TourianAccessEntries { get; } =
    [
        (TourianAccessProgram.Crumble, 0xaae5),
        (TourianAccessProgram.CrumbleRow, 0xaae8),
        (TourianAccessProgram.Clear, 0xab0c),
    ];

    /// <summary>The Wrecked Ship attic's list (<c>InstList_PLM_WreckedShipAttic</c>, $84:BAFF).</summary>
    internal static IReadOnlyList<(WreckedShipAtticProgram Program, ushort Entry)> WreckedShipAtticEntries { get; } =
    [
        (WreckedShipAtticProgram.Attic, 0xbaff),
    ];

    /// <summary>The lavaquake list's length in words: three install-and-sleep phases.</summary>
    internal const int SpeedBoosterEscapeWordCount = 9;
}
