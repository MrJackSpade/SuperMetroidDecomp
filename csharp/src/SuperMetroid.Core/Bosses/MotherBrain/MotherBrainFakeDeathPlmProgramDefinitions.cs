using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

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

    /// <summary>Reads only aligned timer, draw, or delete words within the twenty-two six-byte mutation lists.</summary>
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
        MotherBrainFakeDeathProgram.FillWall => (ushort)MotherBrainFakeDeathDraw.FillWall,
        MotherBrainFakeDeathProgram.EscapeDoor => (ushort)MotherBrainFakeDeathDraw.EscapeDoor,
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
            (ushort)((ushort)MotherBrainFakeDeathDraw.BackgroundRow2 +
                ((ushort)program - (ushort)MotherBrainFakeDeathProgram.BackgroundRow2) / ProgramByteLength * 30),
        MotherBrainFakeDeathProgram.ClearCeilingBlock => (ushort)MotherBrainFakeDeathDraw.ClearCeilingBlock,
        MotherBrainFakeDeathProgram.ClearCeilingTube => (ushort)MotherBrainFakeDeathDraw.ClearCeilingTube,
        MotherBrainFakeDeathProgram.ClearBottomMiddleSideTube => (ushort)MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube,
        MotherBrainFakeDeathProgram.ClearBottomMiddleTubes => (ushort)MotherBrainFakeDeathDraw.ClearBottomMiddleTubes,
        MotherBrainFakeDeathProgram.ClearBottomLeftTube => (ushort)MotherBrainFakeDeathDraw.ClearBottomLeftTube,
        MotherBrainFakeDeathProgram.ClearBottomRightTube => (ushort)MotherBrainFakeDeathDraw.ClearBottomRightTube,
        _ => throw new InvalidOperationException($"Undefined MotherBrainFakeDeathProgram {program}."),
    };
}
