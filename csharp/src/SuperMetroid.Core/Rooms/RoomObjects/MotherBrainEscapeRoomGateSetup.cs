namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The setup of the gate Mother Brain's escape room shuts behind Samus.</summary>
internal sealed record MotherBrainEscapeRoomGateSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpMotherBrainEscapeRoomGate();
}
