namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A map or refill station has finished and shows its completion message (#2383).</summary>
public sealed record StationCompleted : StationActivationEvent
{
    /// <summary>A station's completion showing <paramref name="message"/>.</summary>
    /// <param name="message">The completion message: map data, energy or missiles.</param>
    public StationCompleted(GameplayMessageId message) => Message = message;

    /// <inheritdoc/>
    public override GameplayMessageId Message { get; }

    /// <inheritdoc/>
    internal override void Open(IStationActivationMessages messages) => Ensure.NotNull(messages).ShowStationMessage(Message);
}
