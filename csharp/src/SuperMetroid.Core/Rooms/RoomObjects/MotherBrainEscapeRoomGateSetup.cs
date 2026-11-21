namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The setup of the gate Mother Brain's escape room shuts behind Samus.</summary>
internal sealed partial record MotherBrainEscapeRoomGateSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override RoomObjectKind Kind => RoomObjectKind.MotherBrainEscapeRoomGate;

    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpMotherBrainEscapeRoomGate();
}
