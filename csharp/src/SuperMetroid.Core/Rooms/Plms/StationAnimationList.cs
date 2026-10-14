namespace SuperMetroid.Core.Rooms.Plms;

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
