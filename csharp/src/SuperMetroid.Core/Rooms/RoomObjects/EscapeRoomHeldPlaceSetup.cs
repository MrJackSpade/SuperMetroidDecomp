namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The setup of an escape room's held place: the place holds an object that does nothing.</summary>
internal sealed record EscapeRoomHeldPlaceSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override RoomObjectKind Kind => RoomObjectKind.EscapeRoomHeldPlace;

    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpEscapeRoomHeldPlace();
}
