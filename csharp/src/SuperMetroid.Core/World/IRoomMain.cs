namespace SuperMetroid.Core.World;

/// <summary>
/// One room's main behaviour, composed by its load from the main its state names
/// (<see cref="RoomStates.RoomMainBehavior"/>) with the state it keeps, and run once on every gameplay update with the
/// room's own update (#2287). A room whose state runs no main has none.
/// </summary>
internal interface IRoomMain
{
    /// <summary>Runs the main once.</summary>
    /// <param name="update">The room and the session owners the update runs it against.</param>
    /// <returns>What the main did that the update acts on (#2545).</returns>
    RoomMainOutcome Run(RoomMainUpdate update);
}
