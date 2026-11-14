namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The condition a <see cref="TorizoCue.Replay"/> cue replays its run of steps while (#1788).</summary>
internal enum TorizoReplayKind
{
    /// <summary>While the jumping body rises.</summary>
    WhileRising,
    /// <summary>While Golden Torizo has not yet dropped to its floor.</summary>
    WhileAirborne,
    /// <summary>While any of Golden Torizo's eggs remains.</summary>
    WhileEggsRemain,
}
