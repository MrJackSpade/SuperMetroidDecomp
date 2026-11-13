using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The gate in Mother Brain's escape room (#1554): placed shut
/// (<see cref="MotherBrainEscapeGateDefinitions.Shut"/>), or closing behind Samus
/// (<see cref="MotherBrainEscapeGateDefinitions.Closing"/>). A shut gate at the door Samus arrives through closes behind her
/// instead, from its next update.
/// </summary>
/// <param name="change">The gate's change, shut or closing.</param>
internal sealed class MotherBrainEscapeGateObject(TerrainChange change) : TerrainChangeObject(change), IClosingDoor
{
    /// <inheritdoc/>
    public void CloseBehindSamus() => Restart(MotherBrainEscapeGateDefinitions.Closing);
}
