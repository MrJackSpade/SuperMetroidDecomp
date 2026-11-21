namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A downward gate's setup.</summary>
internal sealed partial record DownwardGateSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override RoomObjectKind Kind => RoomObjectKind.DownwardGate;

    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpDownwardGate();
}
