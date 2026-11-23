using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The gate in Mother Brain's escape room (#1554): placed shut
/// (<see cref="MotherBrainEscapeGateDefinitions.Shut"/>), or closing behind Samus
/// (<see cref="MotherBrainEscapeGateDefinitions.Closing"/>). A shut gate at the door Samus arrives through closes behind her
/// instead, from its next update.
/// </summary>
internal sealed partial class MotherBrainEscapeGateObject : TerrainChangeObject, IDoorBehindSamus
{
    /// <summary>Creates a <see cref="MotherBrainEscapeGateObject"/>.</summary>
    /// <param name="change">The gate's change, shut or closing.</param>
    public MotherBrainEscapeGateObject(TerrainChange change)
        : base(change)
    {
    }

    /// <summary>The gate never stays open behind Samus: its placement names no door opened for good.</summary>
    public bool StaysOpenBehindSamus(Func<LockedDoor, bool> openedForGood) =>
        false;

    /// <inheritdoc/>
    public void CloseBehindSamus() => Restart(MotherBrainEscapeGateDefinitions.Closing);
}
