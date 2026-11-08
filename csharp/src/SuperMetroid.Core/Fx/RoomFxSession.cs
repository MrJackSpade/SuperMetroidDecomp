namespace SuperMetroid.Core.Fx;

/// <summary>
/// The room-FX state that outlives each room: the Fireflea darkness state, which room loading never clears. Each room's
/// <see cref="RoomFxOverlayState"/> reads it here.
/// </summary>
public sealed class RoomFxSession
{
    /// <summary>The Fireflea FX state. It lives for the session, so a room load does not reset it.</summary>
    public FirefleaFxState Fireflea { get; } = new();
}
