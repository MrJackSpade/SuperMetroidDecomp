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
