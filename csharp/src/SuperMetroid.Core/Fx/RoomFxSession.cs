namespace SuperMetroid.Core.Fx;

/// <summary>
/// The room-FX state that outlives each room: the Fireflea darkness state and the fixed colour, which room loading
/// never clears. Each room's <see cref="RoomFxOverlayState"/> reads them here.
/// </summary>
public sealed class RoomFxSession
{
    /// <summary>The Fireflea FX state. It lives for the session, so a room load does not reset it.</summary>
    public FirefleaFxState Fireflea { get; } = new();

    /// <summary>The room's fixed colour. It is session-lived, so it keeps the last writer's colour across rooms.</summary>
    public RoomFixedColor FixedColors { get; } = new();
}
