namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A downward gate's setup.</summary>
internal sealed record DownwardGateSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpDownwardGate();
}
