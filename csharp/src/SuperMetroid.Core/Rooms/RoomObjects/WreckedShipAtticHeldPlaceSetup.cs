namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The setup of the Wrecked Ship attic's held place: the place holds an object that does nothing.</summary>
internal sealed record WreckedShipAtticHeldPlaceSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpWreckedShipAtticHeldPlace();
}
