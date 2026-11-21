namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Speed Booster's escape's setup.</summary>
internal sealed record SpeedBoosterEscapeSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override RoomObjectKind Kind => RoomObjectKind.SpeedBoosterEscape;

    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpSpeedBoosterEscape();
}
