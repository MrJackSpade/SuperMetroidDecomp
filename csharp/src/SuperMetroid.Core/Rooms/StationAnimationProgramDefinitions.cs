namespace SuperMetroid.Core.Rooms;

/// <summary>Immutable timing and draw-list selections for the four station animation families.</summary>
internal static class StationAnimationProgramDefinitions
{
    /// <summary>Map station idle cycle at $84:AD66, after its $84:AD62 entry opcode.</summary>
    internal const ushort MapIdle = 0xad66;
    /// <summary>Map station acquired cycle at $84:AD76.</summary>
    internal const ushort MapAcquired = 0xad76;
    /// <summary>Energy station idle/recharged cycle at $84:ADC6.</summary>
    internal const ushort Energy = 0xadc6;
    /// <summary>Missile station idle/recharged cycle at $84:AE50.</summary>
    internal const ushort Missile = 0xae50;

    internal readonly record struct Frame(ushort Duration, ushort DrawPointer);

    /// <summary>
    /// Seven named program entries own fifteen timed frames. Map/resource cycles
    /// have three frames at twelve-byte draw-list strides and six-tick holds;
    /// acquired maps hold two ticks. Save idle draws once for one tick; its two
    /// active entries each own one four-tick frame. Reject all other list/index pairs.
    /// </summary>
    internal static Frame Resolve(ushort list, int frameIndex)
    {
        int count = list is MapIdle or MapAcquired or Energy or Missile ? 3 :
            list is RoomPlmInstructionLists.SaveStationIdleDraw or
                RoomPlmInstructionLists.SaveStationAnimationFirstFrame or
                RoomPlmInstructionLists.SaveStationAnimationSecondFrame ? 1 : 0;
        if ((uint)frameIndex >= (uint)count)
            throw new InvalidDataException(
                $"Station animation $84:{list:X4} frame {frameIndex} is not compiled.");
        ushort duration = list switch
        {
            MapAcquired => 2,
            RoomPlmInstructionLists.SaveStationIdleDraw => 1,
            RoomPlmInstructionLists.SaveStationAnimationFirstFrame or
                RoomPlmInstructionLists.SaveStationAnimationSecondFrame => 4,
            _ => 6,
        };
        ushort firstDraw = list switch
        {
            MapIdle or MapAcquired => RoomPlmStationDrawDefinitions.MapFirst,
            Energy => RoomPlmStationDrawDefinitions.EnergyFirst,
            Missile => RoomPlmStationDrawDefinitions.MissileFirst,
            RoomPlmInstructionLists.SaveStationIdleDraw => RoomPlmStationDrawDefinitions.SaveIdle,
            RoomPlmInstructionLists.SaveStationAnimationFirstFrame => RoomPlmStationDrawDefinitions.SaveActive,
            _ => RoomPlmStationDrawDefinitions.SaveAlternate,
        };
        return new(duration, (ushort)(firstDraw + frameIndex * 12));
    }
}
