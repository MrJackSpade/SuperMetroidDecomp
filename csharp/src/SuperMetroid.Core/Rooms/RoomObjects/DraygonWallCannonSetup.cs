namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A Draygon wall cannon's setup (#2400): the wall cannon its placement controls, which wall it faces out from and whether
/// it starts whole or already destroyed, all carried by the placement.
/// </summary>
/// <param name="Cannon">The wall cannon the placement controls.</param>
/// <param name="Orientation">Which wall the cannon faces out from, selecting what it shows.</param>
/// <param name="Start">Whether the cannon starts whole or already destroyed.</param>
internal sealed record DraygonWallCannonSetup(DraygonWallCannon Cannon, DraygonCannonOrientation Orientation, DraygonCannonStart Start)
    : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override RoomObjectKind Kind => RoomObjectKindDefinitions.DraygonCannons[new(Orientation, Start)];

    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpDraygonCannon(this);
}
