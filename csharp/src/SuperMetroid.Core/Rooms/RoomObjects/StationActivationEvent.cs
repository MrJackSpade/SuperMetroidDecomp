using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A station's action the room-object step publishes for the gameplay frame (#2383): a save station asking to save, a save
/// station's save completed, or a map or refill station's completion message. Each is its own type, so the frame opens
/// what the action asks and cannot receive an impossible combination of station and message.
/// </summary>
public abstract record StationActivationEvent
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

/// <summary>A save station asks whether to save at the load station it saves at (#2383).</summary>
/// <param name="Save">The save station that asks.</param>
public sealed record SaveConfirmationRequested(SaveStationActivation Save) : StationActivationEvent
{
    /// <inheritdoc/>
    public override GameplayMessageId Message => GameplayMessageId.SaveConfirmation;

    /// <inheritdoc/>
    internal override void Open(IStationActivationMessages messages) => Ensure.NotNull(messages).RequestSaveConfirmation(Save);
}

/// <summary>A save station's save has completed and shows the saved-game message (#2383).</summary>
/// <param name="Save">The save station that saved.</param>
public sealed record SaveCompleted(SaveStationActivation Save) : StationActivationEvent
{
    /// <inheritdoc/>
    public override GameplayMessageId Message => GameplayMessageId.SaveCompleted;

    /// <inheritdoc/>
    internal override void Open(IStationActivationMessages messages) => Ensure.NotNull(messages).ShowSaveCompleted(Save);
}

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

/// <summary>The save station an action came from (#2383): the load station it saves at, its area and its block.</summary>
/// <param name="Station">The load station the save station saves at, which identifies the resident station a save returns to.</param>
/// <param name="Area">The room's area.</param>
/// <param name="Block">The station's room block, rather than a side-access trigger's.</param>
public readonly record struct SaveStationActivation(LoadStation Station, AreaId Area, Position16<RoomBlocks> Block);

/// <summary>What the gameplay frame opens for the station actions a room-object step publishes (#2383).</summary>
internal interface IStationActivationMessages
{
    /// <summary>Asks whether to save at <paramref name="save"/>.</summary>
    void RequestSaveConfirmation(SaveStationActivation save);

    /// <summary>Shows the saved-game message of <paramref name="save"/>.</summary>
    void ShowSaveCompleted(SaveStationActivation save);

    /// <summary>Shows a map or refill station's completion <paramref name="message"/>.</summary>
    void ShowStationMessage(GameplayMessageId message);
}
