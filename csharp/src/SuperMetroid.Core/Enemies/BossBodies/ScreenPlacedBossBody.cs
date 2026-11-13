namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A boss body placed against the screen (#1617): the pixel of its parts' area at the screen's top-left, which the frame
/// turns into where the body lies in the room (<see cref="BossBodyPlacement.InRoom"/>), and whether it shows.
/// </summary>
/// <param name="Placement">The pixel of the body's parts' area at the screen's top-left.</param>
/// <param name="Shown">Whether the body shows.</param>
public readonly record struct ScreenPlacedBossBody(Position16<BackgroundPixels> Placement, bool Shown);
