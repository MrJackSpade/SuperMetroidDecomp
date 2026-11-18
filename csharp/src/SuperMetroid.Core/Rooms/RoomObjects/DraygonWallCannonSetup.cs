namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A Draygon wall cannon's setup, controlling the wall cannon its placement names.</summary>
/// <param name="Cannon">The wall cannon the placement controls.</param>
internal sealed record DraygonWallCannonSetup(DraygonWallCannon Cannon) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpDraygonCannon(kind, Cannon);
}
