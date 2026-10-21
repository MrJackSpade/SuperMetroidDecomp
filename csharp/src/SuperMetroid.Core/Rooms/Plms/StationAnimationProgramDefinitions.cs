namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Immutable timing and draw selections for the four station animation families.</summary>
internal static class StationAnimationProgramDefinitions
{
    /// <summary>Pairs a station frame's native hold duration with the draw shown while it is held.</summary>
    /// <param name="Duration">Number of station animation ticks before advancing to the next frame.</param>
    /// <param name="Draw">The blocks displayed for this frame.</param>
    internal readonly record struct Frame(ushort Duration, PlmDraw Draw);

    /// <summary>Timed frames in each list: three for map/resource cycles, one for each save entry.</summary>
    internal static int FrameCount(StationAnimationList list) => list switch
    {
        StationAnimationList.MapIdle or StationAnimationList.MapAcquired or
            StationAnimationList.Energy or StationAnimationList.Missile => 3,
        StationAnimationList.SaveIdle or StationAnimationList.SaveFirstFrame or
            StationAnimationList.SaveSecondFrame => 1,
        _ => throw new InvalidOperationException($"Undefined StationAnimationList {list}."),
    };

    /// <summary>The draws a list shows, in order.</summary>
    private static StationDraw[] Draws(StationAnimationList list) => list switch
    {
        StationAnimationList.MapIdle or StationAnimationList.MapAcquired => [StationDraw.MapFrame0, StationDraw.MapFrame1, StationDraw.MapFrame2],
        StationAnimationList.Energy => [StationDraw.EnergyFrame0, StationDraw.EnergyFrame1, StationDraw.EnergyFrame2],
        StationAnimationList.Missile => [StationDraw.MissileFrame0, StationDraw.MissileFrame1, StationDraw.MissileFrame2],
        StationAnimationList.SaveIdle => [StationDraw.SaveIdle],
        StationAnimationList.SaveFirstFrame => [StationDraw.SaveActive],
        StationAnimationList.SaveSecondFrame => [StationDraw.SaveAlternate],
        _ => throw new InvalidOperationException($"Undefined StationAnimationList {list}."),
    };

    /// <summary>
    /// Seven named program entries own fifteen timed frames. Map/resource cycles
    /// have three frames and six-tick holds;
    /// acquired maps hold two ticks. Save idle draws once for one tick; its two
    /// active entries each own one four-tick frame. Reject all other frame indexes.
    /// </summary>
    internal static Frame Resolve(StationAnimationList list, int frameIndex)
    {
        if ((uint)frameIndex >= (uint)FrameCount(list))
            throw new InvalidDataException(
                $"Station animation {list} frame {frameIndex} is not compiled.");
        ushort duration = list switch
        {
            StationAnimationList.MapAcquired => 2,
            StationAnimationList.SaveIdle => 1,
            StationAnimationList.SaveFirstFrame or StationAnimationList.SaveSecondFrame => 4,
            StationAnimationList.MapIdle or StationAnimationList.Energy or StationAnimationList.Missile => 6,
            _ => throw new InvalidOperationException($"Undefined StationAnimationList {list}."),
        };
        return new(duration, PlmDraw.Of(Draws(list)[frameIndex]));
    }
}
