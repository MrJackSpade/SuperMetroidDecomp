namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Immutable timing and draw-list selections for the four station animation families.</summary>
internal static class StationAnimationProgramDefinitions
{
    /// <summary>Pairs a station frame's native hold duration with the bank-$84 PLM draw list used while it is held.</summary>
    /// <param name="Duration">Number of station animation ticks before advancing to the next frame.</param>
    /// <param name="DrawPointer">Bank-$84 pointer of the blocks displayed for this frame.</param>
    internal readonly record struct Frame(ushort Duration, ushort DrawPointer);

    /// <summary>Timed frames in each list: three for map/resource cycles, one for each save entry.</summary>
    internal static int FrameCount(StationAnimationList list) => list switch
    {
        StationAnimationList.MapIdle or StationAnimationList.MapAcquired or
            StationAnimationList.Energy or StationAnimationList.Missile => 3,
        StationAnimationList.SaveIdle or StationAnimationList.SaveFirstFrame or
            StationAnimationList.SaveSecondFrame => 1,
        _ => throw new InvalidOperationException($"Undefined StationAnimationList {list}."),
    };

    /// <summary>
    /// Seven named program entries own fifteen timed frames. Map/resource cycles
    /// have three frames at twelve-byte draw-list strides and six-tick holds;
    /// acquired maps hold two ticks. Save idle draws once for one tick; its two
    /// active entries each own one four-tick frame. Reject all other frame indexes.
    /// </summary>
    internal static Frame Resolve(StationAnimationList list, int frameIndex)
    {
        if ((uint)frameIndex >= (uint)FrameCount(list))
            throw new InvalidDataException(
                $"Station animation $84:{(int)list:X4} frame {frameIndex} is not compiled.");
        ushort duration = list switch
        {
            StationAnimationList.MapAcquired => 2,
            StationAnimationList.SaveIdle => 1,
            StationAnimationList.SaveFirstFrame or StationAnimationList.SaveSecondFrame => 4,
            StationAnimationList.MapIdle or StationAnimationList.Energy or StationAnimationList.Missile => 6,
            _ => throw new InvalidOperationException($"Undefined StationAnimationList {list}."),
        };
        ushort firstDraw = list switch
        {
            StationAnimationList.MapIdle or StationAnimationList.MapAcquired => (ushort)StationDraw.MapFrame0,
            StationAnimationList.Energy => (ushort)StationDraw.EnergyFrame0,
            StationAnimationList.Missile => (ushort)StationDraw.MissileFrame0,
            StationAnimationList.SaveIdle => (ushort)StationDraw.SaveIdle,
            StationAnimationList.SaveFirstFrame => (ushort)StationDraw.SaveActive,
            StationAnimationList.SaveSecondFrame => (ushort)StationDraw.SaveAlternate,
            _ => throw new InvalidOperationException($"Undefined StationAnimationList {list}."),
        };
        return new(duration, (ushort)(firstDraw + frameIndex * RoomPlmStationDrawDefinitions.FrameStride));
    }
}
