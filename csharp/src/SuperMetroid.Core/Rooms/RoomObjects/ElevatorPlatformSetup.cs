namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>An elevator platform's setup.</summary>
internal sealed record ElevatorPlatformSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override RoomObjectKind Kind => RoomObjectKind.ElevatorPlatform;

    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpElevatorPlatform();
}
