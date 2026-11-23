namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Maridia's glass tube's setup.</summary>
internal sealed partial record NoobTubeSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override RoomObjectKind Kind => RoomObjectKind.NoobTube;

    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpNoobTube();
}
