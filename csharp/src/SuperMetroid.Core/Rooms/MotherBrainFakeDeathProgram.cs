namespace SuperMetroid.Core.Rooms;

/// <summary>The twenty-two contiguous fake-death mutation lists, valued by their $84 address.</summary>
internal enum MotherBrainFakeDeathProgram : ushort
{
    /// <inheritdoc cref="RoomPlmInstructionLists.FillMotherBrainsWall"/>
    FillWall = RoomPlmInstructionLists.FillMotherBrainsWall,
    /// <inheritdoc cref="RoomPlmInstructionLists.MotherBrainsRoomEscapeDoor"/>
    EscapeDoor = RoomPlmInstructionLists.MotherBrainsRoomEscapeDoor,
    /// <inheritdoc cref="RoomPlmInstructionLists.MotherBrainsBackgroundRow2"/>
    BackgroundRow2 = RoomPlmInstructionLists.MotherBrainsBackgroundRow2,
    /// <inheritdoc cref="RoomPlmInstructionLists.MotherBrainsBackgroundRow3"/>
    BackgroundRow3 = RoomPlmInstructionLists.MotherBrainsBackgroundRow3,
    /// <inheritdoc cref="RoomPlmInstructionLists.MotherBrainsBackgroundRow4"/>
    BackgroundRow4 = RoomPlmInstructionLists.MotherBrainsBackgroundRow4,
    /// <inheritdoc cref="RoomPlmInstructionLists.MotherBrainsBackgroundRow5"/>
    BackgroundRow5 = RoomPlmInstructionLists.MotherBrainsBackgroundRow5,
    /// <inheritdoc cref="RoomPlmInstructionLists.MotherBrainsBackgroundRow6"/>
    BackgroundRow6 = RoomPlmInstructionLists.MotherBrainsBackgroundRow6,
    /// <inheritdoc cref="RoomPlmInstructionLists.MotherBrainsBackgroundRow7"/>
    BackgroundRow7 = RoomPlmInstructionLists.MotherBrainsBackgroundRow7,
    /// <inheritdoc cref="RoomPlmInstructionLists.MotherBrainsBackgroundRow8"/>
    BackgroundRow8 = RoomPlmInstructionLists.MotherBrainsBackgroundRow8,
    /// <inheritdoc cref="RoomPlmInstructionLists.MotherBrainsBackgroundRow9"/>
    BackgroundRow9 = RoomPlmInstructionLists.MotherBrainsBackgroundRow9,
    /// <inheritdoc cref="RoomPlmInstructionLists.MotherBrainsBackgroundRowA"/>
    BackgroundRowA = RoomPlmInstructionLists.MotherBrainsBackgroundRowA,
    /// <inheritdoc cref="RoomPlmInstructionLists.MotherBrainsBackgroundRowB"/>
    BackgroundRowB = RoomPlmInstructionLists.MotherBrainsBackgroundRowB,
    /// <inheritdoc cref="RoomPlmInstructionLists.MotherBrainsBackgroundRowC"/>
    BackgroundRowC = RoomPlmInstructionLists.MotherBrainsBackgroundRowC,
    /// <inheritdoc cref="RoomPlmInstructionLists.MotherBrainsBackgroundRowD"/>
    BackgroundRowD = RoomPlmInstructionLists.MotherBrainsBackgroundRowD,
    /// <summary>$84:AC59, cartridge-unused background row E.</summary>
    BackgroundRowE = 0xac59,
    /// <summary>$84:AC5F, cartridge-unused background row F.</summary>
    BackgroundRowF = 0xac5f,
    /// <inheritdoc cref="RoomPlmInstructionLists.ClearMotherBrainCeilingBlock"/>
    ClearCeilingBlock = RoomPlmInstructionLists.ClearMotherBrainCeilingBlock,
    /// <inheritdoc cref="RoomPlmInstructionLists.ClearMotherBrainCeilingTube"/>
    ClearCeilingTube = RoomPlmInstructionLists.ClearMotherBrainCeilingTube,
    /// <inheritdoc cref="RoomPlmInstructionLists.ClearMotherBrainBottomMiddleSideTube"/>
    ClearBottomMiddleSideTube = RoomPlmInstructionLists.ClearMotherBrainBottomMiddleSideTube,
    /// <inheritdoc cref="RoomPlmInstructionLists.ClearMotherBrainBottomMiddleTubes"/>
    ClearBottomMiddleTubes = RoomPlmInstructionLists.ClearMotherBrainBottomMiddleTubes,
    /// <inheritdoc cref="RoomPlmInstructionLists.ClearMotherBrainBottomLeftTube"/>
    ClearBottomLeftTube = RoomPlmInstructionLists.ClearMotherBrainBottomLeftTube,
    /// <inheritdoc cref="RoomPlmInstructionLists.ClearMotherBrainBottomRightTube"/>
    ClearBottomRightTube = RoomPlmInstructionLists.ClearMotherBrainBottomRightTube,
}
