namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// Where a boss places the body its room shows, and whether the body shows nothing this frame (#2194): what the room's
/// display latches of the boss through the body it attached to the room as it set up, for as long as the room shows the
/// body, never through the boss's species.
/// </summary>
internal interface IBossBodyShowing
{
    /// <summary>The room pixel each of the body's installed frames lies against, or null while the boss places none.</summary>
    Position16<RoomPixels>? BodyPlacement { get; }

    /// <summary>Whether the body shows nothing this frame.</summary>
    bool BodyHidden { get; }
}
