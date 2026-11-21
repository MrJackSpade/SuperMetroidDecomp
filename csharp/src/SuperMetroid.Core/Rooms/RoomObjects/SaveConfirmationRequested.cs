namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A save station asks whether to save at the load station it saves at (#2383).</summary>
/// <param name="Save">The save station that asks.</param>
internal sealed partial record SaveConfirmationRequested(SaveStationActivation Save) : StationActivationEvent
{
    /// <inheritdoc/>
    public override GameplayMessageId Message => GameplayMessageId.SaveConfirmation;

    /// <inheritdoc/>
    internal override void Open(IStationActivationMessages messages) => Ensure.NotNull(messages).RequestSaveConfirmation(Save);
}
