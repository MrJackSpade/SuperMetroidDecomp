using SuperMetroid.Core;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Development-tool members of <see cref="RoomPlmInstructionLists"/>; never linked by player hosts.
/// The PLM program audit enumerates these lists alongside the shipped catalog by reflection.
/// </summary>
internal static class RoomPlmInstructionListsTooling
{
    /// <summary>Downward gate's close-and-wait loop at <c>$84:BC13</c>.</summary>
    [AccessedByReflection]
    public const ushort DownwardGateClosing = 0xbc13;
    /// <summary>Downward gate's open-and-wait loop at <c>$84:BC3A</c>.</summary>
    [AccessedByReflection]
    public const ushort DownwardGateOpening = 0xbc3a;
    /// <summary><c>$84:BCB5 InstList_PLM_DownwardsGateShotblock_BlueRight</c>.</summary>
    [AccessedByReflection]
    public const ushort DownwardGateShotBlockBlueRight = 0xbcb5;
    /// <summary><c>$84:BCC7 InstList_PLM_DownwardsGateShotblock_GreenLeft</c>.</summary>
    [AccessedByReflection]
    public const ushort DownwardGateShotBlockGreenLeft = 0xbcc7;
    /// <summary><c>$84:BCCD InstList_PLM_DownwardsGateShotblock_GreenRight</c>.</summary>
    [AccessedByReflection]
    public const ushort DownwardGateShotBlockGreenRight = 0xbccd;
    /// <summary><c>$84:BCBB InstList_PLM_DownwardsGateShotblock_RedLeft</c>.</summary>
    [AccessedByReflection]
    public const ushort DownwardGateShotBlockRedLeft = 0xbcbb;
    /// <summary><c>$84:BCC1 InstList_PLM_DownwardsGateShotblock_RedRight</c>.</summary>
    [AccessedByReflection]
    public const ushort DownwardGateShotBlockRedRight = 0xbcc1;
    /// <summary><c>$84:BCD3 InstList_PLM_DownwardsGateShotblock_YellowLeft</c>.</summary>
    [AccessedByReflection]
    public const ushort DownwardGateShotBlockYellowLeft = 0xbcd3;
    /// <summary><c>$84:BCD9 InstList_PLM_DownwardsGateShotblock_YellowRight</c>.</summary>
    [AccessedByReflection]
    public const ushort DownwardGateShotBlockYellowRight = 0xbcd9;
    /// <summary>
    /// <c>$84:BB34 InstList_PLM_GateThatClosesDuringEscapeAfterMotherBrain_0</c>:
    /// draw the already-closed gate for six frames, then release the resident PLM slot.
    /// </summary>
    [AccessedByReflection]
    public const ushort MotherBrainEscapeRoomGateClosed = 0xbb34;
    /// <summary>Entry two bytes into the scroll list after collision wakes it.</summary>
    [AccessedByReflection]
    public const ushort ScrollTriggerActivated = 0xaf8c;
}
