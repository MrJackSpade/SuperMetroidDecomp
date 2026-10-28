namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Immutable timing and draw selections for the four station animation families.</summary>
internal static class StationAnimationProgramDefinitions
{
    /// <summary>Pairs a station frame's native hold duration with the draw shown while it is held.</summary>
    /// <param name="Duration">Number of station animation ticks before advancing to the next frame.</param>
    /// <param name="Draw">The blocks displayed for this frame.</param>
    internal readonly record struct Frame(int Duration, PlmDraw Draw);

    /// <summary>
    /// Seven named program entries own fifteen timed frames. Map/resource cycles
    /// have three frames and six-tick holds;
}
