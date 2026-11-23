namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>How Ridley's pogo tail has settled after an update, which chooses the pogo's next step (#2553).</summary>
/// <param name="LinkMoves">Whether a link of the tail still moves after this update.</param>
/// <param name="RestsWhileFalling">Whether every link rests while Ridley falls or hangs still.</param>
internal readonly partial record struct RidleyPogoTailSettling(bool LinkMoves, bool RestsWhileFalling);
