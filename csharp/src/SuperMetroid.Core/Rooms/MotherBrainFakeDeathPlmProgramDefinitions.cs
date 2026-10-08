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
            2 => DrawForProgram(address - 2),
            4 => checked((ushort)RoomPlmInstructionCodes.Delete),
            _ => throw new InvalidDataException(
                $"Mother Brain instruction offset {offset} is not a word."),
        };
        return true;
    }

    /// <summary>
    /// Background row 2 through unused F selects thirteen-word horizontal draws
    /// with thirty-byte stride. Wall, door and tube lists select their named draw.
    /// This is called only for aligned, bounded draw operands in AC05..AC88.
    /// </summary>
    private static ushort DrawForProgram(int start)
    {
        if (start >= RoomPlmInstructionLists.MotherBrainsBackgroundRow2 &&
            start < RoomPlmInstructionLists.ClearMotherBrainCeilingBlock)
            return (ushort)(MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRow2 +
                (start - RoomPlmInstructionLists.MotherBrainsBackgroundRow2) / ProgramByteLength * 30);
        return start switch
        {
            RoomPlmInstructionLists.FillMotherBrainsWall => MotherBrainFakeDeathPlmDrawDefinitions.FillWall,
            RoomPlmInstructionLists.MotherBrainsRoomEscapeDoor => MotherBrainFakeDeathPlmDrawDefinitions.EscapeDoor,
            RoomPlmInstructionLists.ClearMotherBrainCeilingBlock => MotherBrainFakeDeathPlmDrawDefinitions.ClearCeilingBlock,
            RoomPlmInstructionLists.ClearMotherBrainCeilingTube => MotherBrainFakeDeathPlmDrawDefinitions.ClearCeilingTube,
            RoomPlmInstructionLists.ClearMotherBrainBottomMiddleSideTube => MotherBrainFakeDeathPlmDrawDefinitions.ClearBottomMiddleSideTube,
            RoomPlmInstructionLists.ClearMotherBrainBottomMiddleTubes => MotherBrainFakeDeathPlmDrawDefinitions.ClearBottomMiddleTubes,
            RoomPlmInstructionLists.ClearMotherBrainBottomLeftTube => MotherBrainFakeDeathPlmDrawDefinitions.ClearBottomLeftTube,
            RoomPlmInstructionLists.ClearMotherBrainBottomRightTube => MotherBrainFakeDeathPlmDrawDefinitions.ClearBottomRightTube,
            _ => throw new InvalidOperationException("Invalid bounded Mother Brain mutation program."),
        };
    }
}
