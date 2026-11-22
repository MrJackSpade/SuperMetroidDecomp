namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Mother Brain's glass's setup.</summary>
internal sealed partial record MotherBrainGlassSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override RoomObjectKind Kind => RoomObjectKind.MotherBrainGlass;

    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpMotherBrainGlass();
}
