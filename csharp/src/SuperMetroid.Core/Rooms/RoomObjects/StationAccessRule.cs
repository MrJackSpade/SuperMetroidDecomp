namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// How Samus uses a map, energy or missile station: she runs into one of the access blocks either side of the station
/// toward it, the station shows its animations while she does, and its activation gives her its reward and shows its
/// message.
/// </summary>
/// <param name="Kind">The station kind the rule belongs to.</param>
/// <param name="RightOffset">The right access block's distance in blocks from the station block.</param>
/// <param name="LeftOffset">The left access block's distance in blocks from the station block.</param>
/// <param name="Animation">The station's ordinary animation.</param>
/// <param name="AcquiredAnimation">The animation the station shows once its reward is held; null when it keeps its ordinary one.</param>
/// <param name="CompletedMessage">The message the station's activation shows.</param>
/// <param name="Reward">What the station's activation gives Samus: a refill, or its area's map.</param>
internal sealed partial record StationAccessRule(
    StationKind Kind,
    int RightOffset,
    int LeftOffset,
    StationAnimation Animation,
    StationAnimation? AcquiredAnimation,
    GameplayMessageId CompletedMessage,
    StationReward Reward)
{
    /// <summary>The columns from the station block to its access block on <paramref name="side"/>.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="side"/> is undefined.</exception>
    internal int ColumnsToAccessOn(HorizontalDirection side) => side switch
    {
        HorizontalDirection.Right => RightOffset,
        HorizontalDirection.Left => LeftOffset,
        _ => throw new InvalidOperationException($"Undefined {nameof(HorizontalDirection)} {side}."),
    };
}
