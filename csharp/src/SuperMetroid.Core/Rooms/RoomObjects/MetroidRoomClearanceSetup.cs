namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Metroid room's clearance's setup, watching its placement's room's Metroids.</summary>
/// <param name="Watches">The event whose Metroids the clearance watches, or null for none.</param>
internal sealed record MetroidRoomClearanceSetup(MetroidRoomArgument? Watches) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpMetroidRoomClearance(Watches);
}
