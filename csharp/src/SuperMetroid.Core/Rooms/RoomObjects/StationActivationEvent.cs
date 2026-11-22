namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A station's action the room-object step publishes for the gameplay frame (#2383): a save station asking to save, a save
/// station's save completed, or a map or refill station's completion message. Each is its own type, so the frame opens
/// what the action asks and cannot receive an impossible combination of station and message.
/// </summary>
public abstract partial record StationActivationEvent
{
    /// <summary>Only the station actions below derive from this.</summary>
    private protected StationActivationEvent()
    {
    }

    /// <summary>The message the action opens.</summary>
    public abstract GameplayMessageId Message { get; }

    /// <summary>Opens what the action asks of <paramref name="messages"/>.</summary>
    internal abstract void Open(IStationActivationMessages messages);
}
