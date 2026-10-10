namespace SuperMetroid.Core.Rooms;

/// <summary>The seven bank-$84 station animation entries, valued by their native list address.</summary>
internal enum StationAnimationList : ushort
{
    /// <summary>Map station idle cycle at $84:AD66, after its $84:AD62 entry opcode.</summary>
    MapIdle = 0xad66,
    /// <summary>Map station acquired cycle at $84:AD76.</summary>
    MapAcquired = 0xad76,
    /// <summary>Energy station idle/recharged cycle at $84:ADC6.</summary>
    Energy = 0xadc6,
    /// <summary>Missile station idle/recharged cycle at $84:AE50.</summary>
    Missile = 0xae50,
    /// <inheritdoc cref="RoomPlmInstructionLists.SaveStationIdleDraw"/>
    SaveIdle = RoomPlmInstructionLists.SaveStationIdleDraw,
    /// <inheritdoc cref="RoomPlmInstructionLists.SaveStationAnimationFirstFrame"/>
    SaveFirstFrame = RoomPlmInstructionLists.SaveStationAnimationFirstFrame,
    /// <inheritdoc cref="RoomPlmInstructionLists.SaveStationAnimationSecondFrame"/>
    SaveSecondFrame = RoomPlmInstructionLists.SaveStationAnimationSecondFrame,
}

/// <summary>Immutable timing and draw-list selections for the four station animation families.</summary>
internal static class StationAnimationProgramDefinitions
{
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
            StationAnimationList.MapIdle or StationAnimationList.MapAcquired => RoomPlmStationDrawDefinitions.MapFirst,
            StationAnimationList.Energy => RoomPlmStationDrawDefinitions.EnergyFirst,
            StationAnimationList.Missile => RoomPlmStationDrawDefinitions.MissileFirst,
            StationAnimationList.SaveIdle => RoomPlmStationDrawDefinitions.SaveIdle,
            StationAnimationList.SaveFirstFrame => RoomPlmStationDrawDefinitions.SaveActive,
            StationAnimationList.SaveSecondFrame => RoomPlmStationDrawDefinitions.SaveAlternate,
            _ => throw new InvalidOperationException($"Undefined StationAnimationList {list}."),
        };
        return new(duration, (ushort)(firstDraw + frameIndex * 12));
    }
}
