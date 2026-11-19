namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Metroid room's clearance's setup, watching its placement's room's Metroids.</summary>
/// <param name="Watches">The event the clearance marks once the room's Metroids are dead.</param>
internal sealed record MetroidRoomClearanceSetup(WatchedMetroidRoom Watches) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpMetroidRoomClearance(Watches);
}
