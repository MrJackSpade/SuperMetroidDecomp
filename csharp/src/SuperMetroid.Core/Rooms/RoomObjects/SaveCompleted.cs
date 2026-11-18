namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A save station's save has completed and shows the saved-game message (#2383).</summary>
/// <param name="Save">The save station that saved.</param>
public sealed record SaveCompleted(SaveStationActivation Save) : StationActivationEvent
{
    /// <inheritdoc/>
    public override GameplayMessageId Message => GameplayMessageId.SaveCompleted;

    /// <inheritdoc/>
    internal override void Open(IStationActivationMessages messages) => Ensure.NotNull(messages).ShowSaveCompleted(Save);
}
