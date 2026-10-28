using static SuperMetroid.Core.Rooms.Plms.StationAnimationProgramDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The frames and draws of each station animation program.</summary>
internal static class StationAnimationPrograms
{
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

    /// acquired maps hold two ticks. Save idle draws once for one tick; its two
    /// active entries each own one four-tick frame. Reject all other frame indexes.
    /// </summary>
    internal static Frame Resolve(StationAnimationList list, int frameIndex)
    {
        if ((uint)frameIndex >= (uint)FrameCount(list))
            throw new InvalidDataException(
                $"Station animation {list} frame {frameIndex} is not compiled.");
        int duration = list switch
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
