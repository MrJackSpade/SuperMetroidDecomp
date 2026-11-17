namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// How one Torizo walks (#633, #1891, #2335): the turn a wall in its way starts, by the way it faces, and how long it waits
/// before it turns toward a Samus behind it.
/// </summary>
/// <param name="WallTurnFacingRight">The animation a wall in its way turns it into while it faces right.</param>
/// <param name="WallTurnFacingLeft">The animation a wall in its way turns it into while it faces left.</param>
/// <param name="TurnDelay">The updates it waits before it turns toward a Samus behind it.</param>
internal sealed record TorizoWalker(TorizoAnimation WallTurnFacingRight, TorizoAnimation WallTurnFacingLeft, int TurnDelay);
