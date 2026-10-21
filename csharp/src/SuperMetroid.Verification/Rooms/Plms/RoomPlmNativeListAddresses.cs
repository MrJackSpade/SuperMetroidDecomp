using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static IReadOnlyList<NativeProgramEntry<NoobTubeProgram>> NoobTubeEntries { get; } =
    [
        new NativeProgramEntry<NoobTubeProgram>(NoobTubeProgram.Main, 0xd4d4),
        new NativeProgramEntry<NoobTubeProgram>(NoobTubeProgram.WaitForInput, 0xd4e8),
        new NativeProgramEntry<NoobTubeProgram>(NoobTubeProgram.Break, 0xd4f2),
        new NativeProgramEntry<NoobTubeProgram>(NoobTubeProgram.AlreadyBroken, 0xd521),
    ];

    /// <summary>The Brinstar plants' lists (<c>InstList_PLM_BrinstarFloorPlant_0..1</c>, <c>..CeilingPlant_0..1</c>, $84:ACB8..AD37).</summary>
    internal static IReadOnlyList<NativeProgramEntry<SamusEaterProgram>> SamusEaterEntries { get; } =
    [
        new NativeProgramEntry<SamusEaterProgram>(SamusEaterProgram.FloorBite, 0xacb8),
        new NativeProgramEntry<SamusEaterProgram>(SamusEaterProgram.FloorChew, 0xacbf),
        new NativeProgramEntry<SamusEaterProgram>(SamusEaterProgram.CeilingBite, 0xacf8),
        new NativeProgramEntry<SamusEaterProgram>(SamusEaterProgram.CeilingChew, 0xacff),
    ];

    /// <summary>The shared one-instruction lists (<c>InstList_PLM_Delete</c> $84:AAE3, the metroids-cleared sleep $84:DB42).</summary>
    internal static IReadOnlyList<NativeProgramEntry<RoomPlmSharedProgram>> SharedEntries { get; } =
    [
        new NativeProgramEntry<RoomPlmSharedProgram>(RoomPlmSharedProgram.Delete, 0xaae3),
        new NativeProgramEntry<RoomPlmSharedProgram>(RoomPlmSharedProgram.Sleep, 0xdb42),
    ];

    /// <summary>The Speed Booster lavaquake's list (<c>InstList_PLM_SpeedBoosterEscape</c>, $84:B88A), one phase per sleep.</summary>
    internal static IReadOnlyList<NativeProgramEntry<SpeedBoosterEscapeProgram>> SpeedBoosterEscapeEntries { get; } =
    [
        new NativeProgramEntry<SpeedBoosterEscapeProgram>(SpeedBoosterEscapeProgram.WaitForSpeedBooster, 0xb88a),
        new NativeProgramEntry<SpeedBoosterEscapeProgram>(SpeedBoosterEscapeProgram.WaitForSamusLeft, 0xb890),
        new NativeProgramEntry<SpeedBoosterEscapeProgram>(SpeedBoosterEscapeProgram.AdvanceLava, 0xb896),
    ];

    /// <summary>The Speed Booster blocks' lists ($84:C928 and $84:C951..C9F8).</summary>
    /// <summary>Every scripted room program's native entry.</summary>
    internal static IReadOnlyList<NativeProgramEntry<ScriptedRoomPlmProgram>> ScriptedRoomEntries { get; } =
    [
        new NativeProgramEntry<ScriptedRoomPlmProgram>(ScriptedRoomPlmProgram.ElevatorPlatform, 0xafb6),
        new NativeProgramEntry<ScriptedRoomPlmProgram>(ScriptedRoomPlmProgram.ShaktoolRoomController, 0xb8d6),
        new NativeProgramEntry<ScriptedRoomPlmProgram>(ScriptedRoomPlmProgram.MaridiaElevatube, 0xb8f0),
        new NativeProgramEntry<ScriptedRoomPlmProgram>(ScriptedRoomPlmProgram.OldTourianWallWait, 0xb919),
        new NativeProgramEntry<ScriptedRoomPlmProgram>(ScriptedRoomPlmProgram.OldTourianWallOpen, 0xb91f),
        new NativeProgramEntry<ScriptedRoomPlmProgram>(ScriptedRoomPlmProgram.EscapeAnimalReaction, 0xb9a2),
        new NativeProgramEntry<ScriptedRoomPlmProgram>(ScriptedRoomPlmProgram.CrateriaPassage, 0xbb19),
        new NativeProgramEntry<ScriptedRoomPlmProgram>(ScriptedRoomPlmProgram.MotherBrainEscapeGate, 0xbb34),
        new NativeProgramEntry<ScriptedRoomPlmProgram>(ScriptedRoomPlmProgram.MotherBrainEscapeGateClosing, 0xbb44),
        new NativeProgramEntry<ScriptedRoomPlmProgram>(ScriptedRoomPlmProgram.EnemyBreakableTerrain, 0xcd53),
        new NativeProgramEntry<ScriptedRoomPlmProgram>(ScriptedRoomPlmProgram.ChozoCrumblePlug, 0xd0f6),
        new NativeProgramEntry<ScriptedRoomPlmProgram>(ScriptedRoomPlmProgram.LowerNorfairChozoHand, 0xd13f),
        new NativeProgramEntry<ScriptedRoomPlmProgram>(ScriptedRoomPlmProgram.LowerNorfairChozoHandLowered, 0xd14d),
        new NativeProgramEntry<ScriptedRoomPlmProgram>(ScriptedRoomPlmProgram.ChozoClearSlope, 0xd3cf),
        new NativeProgramEntry<ScriptedRoomPlmProgram>(ScriptedRoomPlmProgram.ChozoBlockSlope, 0xd3ec),
    ];

    /// <summary>The native entry of block-reaction program <paramref name="program"/>.</summary>
    internal static ushort BlockReactionEntry(BlockReactionPlmProgram program) =>
        BlockReactionEntries.Single(entry => entry.Program == program).Entry;

    /// <summary>Every block-reaction program's native entry.</summary>
    internal static IReadOnlyList<NativeProgramEntry<BlockReactionPlmProgram>> BlockReactionEntries { get; } =
    [
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.CrumbleReveal1x1, 0xc8ec),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.CrumbleReveal2x1, 0xc8f2),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.CrumbleReveal1x2, 0xc8f8),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.CrumbleReveal2x2, 0xc8fe),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.BombedPowerBombBlockUnused, 0xc91c),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.BombedSuperMissileBlockUnused, 0xc922),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.ContactCrumble1x1Respawning, 0xc9f9),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.ContactCrumble2x1Respawning, 0xca1c),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.ContactCrumble1x2Respawning, 0xca41),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.ContactCrumble2x2Respawning, 0xca66),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.ContactCrumble1x1Permanent, 0xca8b),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.ContactCrumble2x1Permanent, 0xcaa0),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.ContactCrumble1x2Permanent, 0xcab5),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.ContactCrumble2x2Permanent, 0xcaca),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.RespawningShotBlock1x1, 0xcadf),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.RespawningShotBlock2x1, 0xcb02),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.RespawningShotBlock1x2, 0xcb27),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.RespawningShotBlock2x2, 0xcb4c),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.RespawningSuperMissileBlock, 0xcb71),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.RespawningPowerBombBlock, 0xcb94),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.PermanentShotBlock1x1, 0xcbb7),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.PermanentShotBlock2x1, 0xcbcc),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.PermanentShotBlock1x2, 0xcbe1),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.PermanentShotBlock2x2, 0xcbf6),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.PermanentSuperMissileBlock, 0xcc0b),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.PermanentPowerBombBlock, 0xcc20),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.CollisionBombBlock1x1Respawning, 0xcc35),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.ReactionBombBlock1x1Respawning, 0xcc3c),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.BombBlock1x1RespawningBreak, 0xcc3f),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.CollisionBombBlock2x1Respawning, 0xcc5f),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.ReactionBombBlock2x1Respawning, 0xcc66),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.BombBlock2x1RespawningBreak, 0xcc69),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.CollisionBombBlock1x2Respawning, 0xcc8b),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.ReactionBombBlock1x2Respawning, 0xcc92),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.BombBlock1x2RespawningBreak, 0xcc95),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.CollisionBombBlock2x2Respawning, 0xccb7),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.ReactionBombBlock2x2Respawning, 0xccbe),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.BombBlock2x2RespawningBreak, 0xccc1),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.CollisionBombBlock1x1Permanent, 0xcce3),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.ReactionBombBlock1x1Permanent, 0xccea),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.BombBlock1x1PermanentBreak, 0xcced),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.CollisionBombBlock2x1Permanent, 0xccff),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.ReactionBombBlock2x1Permanent, 0xcd06),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.BombBlock2x1PermanentBreak, 0xcd09),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.CollisionBombBlock1x2Permanent, 0xcd1b),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.ReactionBombBlock1x2Permanent, 0xcd22),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.BombBlock1x2PermanentBreak, 0xcd25),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.CollisionBombBlock2x2Permanent, 0xcd37),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.ReactionBombBlock2x2Permanent, 0xcd3e),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.BombBlock2x2PermanentBreak, 0xcd41),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.RespawningBreakableGrappleBlock, 0xcd6a),
        new NativeProgramEntry<BlockReactionPlmProgram>(BlockReactionPlmProgram.PermanentBreakableGrappleBlock, 0xcda9),
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
    internal static IReadOnlyList<NativeProgramEntry<TourianAccessProgram>> TourianAccessEntries { get; } =
    [
        new NativeProgramEntry<TourianAccessProgram>(TourianAccessProgram.Crumble, 0xaae5),
        new NativeProgramEntry<TourianAccessProgram>(TourianAccessProgram.CrumbleRow, 0xaae8),
        new NativeProgramEntry<TourianAccessProgram>(TourianAccessProgram.Clear, 0xab0c),
    ];

    /// <summary>The Wrecked Ship attic's list (<c>InstList_PLM_WreckedShipAttic</c>, $84:BAFF).</summary>
    internal static IReadOnlyList<NativeProgramEntry<WreckedShipAtticProgram>> WreckedShipAtticEntries { get; } =
    [
        new NativeProgramEntry<WreckedShipAtticProgram>(WreckedShipAtticProgram.Attic, 0xbaff),
    ];

    /// <summary>The lavaquake list's length in words: three install-and-sleep phases.</summary>
    internal const int SpeedBoosterEscapeWordCount = 9;
}
