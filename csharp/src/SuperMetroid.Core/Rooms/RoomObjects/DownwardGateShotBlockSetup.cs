namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A downward gate's shot block's setup, triggering its placement's gate.</summary>
/// <param name="Trigger">The gate's colour and side the shot block triggers, or null for none.</param>
internal sealed record DownwardGateShotBlockSetup(GateTriggerArgument? Trigger) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpDownwardGateShotBlock(Trigger);
}
