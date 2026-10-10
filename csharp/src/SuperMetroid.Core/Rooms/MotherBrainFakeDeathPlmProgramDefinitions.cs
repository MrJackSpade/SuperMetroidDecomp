using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Mother Brain fake-death room mutations at $84:AC05..AC88. Every six-byte
/// program draws one physical layout for one frame, then deletes its PLM.
/// The two cartridge-unused background rows are included because they are
/// contiguous authored instruction data, not adjacent machine code.
/// </summary>
internal static class MotherBrainFakeDeathPlmProgramDefinitions
{
    /// <summary><c>$84:AC05</c>: first fake-death room mutation.</summary>
    internal const ushort Start = RoomPlmInstructionLists.FillMotherBrainsWall;
    /// <summary><c>$84:AC89</c>: first byte after the twenty-two lists.</summary>
    internal const ushort EndExclusive = 0xac89;
    /// <summary>Native size of each timer/draw/delete list in bytes.</summary>
    internal const int ProgramByteLength = 6;

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        int offset = address - Start;
        if (offset < 0 || address >= EndExclusive || (offset & 1) != 0)
        {
            value = 0;
            return false;
        }
        value = (offset % ProgramByteLength) switch
        {
            0 => (ushort)1,
            2 => DrawForProgram(ClosedNativeWords.Decode<MotherBrainFakeDeathProgram>(
                (ushort)(address - 2), "Mother Brain fake-death program")),
            4 => checked((ushort)RoomPlmInstruction.Delete),
            _ => throw new InvalidDataException(
                $"Mother Brain instruction offset {offset} is not a word."),
        };
        return true;
    }

    /// <summary>
    /// Background row 2 through unused F selects thirteen-word horizontal draws
    /// with thirty-byte stride. Wall, door and tube lists select their named draw.
    /// </summary>
    private static ushort DrawForProgram(MotherBrainFakeDeathProgram program) => program switch
    {
        MotherBrainFakeDeathProgram.FillWall => MotherBrainFakeDeathPlmDrawDefinitions.FillWall,
        MotherBrainFakeDeathProgram.EscapeDoor => MotherBrainFakeDeathPlmDrawDefinitions.EscapeDoor,
        MotherBrainFakeDeathProgram.BackgroundRow2 or
        MotherBrainFakeDeathProgram.BackgroundRow3 or
        MotherBrainFakeDeathProgram.BackgroundRow4 or
        MotherBrainFakeDeathProgram.BackgroundRow5 or
        MotherBrainFakeDeathProgram.BackgroundRow6 or
        MotherBrainFakeDeathProgram.BackgroundRow7 or
        MotherBrainFakeDeathProgram.BackgroundRow8 or
        MotherBrainFakeDeathProgram.BackgroundRow9 or
        MotherBrainFakeDeathProgram.BackgroundRowA or
        MotherBrainFakeDeathProgram.BackgroundRowB or
        MotherBrainFakeDeathProgram.BackgroundRowC or
        MotherBrainFakeDeathProgram.BackgroundRowD or
        MotherBrainFakeDeathProgram.BackgroundRowE or
        MotherBrainFakeDeathProgram.BackgroundRowF =>
            (ushort)(MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRow2 +
                ((ushort)program - (ushort)MotherBrainFakeDeathProgram.BackgroundRow2) / ProgramByteLength * 30),
        MotherBrainFakeDeathProgram.ClearCeilingBlock => MotherBrainFakeDeathPlmDrawDefinitions.ClearCeilingBlock,
        MotherBrainFakeDeathProgram.ClearCeilingTube => MotherBrainFakeDeathPlmDrawDefinitions.ClearCeilingTube,
        MotherBrainFakeDeathProgram.ClearBottomMiddleSideTube => MotherBrainFakeDeathPlmDrawDefinitions.ClearBottomMiddleSideTube,
        MotherBrainFakeDeathProgram.ClearBottomMiddleTubes => MotherBrainFakeDeathPlmDrawDefinitions.ClearBottomMiddleTubes,
        MotherBrainFakeDeathProgram.ClearBottomLeftTube => MotherBrainFakeDeathPlmDrawDefinitions.ClearBottomLeftTube,
        MotherBrainFakeDeathProgram.ClearBottomRightTube => MotherBrainFakeDeathPlmDrawDefinitions.ClearBottomRightTube,
        _ => throw new InvalidOperationException($"Undefined MotherBrainFakeDeathProgram {program}."),
    };
}

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
